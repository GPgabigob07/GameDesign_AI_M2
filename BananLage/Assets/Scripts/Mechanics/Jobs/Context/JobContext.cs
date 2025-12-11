using Structures;

namespace Mechanics
{
    public abstract class JobContext
    {
        public BaseStructure Structure { get; protected set; }
        public TaskType TaskType { get; protected set; }
        public bool IsFinished { get; protected set; }

        public bool HasBegun { get; protected set; }
        public bool HasEnded { get; protected set; }

        public int CyclesElapsed { get; protected set; }
        
        public virtual int CyclesToComplete => 0;

        protected JobContext(BaseStructure structure, TaskType taskType)
        {
            Structure = structure;
            TaskType = taskType;
        }

        public void Deconstruct(out BaseStructure str, out TaskType taskType)
        {
            str = Structure;
            taskType = TaskType;
        }

        public abstract void Tick();

        public void Begin()
        {
            HasBegun = true;
            OnBegin();
        }

        protected virtual void OnBegin()
        {
        }

        protected virtual void OnFinish()
        {
        }

        protected abstract void OnTick();

        public abstract void ReleaseMonkey(MonkeyData monkey);

        public abstract bool MonkeyHandshake(BaseStructure structureController);
        
        public void CycleChanged() => CyclesElapsed++;
    }
}