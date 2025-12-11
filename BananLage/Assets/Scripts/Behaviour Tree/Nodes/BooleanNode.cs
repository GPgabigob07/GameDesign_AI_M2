using System;

namespace Behaviour_Tree.Nodes
{
    public sealed class BooleanNode<BTM> : LeafNode<BTM> where BTM : BehaviourTreeManager<BTM>
    {
        private readonly Func<BTM, bool> _condition;

        public BooleanNode(Func<BTM, bool> condition, string refereName)
        {
            _condition = condition;
            Name = refereName;
        }

        public override NodeResult Process()
        {
            return _condition(Manager) ? NodeResult.Success : NodeResult.Failure;
        }
    }
}