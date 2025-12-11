using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Map;
using Mechanics.PathFinding;
using Mechanics.Village;
using UnityEngine;

namespace Mechanics
{
    public class PathManager : MonoBehaviour
    {
        private static PathManager _instance;

        [SerializeField] private float thinkingTime = 0.05f;
        [SerializeField] private bool forceDelay;
        private void Awake()
        {
            if (!_instance)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }


        private IEnumerator FindPath(Vector2Int start, Bounds acceptableArea, PathAgent agent)
        {
            var map = MapMatrix.GetInstance();
            var open = agent.queue = new();
            var allNodes = new Dictionary<Vector2Int, PathNode>();
            var closed = new HashSet<Vector2Int>();

            var acceptablePoints = acceptableArea.Points().ToList();
            var end = acceptablePoints.RandomEntry().ToInt();

            var startNode = GetOrCreateNode(start, allNodes);
            startNode.G = 0;
            startNode.H = Heuristic(start, end);

            open.Enqueue(startNode, startNode.F);
            var closest = float.MaxValue;

            while (open.Count > 0)
            {
                var current = open.Dequeue();
                if (current != allNodes[current.Position])
                    continue;

                if (!map.IsValidArea(current.Position)) continue;

                if (acceptableArea.Contains(current.Position.To3D()))
                {
                    if (!VillageManager.ImmediateResolve)
                        yield return new WaitForSeconds(thinkingTime);
                    agent.NewPath(BuildPath(current));
                    agent.Fetching = false;
                    yield break;
                }

                closed.Add(current.Position);
                var p = current.Position;
                var neighboars = map.GetNeighbors(p);
                foreach (var pos in neighboars)
                {
                    if (closed.Contains(pos)) continue;

                    var neighbor = GetOrCreateNode(pos, allNodes);

                    if (pos == end)
                        Debug.Log($"Found the damn thing!");

                    var tentativeG = current.G + map.PointWeight(neighbor.Position,
                        allowStructure: acceptableArea.Contains(pos.To3D()));

                    var isBetter = tentativeG < neighbor.G;

                    if (isBetter)
                    {
                        neighbor.Parent = current;
                        neighbor.G = tentativeG;
                        neighbor.H = Heuristic(pos, end);

                        open.Enqueue(neighbor, neighbor.F);
                    }
                }

                if (forceDelay)
                    yield return null;
            }
        }

        private int Heuristic(Vector2Int a, Vector2Int b)
            => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y); // Manhattan

        private PathNode GetOrCreateNode(Vector2Int pos, Dictionary<Vector2Int, PathNode> dict)
        {
            if (!dict.TryGetValue(pos, out var node))
            {
                node = new PathNode(pos)
                {
                    G = 99999 // "infinity"
                };
                dict[pos] = node;
            }

            return node;
        }

        private List<Vector2Int> BuildPath(PathNode node)
        {
            var list = new List<Vector2Int>();
            while (node != null)
            {
                list.Add(node.Position);
                node = node.Parent;
            }

            list.Reverse();
            return list;
        }

        public static void FindPathAsync(Vector2 from, Bounds acceptableArea, PathAgent agent)
        {
            if (agent.Fetching) return;
            agent.Fetching = true;
            agent.targetArea = acceptableArea;
            agent.StartCoroutine(_instance.FindPath(from.ToInt(), acceptableArea, agent));
        }
    }

    public class PathNode
    {
        public Vector2Int Position;
        public long G; // distance from start
        public long H; // heuristic distance to target
        public long F => G + H;

        public PathNode Parent;

        public PathNode(Vector2Int pos)
        {
            Position = pos;
        }
    }
}