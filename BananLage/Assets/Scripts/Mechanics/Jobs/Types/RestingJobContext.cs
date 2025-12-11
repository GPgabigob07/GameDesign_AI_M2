using System.Collections.Generic;
using AI.Monkey;
using Mechanics.Village;
using Structures;
using UnityEngine;

namespace Mechanics.Jobs.Types
{
    public class RestingJobContext : MultiWorkerJobContext
    {
        private CycleConfiguration config;
        private float _efficiency;

        private List<string> _idsToRemove = new();

        public RestingJobContext(BaseStructure structure, TaskType taskType, float efficiency) : base(structure,
            taskType)
        {
            config = structure.StructureData.productionCycleConfiguration;
            _efficiency = efficiency;
        }

        protected override void OnTick()
        {
            foreach (var m in Monkeys.Values)
            {
                if (IsMonkeyResting(m.Self))
                    m.Self.Animations.PlayTask(TaskType.Rest);

                /*if (VillageManager.ImmediateResolve || (WorkersTime[m.Id] -= Time.deltaTime) <= 0)
                    _idsToRemove.Add(m.Id);*/
            }

            /*foreach (var id in _idsToRemove)
            {
                if (Monkeys.TryGetValue(id, out var monkey))
                    ReleaseMonkey(monkey);
            }*/

            IsFinished = CyclesElapsed >= 1;
        }

        protected override void OnFinish()
        {
            base.OnFinish();
            //
            HasEnded = false;
            HasBegun = true;
            CyclesElapsed = 0;
            IsFinished = false;
        }

        public override void ReleaseMonkey(MonkeyData monkey)
        {
            base.ReleaseMonkey(monkey);
            VillageManager.RecoverAv(monkey.Id, _efficiency);
            monkey.Self.PutToRest = false;
        }

        public override bool HasSpace => Monkeys.Count < config.maxWorkers;
        protected override int AvPerWorker => int.MaxValue;

        protected override void OnMonkeyAdded(MonkeyData monkey, int remainingAv)
        {
            WorkersTime[monkey.Id] = 10f;
        }

        public bool IsMonkeyResting(MonkeyCharacterBT monkey)
        {
            return Structure.JobContext == monkey.CurrentJob && Structure.InWorkingArea(monkey.transform.position);
        }
    }
}