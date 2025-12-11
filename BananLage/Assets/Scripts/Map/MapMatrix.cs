using System;
using System.Collections.Generic;
using System.Linq;
using Structures;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TerrainUtils;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace Map
{
    public class MapMatrix
    {
        private static MapMatrix _instance;
        private readonly MapBakeData bakeData;

        private readonly int width, height;
        private readonly int[,] terrainMask, streetMask;
        private readonly bool[,] structureMask;

        private readonly MapIndexer indexer;

        private static Vector2[] forestPosFix =
        {
            PathDirection.North.VectorF() / 2f,
            PathDirection.East.VectorF() / 2f,
            PathDirection.South.VectorF() / 2f,
            PathDirection.West.VectorF() / 2f,
        };

        private static PathDirection[] neighbors = (PathDirection[])typeof(PathDirection).GetEnumValues();
        
        /*private static PathDirection[] neighbors =
        {
            PathDirection.North,
            PathDirection.East,
            PathDirection.South,
            PathDirection.West
        };*/

        public MapMatrix(MapBakeData bakeData)
        {
            if (_instance != null)
                throw new Exception("MapMatrix already created");

            this.bakeData = bakeData;
            (width, height, terrainMask, streetMask) = this.bakeData;

            structureMask = new bool[width, height];
            indexer = new MapIndexer(width, height);
           // BuildForestMask();
            _instance = this;
        }

        private void BuildForestMask()
        {
            foreach (var centers in bakeData.ForestCenters)
            {
                foreach (var vector2 in forestPosFix)
                {
                    var effective = centers + vector2;
                    var (i, j) = indexer.PointToIndex(effective.ToInt());
                    structureMask[i, j] = true;
                }
            }
        }

        public bool CanPlaceAt(Vector2Int point)
        {
            var (i, j) = indexer.PointToIndex(point);
            return !structureMask[i, j] && streetMask[i, j] <= 0;
        }

        public int PointWeight(Vector2Int point, bool enemy = false, bool allowStructure = false)
        {
            var (i, j) = indexer.PointToIndex(point);

            if (!IsCenter(i, j) && structureMask[i, j])
                return allowStructure ? 0 : int.MaxValue;

            var s = streetMask[i, j];
            if (enemy)
            {
                return s <= 0 ? int.MaxValue : s;
            }

            return s > 0 ? s : (terrainMask[i, j] + 1) * 3;
        }

        private bool IsCenter(int i, int j)
        {
            var hw = width / 2;
            var hh = height / 2;
            
            return i >= hw -1 && i < hw + 1 && j >= hh - 1 && j < hh + 1;
        }

        public IEnumerable<Vector2Int> GetNeighbors(Vector2Int point)
        {
            return neighbors.Select(e => point + e.Vector());
        }

        public void PlaceStructure(Bounds bounds, bool fromBuild = false)
        {
            if (fromBuild) return;
            
            foreach (var point in bounds.Points())
            {
                var (i, j) = indexer.PointToIndex(point.ToInt());
                if (!fromBuild && structureMask[i, j])
                    throw new ArgumentException(); //break it!

                structureMask[i, j] = true;
            }
        }

        public void Render(Tilemap terrain, Tilemap streets, Func<BaseStructure> forest,
            Dictionary<Biome, TileBase[]> biomeTiles, TileBase streetTile
        )
        {
            terrain.ClearAllTiles();
            streets.ClearAllTiles();

            foreach (var center in bakeData.ForestCenters)
            {
                var f = forest();
                f.transform.position = center;
            }

            for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
            {
                var point = indexer.IndexToPoint(x, y).To3D();

                var biome = (Biome)terrainMask[x, y];

                var tile = biomeTiles[biome].RandomEntry();
                terrain.SetTile(point, tile);

                if (streetMask[x, y] > 0)
                {
                    streets.SetTile(point, streetTile);
                    var rotation = Quaternion.Euler(0f, 0f, Random.value switch
                    {
                        < 0.25f => -90,
                        < 0.5f => 90,
                        < 0.75f => 180,
                        _ => 0f
                    });

                    var newMatrix = Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one);
                    streets.SetTransformMatrix(point, newMatrix);
                }
            }
        }

        public static MapMatrix GetInstance()
        {
            Assert.IsNotNull(_instance);
            return _instance;
        }

        public bool IsValidArea(Vector2Int pos)
        {
            return indexer.IsValidPoint(pos);
        }

        public Bounds RandomPointFarFromCenter()
        {
            var wx = Random.Range(20, width);
            var hy = Random.Range(20, height);
            wx = Random.value < 0.5 ? wx : -wx;
            hy = Random.value < 0.5 ? hy : -hy;
            
            return new Bounds(new Vector3(wx, hy, 0), Vector2.one);
        }
    }
}