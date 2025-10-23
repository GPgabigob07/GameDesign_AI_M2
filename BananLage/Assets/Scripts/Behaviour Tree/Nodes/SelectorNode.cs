using UnityEngine;

namespace Behaviour_Tree
{
    public abstract class SelectorNode<M> : Node<M>
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
                    case NodeResult.Failure: continue;

                    case NodeResult.Success:
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