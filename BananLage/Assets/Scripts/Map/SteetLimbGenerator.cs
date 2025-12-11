using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    public class StreetPathBuilder
    {
        private readonly int lineSize;
        private readonly int width, height;
        private readonly bool[,] forestMask;
        private readonly float backtrackTolerance; // how many failed cycles allowed
        private readonly Vector2Int center;
        private readonly int centerSize;

        private readonly YieldInstruction delay;

        public bool enableStep, stepped;

        public StreetPathBuilder(
            int width, int height,
            bool[,] forestMask,
            int lineSize,
            int centerSize,
            Vector2Int center,
            float backtrackTolerance,
            YieldInstruction delay
        )
        {
            this.width = width;
            this.height = height;
            this.forestMask = forestMask;
            this.lineSize = lineSize;
            this.center = center;
            this.centerSize = centerSize;
            this.backtrackTolerance = backtrackTolerance;
            this.delay = delay;
        }

        public IEnumerator BuildPath(Street street, bool debug)
        {
            yield return BuildPathInternal(street, debug);
            if (debug)
                Debug.Log("Finished building path");
            
            street.done = true;
        }
        
        private IEnumerator BuildPathInternal(Street street, bool debug = false)
        {
            street.paths.Clear();
            var pos = center;
            var target = street.target.ToInt();
            var currentDir = (target - pos.ToFloat()).ToDirection();
            var initialDir = currentDir;

            var history = new Stack<(PathDirection dir, Vector2Int root, List<Vector2Int> positions)>();
            
            var limbLifetime = 0.3f;
            var currentLimbLT = limbLifetime;
            var streetLifeTime = 1f;
            
            var effectiveLineSize = lineSize + 1f;
            var decayPerAttempt = 0.1f;

            var reached = false;
            while (!reached)
            {
                effectiveLineSize -= decayPerAttempt;
                effectiveLineSize = Mathf.Max(effectiveLineSize, 1f);

                if ((streetLifeTime -= Time.deltaTime) <= 0f)
                {
                    //restart
                    yield return BuildPathInternal(street, debug);
                    yield break;
                }

                Debug.Log($"Effective Line = {effectiveLineSize}, lifetime={currentLimbLT}");

                var possibleDirections = BuildAvailableTurns(target, pos);
                
                if ((currentLimbLT -= Time.deltaTime) <= 0)
                {
                    //restart
                    yield return BuildPathInternal(street, debug);
                    yield break;
                }
                
                var unwanted  = new List<PathDirection>();
                var rotated = false;
                while (possibleDirections.Count > 0)
                {
                    var attempt = WeightedRandomEntry(possibleDirections, unwanted);
                    if (attempt == initialDir.Opposite())
                    {
                        //do not allow backtrack
                        PurgeHistory(ref history, ref currentDir, ref attempt, ref street, ref pos);
                        break;
                    }

                    yield return Step();
                    var limb = new List<Vector2Int>();

                    var initial = pos;
                    limb.Add(pos);
                    for (var i = 0; i < effectiveLineSize; i++)
                    {
                        if (i == 0 || i > effectiveLineSize - 2)
                        {
                            //start or end of limb, fill extra gaps
                            if (attempt.IsDiagonalOf(currentDir))
                            {
                                var ll = attempt.Rotate(2).Vector();
                                var rr = attempt.Rotate(-2).Vector();
                                var tp = pos + currentDir.Vector();
                                limb.Add(tp);
                                limb.Add(pos + ll);
                                limb.Add(pos + rr);
                                limb.Add(tp + ll);
                                limb.Add(tp + rr);
                            }
                        }

                        var l = pos + attempt.Left().Vector();
                        var r = pos + attempt.Right().Vector();
                        pos += attempt.Vector();

                        limb.Add(pos);
                        limb.Add(l);
                        limb.Add(r);

                        if (Vector2Int.Distance(l, target) < 0.1f
                            || Vector2Int.Distance(pos, target) < 0.1f
                            || Vector2Int.Distance(r, target) < 0.1)
                        {
                            //finalize immediately, means any new limb part reached destination
                            if (debug)
                                Debug.Log(
                                    $"#Street Generator# Reached destination {target} from either {pos}, {l}, {r}");

                            street.paths.AddRange(limb);
                            yield break;
                        }

                        reached = Vector2Int.Distance(l, target) < centerSize / 2f
                                  || Vector2Int.Distance(pos, target) < centerSize / 2f
                                  || Vector2Int.Distance(r, target) < centerSize / 2f;
                    }

                    history.Push((attempt, initial, limb));
                    street.paths.AddRange(limb);

                    yield return Step();

                    if (delay != null)
                        yield return delay;

                    foreach (var p in limb)
                    {
                        var (i, j) = (p.x + width / 2, p.y + height / 2);

                        if (!WithinBounds(p))
                        {
                            yield break; //assume finished, too much edge cases to polish...
                        }

                        if (forestMask[i, j])
                        {
                            //try again the same step
                            street.paths.RemoveAll(e => limb.Contains(e));
                            possibleDirections.Remove(attempt);
                            unwanted.Add(attempt);

                            pos = initial;
                            if (!rotated)
                            {
                                rotated = true;
                                possibleDirections.Add(attempt.Rotate(2));
                                possibleDirections.Add(attempt.Rotate(-2));
                            }
                        }
                        
                        //maybe prevent n amounts of intersections...
                    }

                    if (!possibleDirections.Contains(attempt))
                        break;

                    if (history.Count == 0)
                        break;

                    currentDir = attempt;
                    effectiveLineSize = lineSize + 1f;
                    currentLimbLT = limbLifetime;
                    break;
                }
                
                unwanted.Clear();
            }
        }

        private PathDirection WeightedRandomEntry(List<PathDirection> possibleDirections, List<PathDirection> unwanted)
        {
            foreach (var p in unwanted)
            {
                possibleDirections.Remove(p);
            }
            
            var w = possibleDirections.Count;
            var weights = new int[w];
            var total = 0f;
            for (var i = 0; i < w; ++i)
            {
                weights[i] = w - i;
                total += weights[i];
            }

            var dice = Random.value;
            var check = 0f;

            for (var i = 0; i < w; i++)
            {
                check += weights[i] / total;
                if (check < dice) continue; 
                
                return possibleDirections[i];
            }

            return possibleDirections.RandomEntry();
        }
        
        private void PurgeHistory(ref Stack<(PathDirection dir, Vector2Int root, List<Vector2Int> positions)> history,
            ref PathDirection currentDir, ref PathDirection attempt, ref Street street, ref Vector2Int currentPos)
        {
            if (history.Count == 0)
            {
                street.paths.Clear();
                return;
            }

            var item = history.Pop();

            if (item.dir.Diff(attempt) < 2 || item.dir == currentDir)
            {
                attempt = item.dir;
                // currentDir = item.dir;
                currentPos = item.root;
                foreach (var itemPosition in item.positions)
                {
                    street.paths.Remove(itemPosition);
                }
            }
            else return;
        }

        private List<PathDirection> BuildAvailableTurns(Vector2Int target, Vector2Int pos)
        {
            var output = new List<PathDirection>();
            var optimalVector = (target - pos).ToFloat();
            var optimal = optimalVector.ToDirection();

            optimalVector.Normalize();
            output.Add(optimal);

            //allow 90º turns
            if (optimalVector.sqrMagnitude < 0.3f)
            {
                if (target.x > 0)
                    output.Add(optimal - 2);


                if (target.x < 0)
                    output.Add(optimal + 2);
            }

            output.Add(optimal - 1);
            output.Add(optimal + 1);


            return output;
        }

        private bool WithinBounds(Vector2Int p)
        {
            var (i, j) = (p.x + width / 2, p.y + height / 2);
            return i >= 0 && i < width && j >= 0 && j < height;
        }

        private IEnumerator Step()
        {
            if (!enableStep) yield break;

            yield return new WaitUntil(() => stepped);
            stepped = false;
        }
    }
}