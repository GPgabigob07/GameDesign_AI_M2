using UnityEngine;

namespace Behaviour_Tree
{
    public abstract class SequenceNode<M> : Node<M>
        where M : BehaviourTreeManager<M>
    {
        public override NodeResult Process()
        {
            foreach (var item in Children)
            {
                var result = item.Process();

                if (Manager.debug)
                {
                    Debug.Log($"[{GetType().Name}_Selector] Processing {item.Name}, result: {result}");
                }
                
                switch (result)
                {
                    case NodeResult.Success: continue;
                    case NodeResult.Failure:
                    case NodeResult.Running:
                        item.Reset();
                        return result;

                    default: return result;
                }
            }

            return NodeResult.Unknown;
        }
    }
}