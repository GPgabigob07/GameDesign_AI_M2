using Structures;

namespace Mechanics
{
    public abstract class SingleWorkerJobContext : JobContext
    {
        public MonkeyData Worker { get; }

        public SingleWorkerJobContext(MonkeyData worker, BaseStructure structure, TaskType taskType) : base(structure,
            taskType)
        {
            Worker = worker;
        }

        public override void Tick()
        {
            if (!Worker.Self) return;

            var begun = Structure.InWorkingArea(Worker.Self.transform.position);
            if (!HasBegun && begun) Begin();
            if (!HasBegun) return;

            if (CheckFinished()) return;
            OnTick();
        }
        
        private bool CheckFinished()
        {
            if (IsFinished && !HasEnded)
            {
                HasEnded = true;
                OnFinish();
                ReleaseMonkey(Worker);
            }

            return HasEnded;
        }
        
        public override bool MonkeyHandshake(BaseStructure structureController)
        {
            var strCtx = structureController.JobContext;
            return Worker.Self.CurrentJob == strCtx && structureController.InWorkingArea(Worker.Self.transform.position);
        }
    }
}