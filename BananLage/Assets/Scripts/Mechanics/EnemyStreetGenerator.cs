using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace Mechanics
{
    public class EnemyStreetGenerator : MonoBehaviour
    {
        public int minLine = 3, minLineWidth = 3, size = 100, centerSize = 5;
        public bool top, left, right, bottom, auto = true;
        public Vector3Int center;
        public float gridSize = 1;

        public Tilemap tilemap;
        public TileBase tile;

        private int currentL, currentLR;

        private void Generate(bool clear = true, bool force = false)
        {
            if (centerSize % 2 == 0) centerSize++;
            if (minLine % 2 == 0) minLine++;

            if (minLineWidth % 2 == 0) minLineWidth++;
            if (minLineWidth == 0) minLineWidth = 2;
            if (minLineWidth >= centerSize) minLineWidth = centerSize - 2;

            center = -new Vector3Int(centerSize / 2, centerSize / 2, 0);
            if (clear)
            {
                tilemap.ClearAllTiles();
                for (int i = 0; i < centerSize; i++)
                {
                    for (int j = 0; j < centerSize; j++)
                    {
                        var pos = new Vector3Int(i + center.x, j + center.y);
                        tilemap.SetTile(pos, tile);
                    }
                }

                if (force) return;
            }

            if (right) GenerateToDirection(Vector2Int.right, size * minLine);
            if (left) GenerateToDirection(Vector2Int.left, size * minLine);
            if (top) GenerateToDirection(Vector2Int.up, size * minLine);
            if (bottom) GenerateToDirection(Vector2Int.down, size * minLine);

            /*if (right)
            {
                var dir = new Vector3Int(1, 0);
                var upI = new Vector3Int(dir.y, -dir.x);
                var downI = new Vector3Int(-dir.y, dir.x);

                var halfLine = minLineWidth / 2;
                var halfCenter = centerSize / 2;

                var forwardI = dir;

                var lineStepStart = dir * halfCenter + downI * halfLine;

                var next = lineStepStart;

                var steps = size;
                var rightTurns = 0;
                var leftTurns = 0;
                if (!auto)
                {
                    next += forwardI * currentL;
                    for (var lr = 1; lr <= minLineWidth; lr++)
                    {
                        tilemap.SetTile(next + upI * lr, tile);
                    }

                    if (currentLR == minLineWidth)
                    {
                        currentLR = 0;
                        forwardI = left ? new Vector3Int(forwardI.y, -forwardI.x) : new Vector3Int(-forwardI.y, forwardI.x);
                        upI = new Vector3Int(forwardI.y, -forwardI.x);
                        downI = new Vector3Int(-forwardI.y, forwardI.x);
                    }

                    return;
                }

                
                while (steps-- > 0)
                {
                    for (var l = 0; l < minLineWidth; l++)
                    {
                        for (var lr = 1; lr <= minLineWidth; lr++)
                        {
                            tilemap.SetTile(next + upI * lr, tile);
                        }

                        next += forwardI;
                    }

                    var left = Random.value < .5f;
                    if (left && leftTurns++ > 2)
                    {
                        leftTurns = 0;
                        left = false;
                    }
                    else if (!left && rightTurns++ > 2)
                    {
                        rightTurns = 0;
                        left = true;
                    }

                    forwardI = left ? new Vector3Int(forwardI.y, -forwardI.x) : new Vector3Int(-forwardI.y, forwardI.x);
                    upI = new Vector3Int(forwardI.y, -forwardI.x);
                    downI = new Vector3Int(-forwardI.y, forwardI.x);
                }
            }*/
        }

        [ContextMenu("Next")]
        private void _Next()
        {
            auto = false;
            currentL++;
            currentLR++;
            Generate(clear: false);
            auto = true;
        }

        [ContextMenu("Generate")]
        private void _Generate()
        {
            currentL = 0;
            currentLR = 0;
            Generate();
        }

        [ContextMenu("Clear")]
        private void _Clear()
        {
            Generate(force: true);
        }

        private void GenerateToDirection(Vector2Int direction, int leftTurns = 0, int rightTurns = 0, Vector2Int lastPoint = new())
        {
            var left = new Vector2Int(-direction.y, direction.x);
            var right = new Vector2Int(direction.y, -direction.x);
            
            for (var i = 0; i < minLine; i++)
            {
                if (Vector2Int.Distance(direction, lastPoint) >= size) break;
                for (var l = 0; l < minLineWidth; l++)
                {
                    var fill = left + (right * l);
                    var pos = lastPoint + fill;
                    tilemap.SetTile(new Vector3Int(pos.x, pos.y), tile);
                }

                lastPoint += direction;
            }

            if (Vector2Int.Distance(direction, lastPoint) >= size) return;

            var toLeft = Random.value < 0.5f;

            if (toLeft && leftTurns == 0 )
            {
                GenerateToDirection(left, leftTurns + 1, 0, lastPoint);
            }
            else if (rightTurns == 0)
            {
                GenerateToDirection(right,  0, rightTurns + 1, lastPoint);
            }
            else
            {
                GenerateToDirection(direction,  0, 0, lastPoint);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
        }
    }
}