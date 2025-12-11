using System;
using System.Collections.Generic;
using System.Linq;
using Mechanics.ItemManagement;
using Mechanics.Production;
using Mechanics.Village;
using Structures;
using Structures.Types;
using UnityEngine;

namespace Mechanics.Jobs.Types
{
    public class ProductionJobContext : SingleWorkerJobContext
    {
        public List<ResourceOutput> Output { get; private set; }
        private CycleConfiguration ProductionConfig => Structure.StructureData.productionCycleConfiguration;

        public ProductionJobContext(MonkeyData monkey, StructureController<ProductionJobContext> structure,
            TaskType taskType) : base(monkey, structure, taskType)
        {
        }

        public void AddOutput(ResourceOutput output) => (Output ??= new List<ResourceOutput>()).Add(output);

        protected override void OnBegin()
        {
            Worker.Self.Animations.PlayTask(Structure.StructureData.taskAnimation, TaskType);
            
            JobLibrary.FillOutputs(this);
            if (Structure is YieldStructureController yielder)
                yielder.NotifyJobProgress();
        }

        protected override void OnFinish()
        {
            if (Structure is YieldStructureController yielder)
                yielder.NotifyJobProgress();

            var inv = new Inventory();
            var succeeded = Output.Aggregate(true, (current, resourceOutput) => current && inv.TryAdd(resourceOutput.output, resourceOutput.effectiveAmount));

            if (succeeded)
                InventoryManager.DepositAll(inv, VillageManager.VillageInventory);
        }

        private float soundDebounce = 0f;
        protected override void OnTick()
        {
            VillageManager.ComputeProductionProgressForJob(this);
            IsFinished = Output.All(e => e.Progress >= 1);

            if ((soundDebounce -= Time.deltaTime) <= 0)
            {
                var sounds = SoundEngine.bus.Sounds;
                var sound = TaskType switch
                {
                    TaskType.Gather => sounds.sfxGather,
                    TaskType.Farm => sounds.sfxMining,
                    _ => throw new ArgumentOutOfRangeException()
                };

                if (sound)
                {
                    soundDebounce = sound.length;
                    SoundEngine.PlaySFX(sound, Structure.transform, d3: true);
                }
            }
        }

        public override void ReleaseMonkey(MonkeyData monkey)
        {
            Worker.lastWorkedStructure = ProductionConfig.canExecuteContinuously ? null : Structure;
            Worker.Prowess.AddTaskPerformed(TaskType);
            VillageManager.ConsumeActionValue(
                Worker,
                ProductionConfig.aVCost,
                TaskType
            );
            Worker.Self.CurrentJob = null;
        }
    }
}