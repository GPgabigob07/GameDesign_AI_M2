using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AI.Monkey;
using Mechanics.ItemManagement;
using Mechanics.Jobs.Types;
using Mechanics.Village.Pools;
using Misc;
using Structures;
using Structures.Types;
using UI;
using UI.Controllers;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static UnityEngine.Random;

namespace Mechanics.Village
{
    [RequireComponent(typeof(JobProgressObjectPool))]
    public class VillageManager : StructureController<IdlingJobContext>
    {
        private static VillageManager _instance;
        private static readonly int FlashCount = Shader.PropertyToID("_FlashCount");

        public static bool ImmediateResolve => _instance.skippingCycle;
        public static float Speed { get; private set; } = 1f;

        public static bool InNeedOfBananas
        {
            get
            {
                var totalBananas = VillageInventory["Banana"];
                var totalMonkey = Monkeys.Count * 1.25f;

                return totalBananas < totalMonkey;
            }
        }

        #region computation

        public static bool CanExecuteTask(MonkeyData monkey, TaskType task, BaseStructure str)
        {
            if (_instance.skippingCycle) return false;
            if (task == TaskType.Idle) return true;

            return CanExecuteTask(monkey, task, str.CalculateCost(), out _);
        }

        public static bool CanExecuteTask(MonkeyData monkey, TaskType task, int nominalCost, out int cost)
        {
            cost = CalculateExecutionCost(monkey, task, nominalCost);

            if (_instance._config.debug)
            {
                Debug.Log($"[ID:{monkey.Id}::{monkey.Name}] Effective cost for {task}: {cost} of {nominalCost}]");
            }

            return cost <= monkey.ActionValue;
        }

        private static int CalculateExecutionCost(MonkeyData monkey, TaskType task, int nominalCost)
        {
            var prowess = monkey.Prowess[task]; //1
            var effectiveProwess = GetMultiplier(monkey, prowess); //0,125

            var effectiveCost = monkey.Prowess[task] > _instance._config.NoExecutionCostProwess[task]
                ? _instance._config.MinimalCost
                : nominalCost;

            effectiveCost /= 1 + effectiveProwess;
            return Mathf.RoundToInt(Mathf.Clamp(effectiveCost, 0, float.PositiveInfinity));
        }

        public static void ComputeProductionProgressForJob(ProductionJobContext ctx)
        {
            if (!ctx.HasBegun || ctx.HasEnded) return;

            var (_, task) = ctx;
            var prowess = ctx.Worker.Prowess[task];
            var multiplier = GetMultiplier(ctx.Worker, prowess);
            //calculate % boost for both cost and output
            var boost = 1 + multiplier;

            var outputBoost = 1 + (_instance._config.taskAmpRange.TryGetValue(task, out var amps)
                ? amps.First(e => (prowess * boost) <= e.upTo).amplification //calculate accordingly to config
                : (prowess * boost * _instance._config.NominalOutputBoost)); //use a normalized bonus

            if (ImmediateResolve)
            {
                foreach (var resourceOutput in ctx.Output)
                {
                    resourceOutput.Progress = 1f;
                    resourceOutput.effectiveAmount = (resourceOutput.expectedAmount * outputBoost).Floor();
                }

                return;
            }

            foreach (var resourceOutput in ctx.Output)
            {
                resourceOutput.Progress += (Time.deltaTime * Speed * boost) / resourceOutput.output.productionTime;
                resourceOutput.effectiveAmount = (resourceOutput.expectedAmount * outputBoost).Floor();

                if (_instance._config.debug)
                {
                    Debug.Log(
                        $"[ResourceOutput] Producing: {resourceOutput.output.displayName}x{resourceOutput.expectedAmount} boosted with {outputBoost} = {resourceOutput.effectiveAmount}x at {resourceOutput.Progress * 100}%");
                }
            }
        }

