using AI.Monkey.Job;
using Behaviour_Tree;
using Behaviour_Tree.Nodes;

namespace AI.Monkey.Nodes
{
    public class MonkeyBeginJobNode: LeafNode<MonkeyCharacterBT>
    {
        public override NodeResult Process()
        {
            if (!Manager.CurrentJob!.HasBegun)
            {
                Manager.CurrentJob.Begin();
            }

            return NodeResult.Success;
        }
    }
}