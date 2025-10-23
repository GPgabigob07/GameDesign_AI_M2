using System;

namespace Behaviour_Tree.Nodes
{
    public class BooleanNode<BTM> : LeafNode<BTM> where BTM : BehaviourTreeManager<BTM>
    {
        private readonly Func<BTM, bool> _condition;

        public BooleanNode(Func<BTM, bool> condition)
        {
            _condition = condition;
        }

        public override NodeResult Process()
        {
            return _condition(Manager) ? NodeResult.Success : NodeResult.Failure;
        }
    }
}