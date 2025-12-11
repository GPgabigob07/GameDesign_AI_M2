using AI.Monkey.Job;
using Behaviour_Tree;
using Mechanics;

namespace AI.Monkey.Nodes
{
    public sealed class MonkeyJobScheduleNode: SequenceNode<MonkeyCharacterBT>
    {
        private readonly TaskType _taskType;

        public MonkeyJobScheduleNode(TaskType taskType)
        {
            _taskType = taskType;
            Name = $"MonkeyJobSchedule_{_taskType}";
        }

        protected override void CreateChildren()
        {
            AddChild(new ReserveStructureForMonkeyNode(_taskType));
            AddChild(MonkeyBooleans.MonkeyHasJob());
            AddChild(new MonkeyMoveTo());
            AddChild(new MonkeyJobEvaluatorNode(_taskType));
        }
    }
}