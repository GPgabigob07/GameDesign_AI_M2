using Behaviour_Tree.Nodes;
using Mechanics;
using Mechanics.Jobs;

namespace AI.Monkey.Job
{
    public static class MonkeyBooleans
    {
        public static BooleanNode<MonkeyCharacterBT> IsMonkeyWorking() 
            => new(m => m.CurrentJob != null);
        
        public static BooleanNode<MonkeyCharacterBT> IsAboutToWork()
            => new(m => m.CurrentJob != null && !m.CurrentJob.HasBegun);

        public static BooleanNode<MonkeyCharacterBT> CanMonkeyWork() 
            => new(m => m.CycleData.ActionValue > 0);
        
        public static BooleanNode<MonkeyCharacterBT> CanPerformJob()
            => new(m => m.CurrentJob != null 
                        && JobLibrary.CanPerformJob(m.CurrentJob)); 
        public static BooleanNode<MonkeyCharacterBT> IsDead() => new(m => m.CycleData.Hp <= 0);
    }
}