        public static float ComputeBuildingProgressFor(BuildJobContext ctx)
        {
            if (_instance._config.debug)
                Debug.Log(
                    $"[BuildJobContext] Progressing: {ctx.Structure.name}");

            var (monkeys, _, config) = ctx;

            if (ImmediateResolve)
            {
                var totalContribution = monkeys.Aggregate(0f,
                    (total, monkey) => ComputeBuildingProgressPerMonkey(monkey, ctx, config) + total);
                if (_instance._config.debug)
                    Debug.Log($"[BuildJobContext] Total contribution: {totalContribution}");

                return totalContribution;
            }

            var timeProgress = Time.deltaTime * Speed;

            var progress = 0f;
            foreach (var monkey in monkeys)
                progress += timeProgress * ComputeBuildingProgressPerMonkey(monkey, ctx, config);

            return progress;
        }

        public static void ComputeMonkeyBreed(MatingJobContext ctx, MonkeyData father, MonkeyData mother,
            ref MonkeyData[] breed)
        {
            if (breed == null)
                CreateMonkeyBreed(out breed, father, mother);
        }

        private static void CreateMonkeyBreed(out MonkeyData[] breed, MonkeyData father, MonkeyData mother)
        {
            var config = _instance._config;
            var bedEfficiency = father.Prowess[TaskType.Mate] * config.FatherMatingMultiplier
                                + mother.Prowess[TaskType.Mate] * config.MotherMatingMultiplier;

            var amp = 1 + bedEfficiency;

            var monkeyAmount = amp.Floor();
            breed = new MonkeyData[monkeyAmount];

            var points = father.Prowess.Total + mother.Prowess.Total - config.startProwessPoints; //absolute total
            points /= 2; //average

            var effectivePoints = (points * amp).Floor(); //amplify based on parents, may need a "reduction"...
            var combinedProwess = father.Prowess + mother.Prowess;

            var combindedAv = father.Self.OriginalData.ActionValue + mother.Self.OriginalData.ActionValue;
            var effectiveAv = (combindedAv / 2f) * amp;

            var combinedHp = father.Self.OriginalData.Hp + mother.Self.OriginalData.Hp;
            var effectiveHp = (combinedHp / 2f) * amp;

            for (var i = 0; i < breed.Length; i++)
            {
                var monkey = config.CreateMonkey(config.startProwessPoints);
                var prowess = monkey.Prowess;
                for (var p = 0; p < effectivePoints; p++)
                {
                    prowess[combinedProwess.ResolveNew(config.debug)]++;
                }

                monkey.ActionValue = effectiveAv.Floor();
                monkey.Hp = effectiveHp.Floor();
                breed[i] = monkey;
            }
        }

        public static int ConsumeActionValue(MonkeyData monkey, int howMuch, TaskType ctxTaskType)
        {
            //check for buffs/debuffs
            var toConsume = Mathf.Min(monkey.ActionValue, howMuch);
            var p = monkey.Prowess[ctxTaskType];
            var multiplier = 1 + GetMultiplier(monkey, p);

            var reduced = toConsume / multiplier;
            var limited = Mathf.Clamp(reduced, _instance._config.MinimalCost, int.MaxValue).Round();

            if (_instance._config.debug)
                Debug.Log(
                    $"[VillageManager]: Consuming: {limited} from {monkey.Name}. original={toConsume}, reduced={reduced}");

            monkey.ActionValue -= limited;

            return limited;
        }

        private static float ComputeBuildingProgressPerMonkey(MonkeyData monkey, BuildJobContext ctx,
            CycleConfiguration config)
        {
            var availableAv = ctx.GetAvailableAvFor(monkey);
            if (availableAv <= 0) return 0;
            var p = monkey.Prowess.Build;
            var disposableAv = Mathf.Min(monkey.ActionValue, availableAv);
            var m = 1 + GetMultiplier(monkey, p);
            var step = disposableAv * m;

            if (_instance._config.debug)
                Debug.Log(
                    $"[BuildJobContext] {monkey.Name} can produce {step}, out of av: {disposableAv} * multiplier: {m} for {ctx.Structure.name}");

            return step / (config.timePerAv * availableAv);
        }

        private static float GetMultiplier(MonkeyData monkey, int prowess)
        {
            return prowess * _instance._config.ProwessBoostUnit +
                   monkey.Age * _instance._config.AgeProwessBoostUnit;
        }

