using Behaviour_Tree;
using Behaviour_Tree.Nodes;
using Mechanics;
using Mechanics.Jobs;
using Mechanics.Village;

namespace AI.Monkey.Job
{
    public static class MonkeyBooleans
    {
        public static BooleanNode<MonkeyCharacterBT> MonkeyHasJob()
            => new(m => 
                m.CurrentJob is not null,
                "MonkeyHasJob");

        public static BooleanNode<MonkeyCharacterBT> IsAboutToWork()
            => new(m => 
                m.CurrentJob is { HasBegun: false },
                "IsAboutToWork");

        public static BooleanNode<MonkeyCharacterBT> CanMonkeyWork()
            => new(m => m.CycleData.ActionValue > 0, "CanMonkeyWork");
        
        public static BooleanNode<MonkeyCharacterBT> IsMonkeyResting()
            => new(m => m.IsResting, "CanMonkeyWork"); 
        
        public static BooleanNode<MonkeyCharacterBT> WantsToRest()
            => new(m => m.PutToRest, "CanMonkeyWork");

        public static BooleanNode<MonkeyCharacterBT> CanPerformJob()
            => new(m => m.CurrentJob != null
                        && JobLibrary.CanPerformJob(m.CurrentJob),
                "CanPerformJob");

        public static BooleanNode<MonkeyCharacterBT> IsDead() => new(m => 
            m.CycleData.Hp <= 0 && !m.GoDie,
            "IsDead");
        
        public static BooleanNode<MonkeyCharacterBT> NeedFinishJob() => new(m =>
                m.CurrentJob is not null  
            , "NeedFinishJob");
    }
}