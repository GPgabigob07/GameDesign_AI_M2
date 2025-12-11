using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Misc;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace Map
{
    public class MapGenerator : MonoBehaviour
    {
        public bool delay, debug, debugTerrain, debugForest, debugStreet;
        public SceneConfiguration sceneConfig;

        [Header("Map")] public int height = 101;

        public int width = 101,
            centerSize = 5;

        [Header("Forest")] [SerializeField] private float forestNoiseScale = 3f;
        [SerializeField] private float forestThreshold = 0.55f;
        [SerializeField] private int forestRadius = 10;
        [SerializeField] private int forestNeighborDistance = 2;
        [SerializeField] private int maxForestCount = 5;
        [SerializeField] private int minForestCount = 3;
        [SerializeField] private int minStructuresPercent = 20;
        [SerializeField] private int minDistanceFromCenter = 50;

        private bool areForestValid;

        private int[,] forest;
        private List<Forest> forests;

        [Header("Road")] public int minStreetCount = 3;

        public int lineWidth = 5,
            maxStreetCount = 7,
            linearDistanceFromExits = 30,
            lineSize = 5;

        private List<Street> streets;

        [Header("Preview")] [SerializeField] private Tilemap preview;

        [SerializeField] private TileBase plainGrass, rockyGrass, wetGrass, street, forestTile;
        
        public int[,] terrainMask, streetMask;
        private bool[,] forestMask, forestAreaMask;
        private float[,] heat, moisture;
        private List<Vector2> forestsCenters;

        private WaitForSeconds wait = new(0.005f);
        private Coroutine coroutine;
        public bool enableStep, step;
        private StreetPathBuilder limbGenerator;
        private MapIndexer indexer;

        private void OnValidate()
        {
            if (height % 2 == 0) height++;
            if (width % 2 == 0) width++;
            if (minStreetCount % 2 == 0) minStreetCount++;
            if (lineWidth % 2 == 0) lineWidth++;
            if (lineSize % 2 == 0) lineSize++;
        }

        private void CreateMasks()
        {
            terrainMask = new int[width, height];
            forestMask = new bool[width, height];
            forestAreaMask = new bool[width, height];
            streetMask = new int[width, height];

            streets = new();

            heat = GenerateNoise(width, height, 2f, Random.value * 999f, Random.value * 999f);
            moisture = GenerateNoise(width, height, 2f, Random.value * 999f, Random.value * 999f);

            for (var j = 0; j < width; j++)
            for (var i = 0; i < height; i++)
            {
                terrainMask[j, i] = 0;
                streetMask[j, i] = 0;
            }

            indexer = new(width, height);
        }

        private float[,] GenerateNoise(int w, int h, float scale, float offsetX, float offsetY)
        {
            var map = new float[w, h];

            for (var y = 0; y < h; y++)
            for (var x = 0; x < w; x++)
            {
                var nx = (float)x / w * scale + offsetX;
                var ny = (float)y / h * scale + offsetY;

                map[x, y] = Mathf.PerlinNoise(nx, ny);
            }

            return map;
        }

        [ContextMenu("Generate Map")]
        private void Generate()
        {
            //create masks
            CreateMasks();
            StopAllCoroutines();
            StartCoroutine(GenerateInternal());
        }

        private bool processing;
        private IEnumerator GenerateInternal()
        {
            processing = true;
            //generate terrain using perlin
            yield return GenerateTerrain();

            //generate forest using weighted noise + destruction prune
            yield return GenerateForestsHybrid();

            //generate streets with target drunk walk
            yield return GenerateStreets();

            PreBakeAndPreview();
            processing = false;
        }

        #region Preview & Bake
        private void PreBakeAndPreview()
        {
            preview.ClearAllTiles();
            forestsCenters = new List<Vector2>();
            ClearMasks();

            
            foreach (var f in forests)
            {
                foreach (var str in f.structures)
                {
                    var bound = f.Resolve(str);
                    forestsCenters.Add(bound.center);
                    foreach (var point in bound.Points())
                    {
                        var (i, j) = indexer.PointToIndex(point.ToInt());
                        forestMask[i, j] = true;
                    }
                }
            }

            var center = new Bounds(Vector3.zero, Vector3.one * centerSize);
            foreach (var p in center.Points())
            {
                var (i, j) = indexer.PointToIndex(p.ToInt());
                streetMask[i, j] = 1;
            }

            foreach (var street in streets)
            {
                foreach (var streetPath in street.paths)
                {
                    var (i, j) = indexer.PointToIndex(streetPath);
                    streetMask[i, j] = 1;
                }
            }

            DrawPreview();
            //serialize to disk to be read once the game scene is loaded
        }

        private void ClearMasks()
        {
            forestMask = new bool[width, height];
            forestAreaMask = new bool[width, height];
            streetMask = new int[width, height];
        }

        private void DrawPreview()
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    var point = indexer.IndexToPoint(i, j);

                    if (preview.GetTile(point.To3D()))
                        continue;

                    if (streetMask[i, j] > 0)
                    {
                        preview.SetTile(point.To3D(), street);
                        continue;
                    }

                    if (forestMask[i, j])
                    {
                        preview.SetTile(point.To3D(), forestTile);
                        continue;
                    }

                    var biome = (Biome)terrainMask[i, j];
                    preview.SetTile(point.To3D(), biome switch
                    {
                        Biome.PlainGrass => plainGrass,
                        Biome.RockyGrass => rockyGrass,
                        Biome.WetGrass => wetGrass,
                        _ => throw new ArgumentOutOfRangeException()
                    });
                }
            }
        }

        public void SaveCurrentMapAndProceed()
        {
            if (processing) return;
            
            var terrainBytes = MapStorage.WriteBytes(terrainMask);
            var streetBytes = MapStorage.WriteBytes(streetMask);

            var bakeData = new MapBakeData
            {
                Width = width,
                Height = height,
                ForestCenters = forestsCenters.ToArray(),
                StreetMask = streetBytes,
                TerrainMask = terrainBytes,
            };
            
            if (!MapStorage.SaveBakeToDisk(bakeData))
                return;
            
            SceneNavigator.ToMainGame();
        }

        public void Recreate()
        {
            Generate();
        }

        #endregion
        private void Update()
        {
            if (enableStep && limbGenerator != null)
            {
                limbGenerator.stepped = step;
                step = false;
            }
    
            if (processing)//very inefficient, but this scenario might be fine
                PreBakeAndPreview();
        }

        #region Street Generation
        private IEnumerator GenerateStreets()
        {
            var count = Random.Range(minStreetCount, maxStreetCount);
            streets = new List<Street>();
            var hl = width / 2;
            var hh = height / 2;
            var points = new Vector2Int[count];

            limbGenerator = new StreetPathBuilder(width, height, forestAreaMask, lineSize, centerSize, Vector2Int.zero,
                0.5f, wait);

            limbGenerator.enableStep = enableStep;

            for (var p = 0; p < count; p++)
            {
                points[p] = default(Vector2Int);
                do
                {
                    //use x-axis as pivot
                    if (Random.value < 0.5f)
                    {
                        var x = (hl - Random.value * width).Floor();
                        var y = Random.value < 0.5f ? -hh : hh - 1;
                        points[p] = new Vector2Int(x, y);
                    }
                    else //use y-axsis as pivot
                    {
                        var x = Random.value < 0.5f ? -hl : hl - 1;
                        var y = (hh - Random.value * height).Floor();
                        points[p] = new Vector2Int(x, y);
                    }
                } while (!EnsureStreetExitsDistance(points, p));
            }

            foreach (var target in points)
            {
                var s = new Street
                {
                    target = target,
                    paths = new()
                };

                streets.Add(s);
                yield return limbGenerator.BuildPath(s, debug);
            }

            //yield return new WaitUntil(() => streets.All(e => e.done));
        }

        private bool EnsureStreetExitsDistance(Vector2Int[] points, int i)
        {
            var actual = points[i];
            if (actual == default)
                return false;

            foreach (var p in points)
            {
                if (p == actual)
                    continue;

                int dx = Mathf.Abs(actual.x - p.x);
                int dy = Mathf.Abs(actual.y - p.y);

                if (dx < linearDistanceFromExits || dy < linearDistanceFromExits)
                    return false;
            }

            return true;
        }

        #endregion
        
        #region Forest Generation

        private IEnumerator GenerateForestsHybrid()
        {
            areForestValid = true;
            //reserve areas
            yield return ReserveForests();

            yield return ValidateForests();

            if (!areForestValid)
            {
                yield return GenerateForestsHybrid();
                yield break;
            }

            foreach (var f1 in forests)
            {
                yield return EnsureForestSpacing(f1);
            }

            foreach (var forest1 in forests)
            {
                foreach (var b in forest1.bounds.Points())
                {
                    var (i, j) = indexer.PointToIndex(b.ToInt());
                    forestAreaMask[i, j] = true;
                }
            }
        }

        private IEnumerator EnsureForestSpacing(Forest f1)
        {
            var toRemove = new List<Vector2Int>();

            for (int s1 = 0; s1 < f1.structures.Count; s1++)
            {
                var str1 = f1.structures[s1];

                if (!IsValidStructurePosition(str1, f1.structures))
                {
                    toRemove.Add(str1);
                    continue;
                }

                if (!f1.bounds.Contains(str1.ToFloat()))
                {
                    toRemove.Add(str1);
                    continue;
                }

                var bounds = f1.Resolve(str1);
                for (var s2 = 0; s2 < f1.structures.Count; s2++)
                {
                    if (s1 == s2) continue;

                    var str2 = f1.structures[s2];
                    if (toRemove.Contains(str2))
                        continue;

                    var bounds2 = f1.Resolve(str2);

                    if (bounds2.Intersects(bounds))
                    {
                        toRemove.Add(str2);
                    }
                }
            }

            foreach (var p in toRemove)
            {
                if (delay)
                    yield return wait;

                f1.structures.Remove(p);
            }
        }

        private bool IsValidStructurePosition(Vector2Int newPoint, List<Vector2Int> existingPoints)
        {
            foreach (var existing in existingPoints)
            {
                if (newPoint == existing)
                    continue; //skip self

                var dx = Mathf.Abs(existing.x - newPoint.x);
                var dy = Mathf.Abs(existing.y - newPoint.y);

                //too close vertically
                if (dx == 0 && dy <= forestNeighborDistance)
                    return false;

                //too close horizontally
                if (dy == 0 && dx <= forestNeighborDistance)
                    return false;

                //too close diagonally
                if (dx == forestNeighborDistance / 2 && dy == forestNeighborDistance / 2)
                    return false;
            }

            return true;
        }

        private IEnumerator ReserveForests()
        {
            var amount = Random.Range(minForestCount, maxForestCount);
            forests = new List<Forest>();

            for (int c = 0; c < amount; c++)
            {
                var f = new Forest();

                var x = Random.Range(forestRadius, width - forestRadius) - width / 2;
                var y = Random.Range(forestRadius, height - forestRadius) - height / 2;

                f.bounds = new Bounds(new Vector3(x, y), new Vector3(forestRadius, forestRadius));
                f.structures = new();

                var last = new Vector2Int(int.MaxValue, int.MaxValue);
                foreach (var point in f.bounds.Points())
                {
                    var nx = (float)x / width * forestNoiseScale + Random.value * 999f;
                    var ny = (float)y / height * forestNoiseScale + Random.value * 999f;

                    if (Mathf.PerlinNoise(nx, ny) > forestThreshold)
                    {
                        var p = point.ToInt();
                        if (Vector2.Distance(last, p) >= forestNeighborDistance) //basic separation
                            f.structures.Add(p);

                        last = p;
                    }
                }

                forests.Add(f);
                if (delay)
                    yield return wait;
            }
        }

        private IEnumerator ValidateForests()
        {
            var minDistanceFromOtherForest = (forestRadius) * Mathf.Sqrt(2f);
            areForestValid = false;
            if (forests.Count < minForestCount)
                yield break;

            for (var i = 0; i < forests.Count; i++)
            {
                var f1 = forests[i];

                //not clip center
                if (!IsFarFromCenter(f1))
                {
                    Debug.Log("Killed forests, clipping center");
                    yield break;
                }

                //allow for streets and monkeys to not get stuck between two+ forests
                for (var j = i + 1; j < forests.Count; j++)
                {
                    if (Vector2.Distance((forests[j].bounds.center), f1.bounds.center) < minDistanceFromOtherForest)
                    {
                        Debug.Log($"Killed forests, not far enough from each other: {minDistanceFromOtherForest}");
                        yield break;
                    }
                }
            }

            areForestValid = true;
        }

        private bool IsFarFromCenter(Forest f1)
        {
            var b = f1.bounds.center;
            return Vector3.Distance(b, Vector3.zero) >= minDistanceFromCenter;
        }

        #endregion

        #region Terrain Generation

        private IEnumerator GenerateTerrain()
        {
            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
                    terrainMask[i, j] = (int)Classify(i, j);
                }

                if (delay)
                    yield return wait;
            }
        }

        private Biome Classify(int x, int y)
        {
            return moisture[x, y] switch
            {
                > 0.75f => Biome.WetGrass,
                < 0.3f when heat[x, y] > 0.30f => Biome.RockyGrass,
                _ => Biome.PlainGrass
            };
        }

        #endregion

        #region Gizmos

        private void OnDrawGizmosSelected()
        {
            if (!debug) return;

            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
                    if (terrainMask != null && debugTerrain)
                        DrawTerrainMaskGizmos(i, j);
                }
            }

            if (streets != null && debugStreet)
                DrawStreetGizmos();

            if (debugForest && forests != null)
                DrawForestGizmos();

            if (debugStreet)
                DrawCenterGizmos();
        }

        private void DrawCenterGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawCube(new Vector3(0, 0, 10), new Vector3(centerSize, centerSize));
        }

        private void DrawForestGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(Vector2.zero, new Vector2(width - forestRadius, height - forestRadius));

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(new Vector3(), new(minDistanceFromCenter, minDistanceFromCenter));

            foreach (var f in forests)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(f.bounds.center, f.bounds.size);

                var anchor = new Color(0.5f, 1, 0.5f, 1);
                var area = Color.red;

                foreach (var str in f.structures)
                {
                    Gizmos.color = anchor;
                    Gizmos.DrawCube(str.To3D(), Vector3.one);
                    Gizmos.color = area;
                    Gizmos.DrawWireCube(str.To3D() + new Vector3(0.5F, 0.5F, -1.5f), Vector2.one * 2);
                }
            }

            /*point = indexer.IndexToPoint(i, i1+1).To3D();
            point.z = 2;
            Gizmos.DrawCube(point, Vector3.one);

            point = indexer.IndexToPoint(i+1, i1+1).To3D();
            point.z = 2;
            Gizmos.DrawCube(point, Vector3.one);

            point = indexer.IndexToPoint(i+1, i1).To3D();
            point.z = 2;
            Gizmos.DrawCube(point, Vector3.one);*/
        }

        private void DrawStreetGizmos()
        {
            foreach (var street in streets)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawCube(street.target, Vector3.one);

                Gizmos.color = Color.magenta;
                foreach (var streetPath in street.paths)
                {
                    Gizmos.DrawCube(streetPath.To3D(), Vector2.one);
                }
            }
        }

        private void DrawTerrainMaskGizmos(int i, int i1)
        {
            var t = terrainMask[i, i1] / 256f * 100f;
            Gizmos.color = new(t, t, t);
            var p = indexer.IndexToPoint(i, i1).To3D();
            p.z = -10;
            Gizmos.DrawCube(p, Vector3.one);
        }

        #endregion
    }
    
    public enum Biome
    {
        PlainGrass,
        RockyGrass,
        WetGrass,
    }

    public enum PathDirection
    {
        East,
        NEast,
        North,
        NWest,
        West,
        SWest,
        South,
        SEast
    }

    [Serializable]
    public struct Forest
    {
        public Bounds bounds;
        public List<Vector2Int> structures;

        public Bounds Resolve(Vector2Int str)
        {
            return new Bounds(str + new Vector2(0.5f, 0.5f), Vector2.one);
        }
    }

    [Serializable]
    public struct Street
    {
        public Vector2 target;
        public List<Vector2Int> paths;
        public Stack<(Vector2Int pos, PathDirection dir)> history;
        public bool done;
    }

    public static class PDHelper
    {
        public static Vector2Int Vector(this PathDirection direction)
        {
            return (int)direction switch
            {
                0 => new(1, 0), // → East
                1 => new(1, 1), // → NE
                2 => new(0, 1), // ↑ North
                3 => new(-1, 1), // ← NW
                4 => new(-1, 0), // ← West
                5 => new(-1, -1), // ↙ SW
                6 => new(0, -1), // ↓ South
                7 => new(1, -1),
                _ => default // ↘ SE
            };
        }

        public static Vector2 VectorF(this PathDirection direction)
        {
            return direction.Vector().ToFloat();
        }

        public static PathDirection Right(this PathDirection direction)
        {
            return (PathDirection)(((int)direction + 1 + 8) % 8);
        }

        public static PathDirection Left(this PathDirection direction)
        {
            return (PathDirection)(((int)direction - 1 + 8) % 8);
        }

        public static PathDirection Opposite(this PathDirection direction)
        {
            return (PathDirection)(((int)direction + 4) % 8);
        }

        public static PathDirection Rotate(this PathDirection direction, int steps)
        {
            return (PathDirection)(((int)direction + steps) % 8);
        }

        public static bool IsDiagonalOf(this PathDirection direction, PathDirection other)
        {
            return direction.Left() == other || direction.Right() == other;
        }


        public static int Diff(this PathDirection direction, PathDirection other)
        {
            return (direction - other) % 8;
        }

        public static PathDirection ToDirection(this Vector2 v)
        {
            var x = Mathf.RoundToInt(Mathf.Sign(v.x));
            var y = Mathf.RoundToInt(Mathf.Sign(v.y));

            var direction = PathDirection.North;
            while (direction.Vector() != new Vector2Int(x, y))
                direction = direction.Right();

            return direction;
        }
    }
}