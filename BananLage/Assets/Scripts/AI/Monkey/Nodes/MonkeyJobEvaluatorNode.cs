using AI.Monkey.Job;
using Behaviour_Tree;
using Mechanics;
using Mechanics.Jobs;

namespace AI.Monkey.Nodes
{
    public class MonkeyJobEvaluatorNode: SequenceNode<MonkeyCharacterBT>
    {
        private readonly TaskType _taskType;

        public MonkeyJobEvaluatorNode(TaskType taskType)
        {
            _taskType = taskType;
        }

        protected override void CreateChildren()
        {
            AddChild(MonkeyBooleans.IsMonkeyWorking());
            AddChild(new MonkeyBeginJobNode());
            AddChild(MonkeyBooleans.CanPerformJob());
            AddChild(JobLibrary.GetJobNode(_taskType));
        }
    }
}