namespace Behaviour_Tree
{
    public abstract class LeafNode<M> : Node<M>
        where M : BehaviourTreeManager<M>
    {
        protected override void CreateChildren()
        {
            return;
        }
    }
}