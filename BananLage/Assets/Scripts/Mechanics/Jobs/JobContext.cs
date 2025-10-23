using Structures;

namespace Mechanics
{
    public abstract class JobContext
    {
        public MonkeyData Worker { get; }
        public BaseStructure Structure { get; protected set; }
        public TaskType TaskType { get; protected set; }
        public bool IsFinished { get; protected set; }

        public bool HasBegun { get; private set; }
        public bool HasEnded { get; private set; }

        protected JobContext(MonkeyData monkey, BaseStructure structure, TaskType taskType)
        {
            Worker = monkey;
            Structure = structure;
            TaskType = taskType;
        }

        public void Deconstruct(out MonkeyData monkey, out BaseStructure str, out TaskType taskType)
        {
            monkey = Worker;
            str = Structure;
            taskType = TaskType;
        }

        public void Tick()
        {
            var (m, s, t) = this;
            if (!m.Self) return;

            var mpos = m.Self.transform.position;
            if (!s.InWorkingArea(mpos)) return;

            if (HasEnded) return;
            OnTick();
            
            if (IsFinished && !HasEnded)
            {
                HasEnded = true;
                OnFinish();
                Worker.Self.CurrentJob = null;
            }
        }

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
    }
}