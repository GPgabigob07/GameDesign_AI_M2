namespace Behaviour_Tree
{
    public class InverterNode<M> : Node<M>
        where M : BehaviourTreeManager<M>
    {
        private readonly Node<M> target;

        public InverterNode(Node<M> target)
        {
            this.target = target;
        }

        public override NodeResult Process()
        {
            var result = target.Process();
            return result switch
            {
                NodeResult.Success => NodeResult.Failure,
                NodeResult.Failure => NodeResult.Success,
                NodeResult.Running => NodeResult.Running,
                _ => NodeResult.Unknown,
            };
        }

        protected override void CreateChildren()
        {
            AddChild(target);
        }
    }
    
}