        #endregion

        #region references

        [SerializeField] private MonkeyGlobalConfig _config;

        [SerializeField] private JobProgressObjectPool _pool;
        [SerializeField] private List<StructureForTask> buildableStructures;
        [SerializeField] private bool skippingCycle;
        [SerializeField] private int currentCycle = 0;
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private MonkeyInWorldInfoController monkeyInfo;
        
        private MaterialPropertyBlock lrBlock;
        private Dictionary<string, MonkeyData> AllMonkeys { get; set; }
        public static IReadOnlyDictionary<string, MonkeyData> Monkeys => _instance ? _instance.AllMonkeys : null;
        public static int Cycle => _instance ? _instance.currentCycle : 0;
        private MapController MapController { get; set; }

        [Header("Runtime references")] [SerializeField]
        private List<BaseStructure> activeStructures;

        #endregion

        private void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            _pool = GetComponent<JobProgressObjectPool>();
            AllMonkeys = new Dictionary<string, MonkeyData>();
            _currentJob = new IdlingJobContext(this, TaskType.Idle);
            MapController = FindFirstObjectByType<MapController>();
        }

        #region Self Structure

        public override JobContext JobContext => _currentJob;
        public override TaskType JobType => TaskType.Idle;

        [field: SerializeField] private int idleAvDeduction;
        public static int IdleAvDeduction => _instance.idleAvDeduction;

        protected override IdlingJobContext CreateSpecializedJob(MonkeyData monkey)
        {
            return _currentJob?.Add<IdlingJobContext>(monkey);
        }

        protected override void Update()
        {
            base.Update();
            JobContext.Tick();
        }

        public override bool InWorkingArea(Vector3 pos) => true;
        public override int CalculateCost() => 0;
        public override Vector3 MonkeyWorkingArea => new(Range(-50, 50), Range(-50, 50), 0);
        public override bool IsAvailable => !ImmediateResolve;
        
        public static Inventory VillageInventory => _instance.Inventory;

        #endregion

        #region structures

        public static MapController Map => _instance.MapController ??= FindFirstObjectByType<MapController>();

        public static IEnumerable<BaseStructure> AvailableStructuresFor(TaskType task)
        {
            if (task == TaskType.Idle)
                return new[] { _instance };

            // dynamic necessities... 
            if (task is TaskType.Gather or TaskType.Farm && InNeedOfBananas)
                return _instance.activeStructures
                    .Where(s => s.JobType == task)
                    .Where(e => e.IsAvailable)
                    .Where(e => e.StructureData.outputs is { Length: > 0 })
                    .Where(e => e.StructureData.outputs.Any(otp => otp.resource.displayName == "Banana"));// item indexing only at asset side, should've ready a hybrid code + asset... to late now

            return _instance.activeStructures.Where(e => e.IsAvailable && e.JobType == task);
        }

        public static void RegisterStructure(BaseStructure structure)
        {
            if (!_instance) return;
            InventoryManager.Register(structure);
            _instance.activeStructures.Add(structure);
        }

        public static void UnregisterStructure(BaseStructure structure)
        {
            InventoryManager.Unregister(structure);
            _instance.activeStructures.Remove(structure);
        }

        public static ItemOutputProgress[] GetItemProgressDisplay(ProductionJobContext ctx)
        {
            var origin = ctx.Structure;
            var originPos = origin.transform.position;
            var xOrigin = originPos.x;
            var yOrigin = originPos.y++ /*+ (originData.outputs.Length) - (originData.worldSize.y / 2f)*/;

            var outputs = new ItemOutputProgress[ctx.Output.Count];
            for (var i = 0; i < ctx.Output.Count; i++)
            {
                var next = _instance._pool.Request();
                outputs[i] = next;

                next.transform.position = new Vector3(xOrigin, yOrigin++, 0);
                next.Resource = ctx.Output[i];
            }

            return outputs;
        }

        #endregion

        public static MonkeyProwess GetMinimumExecutionForIncrease()
        {
            return _instance._config.ProwessExecutionIncrease;
        }

