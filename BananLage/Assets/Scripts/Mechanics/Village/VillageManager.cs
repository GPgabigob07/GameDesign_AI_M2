using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Mechanics.Jobs.Types;
using Mechanics.Village.Pools;
using Structures;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Serialization;

namespace Mechanics.Village
{
    [RequireComponent(typeof(JobProgressObjectPool))]
    public class VillageManager: MonoBehaviour
    {
        private static VillageManager _instance;
        
        public static bool ImmediateResolve { get; private set; } = false;
        public static float Speed { get; private set; } = 1f;
        
        #region computation
        public static bool CanExecuteTask(MonkeyData monkey, TaskType task, int nominalCost)
        {
            return CanExecuteTask(monkey, task, nominalCost, out _);
        }

        public static bool CanExecuteTask(MonkeyData monkey, TaskType task, int nominalCost, out int cost)
        {
            cost = CalculateExecutionCost(monkey, task, nominalCost);

            if (_instance._config.debug)
            {
                Debug.Log($"[ID:{monkey.UUID}::{monkey.Name}] Effective cost for {task}: {cost}], deducing...");
            }

            return cost <= monkey.ActionValue;
        }

        private static int CalculateExecutionCost(MonkeyData monkey, TaskType task, int nominalCost)
        {
            var prowess = monkey.Prowess[task];
            var effectiveProwess = GetMultiplier(monkey, prowess);

            var effectiveCost = monkey.Prowess[task] > _instance._config.NoExecutionCostProwess[task] 
                ? _instance._config.MinimalCost 
                : nominalCost;

            effectiveCost *= 1 - effectiveProwess;
            return Mathf.FloorToInt(Math.Clamp(effectiveCost, 0, float.PositiveInfinity));
        }

        public static void ComputeProgressForJob(ProductionJobContext ctx)
        {
            if (!ctx.HasBegun || ctx.HasEnded) return;
            
            var (monkey, _, task) = ctx;
            var prowess = monkey.Prowess[task];
            var multiplier = GetMultiplier(monkey, prowess);
            //calculate % boost for both cost and output
            var boost = 1 + multiplier;
            
            var outputBoost = _instance._config.taskAmpRange.TryGetValue(task, out var amps) 
                ? amps.First(e => (prowess * boost) <= e.upTo).amplification //calculate accordingly to config
                : (prowess * boost * _instance._config.NominalOutputBoost); //use a normalized bonus
            
            foreach (var resourceOutput in ctx.Output)
            {
                resourceOutput.Progress += (Time.deltaTime * Speed * boost) / resourceOutput.output.productionTime;
                resourceOutput.effectiveAmount = (resourceOutput.expectedAmount * (1 + outputBoost)).Floor();

                if (_instance._config.debug)
                {
                    Debug.Log($"[ResourceOutput] Producing: {resourceOutput.effectiveAmount}x{resourceOutput.output.name} at {resourceOutput.Progress * 100}%");
                }
            }
        }
        
        public static float ComputeBuildingProgressFor(BuildJobContext ctx)
        {
            var (monkeys, _) = ctx;
            return monkeys.Aggregate(Time.deltaTime * Speed, (progress, monkey) =>
            {
                var p = monkey.Prowess.Build;
                var multiplier = GetMultiplier(monkey, p);
                return progress * multiplier;
            });
        }

        private static float GetMultiplier(MonkeyData monkey, int prowess)
        {
            return prowess * _instance._config.ProwessBoostUnit / 100 +
                    monkey.Age * _instance._config.AgeProwessBoostUnit / 100;
        }
        
        #endregion
        
        #region references
        [SerializeField] private MonkeyGlobalConfig _config;
        
        [SerializeField] private JobProgressObjectPool _pool;
        [SerializeField]
        private List<StructureForTask> buildableStructures;
        
        private Dictionary<Guid, MonkeyData> AllMonkeys { get; set; }
        
        [Header("Runtime references")]
        [SerializeField] private List<BaseStructure> activeStructures;
        
        #endregion
        private void Awake()
        {
            if (_instance) Destroy(gameObject);
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            _pool = GetComponent<JobProgressObjectPool>();
            AllMonkeys = new Dictionary<Guid, MonkeyData>();
        }

        #region structures
        public static List<BaseStructure> AvailableStructuresFor(TaskType task)
        {
            return _instance.activeStructures.FindAll(e => e.JobContext == null && e.JobType == task);
        }

        public static void RegisterStructure(BaseStructure structure) => _instance.activeStructures.Add(structure);
        public static void UnregisterStructure(BaseStructure structure) => _instance.activeStructures.Remove(structure);

        public static ItemOutputProgress[] GetItemProgressDisplay(ProductionJobContext ctx)
        {
            var origin = ctx.Structure;
            var originData = origin.StructureData;
            var originPos = origin.transform.position;
            var xOrigin = originPos.x - 0.5f;
            var yOrigin = originPos.y /*+ (originData.outputs.Length) - (originData.worldSize.y / 2f)*/;
            
            var outputs = new ItemOutputProgress[ctx.Output.Count];
            for (var i = 0; i < ctx.Output.Count; i++)
            {
                var next = _instance._pool.Request();
                outputs[i] = next;
                
                next.transform.position = new Vector3(xOrigin, yOrigin--, 0);
                next.Resource = ctx.Output[i];
            }
            
            return outputs;
        }

        #endregion

        public static void DismissProgress(ref ItemOutputProgress[] progresses)
        {
            if (progresses == null) return;
            foreach (var itemOutputProgress in progresses)
            {
                _instance._pool.Release(itemOutputProgress);
            }
            progresses = null;
        }

        public static void CollectOutputs(ProductionJobContext ctx)
        {
            var outputs = "";
            foreach (var resourceOutput in ctx.Output)
            {
                outputs += resourceOutput.effectiveAmount + "x" + resourceOutput.output.name + " ";
            }
            Debug.Log($"[VillageManager] Collecting outputs: {outputs}");
        }

        public static void Place(StructureData targetStructure, Vector3 transformPosition)
        {
            //deduce cost
            var structure = _instance.buildableStructures.FirstOrDefault(e => e.task == targetStructure.taskType);

            if (structure == null)
            {
                Debug.LogError("[VilalgeManager] Cannot place a structure without a buildable");
                return;
            }
            
            Instantiate(structure.originalStructure, transformPosition, Quaternion.identity);
        }

        [ContextMenu("Next Cycle")]
        public static void NextCycle()
        {
            foreach (var instanceActiveStructure in _instance.activeStructures)
            { 
                instanceActiveStructure.RefreshCycleData(GetCycleDataFor(instanceActiveStructure));
            }

            foreach (var monkeys in _instance.AllMonkeys.Values)
            {
                var manager = monkeys.Self;
            }
        }

        private static StructureInstanceData GetCycleDataFor(BaseStructure str)
        {
            var original = str.StructureData;
            return new StructureInstanceData
            {
                State = str.JobContext != null ? StructureState.Working : str.CycleData.State,
                AvalableAV = original.maxAvPerCycle
            };
        }

        public static void RegisterWorker(MonkeyData cycleData)
        {
            _instance.AllMonkeys.TryAdd(cycleData.UUID, cycleData);
        }
    }

    [Serializable]
    public class StructureForTask
    
    {
        public TaskType task;
        [SerializeReference]
        public BaseStructure originalStructure;
    }
}