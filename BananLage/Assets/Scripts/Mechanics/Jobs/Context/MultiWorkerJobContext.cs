using System;
using System.Collections.Generic;
using System.Linq;
using Structures;
using UnityEditor;
using UnityEngine;

namespace Mechanics
{
    public abstract class MultiWorkerJobContext : JobContext
    {
        public Dictionary<string, MonkeyData> Monkeys { get; } = new();
        protected Dictionary<string, int> WorkerAVs { get; } = new();
        protected Dictionary<string, float> WorkersTime { get; } = new();
        public abstract bool HasSpace { get; }

        protected abstract int AvPerWorker { get; }

        protected MultiWorkerJobContext(BaseStructure structure, TaskType taskType) : base(structure, taskType)
        {
        }

        public TJob Add<TJob>(MonkeyData worker) where TJob : MultiWorkerJobContext
        {
            if (this is not TJob) throw new Exception("Job context must be of type TJob");
            
            var avs = WorkerAVs.TryAdd(worker.Id, AvPerWorker) ? AvPerWorker : WorkerAVs[worker.Id];
            if (avs <= 0) return null;

            Debug.Log($"Adding monkey {worker.Name}, id: {worker.Id}");

            Monkeys[worker.Id] = worker;
            OnMonkeyAdded(worker, avs);
            return this as TJob;
        }

        /// <summary>
        /// Should setup monkey time, as it depends on cycle configuration
        /// </summary>
        /// <param name="monkey"></param>
        protected abstract void OnMonkeyAdded(MonkeyData monkey, int remainingAv);

        public override void Tick()
        {
            if (CheckFinished()) return;

            var workers = Monkeys.Values;
            if (workers.Count == 0) return;
            if (workers.Any(e => !e.Self)) return;

            if (!HasBegun && MonkeyHandshake(Structure)) Begin();
            if (!HasBegun) return;

            OnTick();
        }

        public override void ReleaseMonkey(MonkeyData monkey)
        {
            var id = monkey.Id;
            if ((Monkeys.ContainsKey(id) && Monkeys.Remove(id)) && ( WorkersTime.ContainsKey(id) && WorkersTime.Remove(id)))
            {
                Debug.Log($"Releasing monkey {monkey.Name} from {this}, id: {id}");
                monkey.lastWorkedStructure = Structure;
                monkey.Self.CurrentJob = null;
                monkey.Prowess.AddTaskPerformed(TaskType);
            }
        }

        private bool CheckFinished()
        {
            if (IsFinished && !HasEnded)
            {
                HasEnded = true;
                OnFinish();
                Monkeys.Values
                    .Where(e => e.Self && e.Self.CurrentJob == this)
                    .ToList()
                    .ForEach(ReleaseMonkey);
            }

            return HasEnded;
        }

        public override bool MonkeyHandshake(BaseStructure structureController)
        {
            var strCtx = structureController.JobContext;
            var monkeysCtx = Monkeys.Values.Select(e => e.Self);
            return monkeysCtx.All(e => 
                e.CurrentJob == strCtx &&
                structureController.InWorkingArea(e.transform.position)
            );
        }

        public int GetAvailableAvFor(MonkeyData monkey)
        {
            return WorkerAVs.GetValueOrDefault(monkey.Id);
        }

        public void Deconstruct(out Dictionary<string, MonkeyData> monkeys, out Dictionary<string, int> avs,
            out Dictionary<string, float> times)
        {
            monkeys = Monkeys;
            avs = WorkerAVs;
            times = WorkersTime;
        }
    }
}