        public static void DismissProgress(ref ItemOutputProgress[] progresses)
        {
            if (progresses == null) return;
            foreach (var itemOutputProgress in progresses)
            {
                _instance._pool.Release(itemOutputProgress);
            }

            progresses = null;
        }

        public static void Place(BuildJobContext jobContext, StructureData targetStructure, Vector3 transformPosition)
        {
            //deduce cost
            var structure = _instance.buildableStructures.FirstOrDefault(e => e.task == targetStructure.taskType);

            if (structure == null)
            {
                Debug.LogError("#VillageManager# Cannot place a structure without a buildable");
                return;
            }

            var str = Instantiate(structure.originalStructure, transformPosition, Quaternion.identity);
            str.StructureData = targetStructure;

            //jobContext.ClearMonkeys(newerStructure);
        }

        private IEnumerator PutMonkeysToRest(AdvanceCycleModalController modalController)
        {
            var oldSpeed = Speed;
            Speed = 3f;
            modalController.Show();
            var monkeys = AllMonkeys.Values.Select(e => e.Self).ToList();

            SendRestSignal(monkeys);

            var delay = new WaitForSeconds(1f);

            while (monkeys.Any(e => e.CurrentJob is { CyclesToComplete: 0 } && !e.IsResting))
            {
                if (_config.debug)
                    Debug.Log("[VillageManager] Waiting for monkeys to start resting");

                yield return delay;
            }

            currentCycle++;

            foreach (var m in monkeys)
            {
                var remainingAv = m.CycleData.ActionValue;
                m.OriginalData = m.OriginalData.Clone().Refresh(m.CycleData);
                m.CycleData = m.OriginalData.Clone();
                m.CycleData.ActionValue = remainingAv;//done as the resting job will reevaluate this value afterwards
            }

            Speed = oldSpeed;
            yield return delay;
            
            foreach (var instanceActiveStructure in activeStructures)
            {
                // instanceActiveStructure.RefreshCycleData(GetCycleDataFor(instanceActiveStructure));
                instanceActiveStructure.OnCycleChanged();
            }
            
            if (CheckLost())
            {
                modalController.SetMessage("Você falhou com\na sociedade dos símios!");
                yield return new WaitForSeconds(3f);
                SceneNavigator.ToMenu();
                yield break;
            }
            
            modalController.Hide();
            skippingCycle = false;
        }
        

        [ContextMenu("Next Cycle")]
        public void NextCycle(AdvanceCycleModalController modalController)
        {
            skippingCycle = true;
            StartCoroutine(PutMonkeysToRest(modalController));
        }

        private void SendRestSignal(IEnumerable<MonkeyCharacterBT> readyMonkeys)
        {
            foreach (var m in readyMonkeys)
            {
                m.PutToRest = true;
            }
        }

        private static StructureInstanceData GetCycleDataFor(BaseStructure str)
        {
            var original = str.StructureData;
            return new StructureInstanceData
            {
                State = !str.IsAvailable ? StructureState.Working : str.CycleData.State,
            };
        }

        public static void RegisterWorker(MonkeyData cycleData)
        {
            InventoryManager.Register(cycleData);
            _instance.AllMonkeys.TryAdd(cycleData.Id, cycleData);
        }

        public static void UnregisterWorker(MonkeyData cycleData)
        {
            InventoryManager.Unregister(cycleData);
            _instance.AllMonkeys.Remove(cycleData.Id);
        }

        public static void DestroyStructure(BaseStructure str)
        {
            //todo
        }

        public static void InstantiateNewMonkeys(BaseStructure structure, MonkeyData[] breed)
        {
            foreach (var monkeyData in breed)
            {
                var unit = Instantiate(_instance._config.MonkeyPrefab);

                unit.transform.position = structure.transform.position;
                unit.OriginalData = monkeyData;
                unit.CycleData = monkeyData.Clone();
            }
        }

        public static void AdvanceCycle(AdvanceCycleModalController modalController)
        {
            _instance.NextCycle(modalController);
        }

