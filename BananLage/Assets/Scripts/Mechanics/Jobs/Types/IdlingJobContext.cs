using System.Collections.Generic;
using System.Linq;
using Mechanics;
using Mechanics.Village;
using UnityEngine;

namespace Structures.Types
{
    public class IdlingJobContext : MultiWorkerJobContext
    {
        public float ElapsedTime { get; private set; }
        public float CreateTime { get; private set; }
        private List<string> _idsToRemove = new();

        public IdlingJobContext(BaseStructure structure, TaskType taskType) : base(structure, taskType)
        {
            CreateTime = Time.time;
        }

        protected override void OnTick()
        {
            foreach (var id in Monkeys.Keys)
            {
                if (VillageManager.ImmediateResolve)
                {
                    _idsToRemove.Add(id);
                    continue;
                }

                if (Monkeys[id].Self.PutToRest || (WorkersTime[id] -= Time.deltaTime) <= 0)
                    _idsToRemove.Add(id);
                
            }

            foreach (var id in _idsToRemove)
            {
                if (Monkeys[id].ActionValue > VillageManager.IdleAvDeduction)
                    ReleaseMonkey(Monkeys[id]);
            }
            
            _idsToRemove.Clear();
        }

        public override void ReleaseMonkey(MonkeyData monkey)
        {
            monkey.ActionValue = Mathf.Max(monkey.ActionValue - VillageManager.IdleAvDeduction, 0);
            base.ReleaseMonkey(monkey);
        }

        public override bool HasSpace => true;
 
        protected override int AvPerWorker => int.MaxValue;
        protected override void OnMonkeyAdded(MonkeyData monkey, int remainingAv)
        {
            WorkersTime[monkey.Id] = 10f;
            monkey.Self.Animations.Idling();
        }

        public void ForceQuit(MonkeyData data)
        {
            ReleaseMonkey(data);
        }
    }
}