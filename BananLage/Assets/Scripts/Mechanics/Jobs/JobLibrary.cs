using System;
using AI.Monkey;
using Mechanics.Jobs.Types;
using Mechanics.Production;
using Mechanics.Village;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Mechanics.Jobs
{
    public class JobLibrary : MonoBehaviour
    {
        private static JobLibrary _instance;

        [field: SerializeField] private MonkeyGlobalConfig Config { get; set; }

        private void Awake()
        {
            if (_instance) Destroy(gameObject);
            _instance = this;
            DontDestroyOnLoad(this);
        }

        public static MonkeyPerformJobNode GetJobNode(TaskType task)
        {
            var node = new MonkeyPerformJobNode();
            return node;
        }

        private void Gather(MonkeyCharacterBT bt, JobContext jobContext)
        {
            if (jobContext.IsFinished) return;

            //VillageManager.ComputeProgressForJob(jobContext);
        }

        public static void FillOutputs(ProductionJobContext jobContext)
        {
            if (jobContext.IsFinished) return;
            if (jobContext.Output is { Count: > 0 }) return;

            var structureStructureData = jobContext.Structure.StructureData;
            foreach (var data in structureStructureData.outputs)
            {
                if (data.chance < Random.value) continue;
                
                jobContext.AddOutput(new ResourceOutput
                {
                    expectedAmount = data.amount,
                    output = data.resource,
                    Progress = VillageManager.ImmediateResolve ? 1 : 0,
                    source = structureStructureData,
                    worker = jobContext.Worker
                });
            }
        }

        public static bool CanPerformJob(JobContext ctx)
        {
            return ctx is { IsFinished: false, HasBegun: true }/* &&
                   VillageManager.CanExecuteTask(ctx.Worker, ctx.TaskType, ctx.Structure)*/;
        }
    }
}