        public static void RecoverAv(string id, float efficiency)
        {
            var reference = _instance.AllMonkeys[id];

            var original = reference.Self.OriginalData;
            var cycleData = reference.Self.CycleData;

            var toRecover = original.ActionValue - cycleData.ActionValue;
            var bananaCost = (toRecover * (1 + 1 * efficiency)).Round();
            var bananaPerAv = bananaCost / (toRecover * 1f);

            var bananas = VillageInventory["Banana"];

            var info = Instantiate(_instance.monkeyInfo);
            
            info.transform.position = cycleData.Self.transform.position;
            
            if (bananaCost > bananas)
            {
                var recoverable = bananas /  bananaPerAv;
                var hpLoss = toRecover - recoverable;
                cycleData.Hp -= hpLoss.Floor();
                cycleData.ActionValue += toRecover;
                VillageInventory["Banana"] = 0;
                
                info.type = MonkeyInfoType.HpLoss;
                info.amount = hpLoss.Floor();
                
                if (_instance._config.debug)
                    Debug.Log($"[VillageManager] Monkey {cycleData.Name} lost {hpLoss} HP");
                
                return;
            }
            
            VillageInventory["Banana"] -= bananaCost;
            cycleData.ActionValue += toRecover;
            
            info.type = MonkeyInfoType.AVGain;
            info.amount = toRecover;
            
            if (_instance._config.debug)
                Debug.Log($"[VillageManager] Monkey {cycleData.Name} gain {toRecover} AV");
        }

        public static void RenderMonkeyPath(List<Vector2Int> points)
        {
            var lr = _instance.lineRenderer;
            lr.GetPropertyBlock(_instance.lrBlock ??= new());
            lr.enabled = points.Count > 0;

            if (lr.enabled)
            {
                _instance.lrBlock.SetFloat(FlashCount, points.Count / 4.5f);
                lr.SetPropertyBlock(_instance.lrBlock);

                lr.positionCount = points.Count;
                lr.SetPositions(points.Select(e => e.To3D().ToFloat()).ToArray());
            }
        }

        private static bool CheckLost()
        {
            var ms = _instance
                .AllMonkeys
                .Select(e => e.Value)
                .ToList();
            
            var mCount = ms
                .Where(e => e.IsMale)
                .Count(e => e.Hp > 0);
            
            var fCount = ms
                .Where(e => e.IsFemale)
                .Count(e => e.Hp > 0);

            return mCount < 1 || fCount < 1;
        }

        public static Dictionary<TaskType, float> GetComputedDecisionWeightsByQuantity()
        {
            //add a tendency to perform what there is most of
            var config = _instance._config;
            var weights = config.decisionWeightPercentPerUnit;

            var result = new Dictionary<TaskType, float>();

            var available = _instance.activeStructures
                .Where(e => e.IsAvailable);

            foreach (var str in available)
                if (!result.TryAdd(str.JobType, 1))
                    result[str.JobType]++;

            foreach (TaskType e in typeof(TaskType).GetEnumValues())
            {
                result.TryAdd(e, 0);
                result[e] = 1 + result[e] * (weights[e] / 100f);
            }

            return result;
        }

        public static Dictionary<TaskType, float> GetComputedDecisionWeightsByNecessity()
        {
            var config = _instance._config;
            var weights = config.decisionWeightPercentPerUnit;

            var result = new Dictionary<TaskType, float>();
            foreach (TaskType e in typeof(TaskType).GetEnumValues())
            {
                result.TryAdd(e, 1);
            }

            // dynamic necessities were a plan...

            // low bananas
            // assumes at least 1.25 bananas per monkey to add a margin
            // no bananas = monkey loses n * HP / 2 AV
            var totalMonkey = Monkeys.Count * 1.25f;

            if (InNeedOfBananas)
            {
                result[TaskType.Gather] = 1 + totalMonkey * (weights[TaskType.Gather] / 100f);
            }

            return result;
        }
    }

    [Serializable]
    public class StructureForTask
    {
        public TaskType task;
        [SerializeReference] public BaseStructure originalStructure;
    }
}