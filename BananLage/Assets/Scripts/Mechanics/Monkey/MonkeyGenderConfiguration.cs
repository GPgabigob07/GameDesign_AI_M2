using System;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Mechanics
{
    [Serializable]
    public struct MonkeyGenderConfiguration
    {
        public int minMatingCycles, maxMatingCycles;
        public int minStartHp, maxStartHp;
        public MonkeyProwess likelyProwess;
        public int baseActionValue;

        public int NewCycle() => Random.Range(minMatingCycles, maxMatingCycles);
        public int NewHp() => Random.Range(minStartHp, maxStartHp);

        public MonkeyProwess NewProwess(int points)
        {
            //Creates a prowess data randomly setting values 
            //but respecting provided weighted configuration 
#if UNITY_EDITOR
            Debug.Log("Creating Monkey Prowess");
#endif

            var prowess = new MonkeyProwess();
            var totalWeights = 0f;
            foreach (var item in typeof(TaskType).GetEnumValues())
            {
                prowess.Allow((TaskType)item, true);
                totalWeights += likelyProwess[(TaskType)item];
            }
            
#if UNITY_EDITOR
            Debug.Log($"[Monkey Prowess] Generating Monkey Prowess: {totalWeights}");
#endif

            for (var i = 0; i < points; i++)
            {
                var dice = Random.value;
                var sum = 0f;
#if UNITY_EDITOR
                Debug.Log($"[Monkey Prowess] Checking dice={dice} for {sum}");
#endif
                foreach (var item in typeof(TaskType).GetEnumValues())
                {
                    var task = (TaskType)item;
                    sum += likelyProwess[task] / totalWeights;
                    if (dice < sum)
                    {
                        prowess[task]++;
#if UNITY_EDITOR
                        Debug.Log($"[Monkey Prowess] Checking increasing {item} to {prowess[task]}");
#endif
                        break;
                    }
                }
            }

            return prowess;
        }
    }
}