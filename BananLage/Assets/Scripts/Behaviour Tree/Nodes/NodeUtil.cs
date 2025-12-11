using System;

namespace Behaviour_Tree.Nodes
{
    public static class NodeUtil
    {
        public static InverterNode<T> Invert<T>(this Node<T> node) where T : BehaviourTreeManager<T>
        {
            return new InverterNode<T>(node);
        }

        public static BooleanNode<T> SimpleBoolean<T>(Func<T, bool> condition, string name) where T : BehaviourTreeManager<T>
        {
            return new BooleanNode<T>(condition, name);
        }
        
    }
}