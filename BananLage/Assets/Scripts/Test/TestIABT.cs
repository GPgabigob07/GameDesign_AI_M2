using Behaviour_Tree;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Test
{
    public class TestIABT : BehaviourTreeManager<TestIABT>
    {
        public override Node<TestIABT> Root { get; protected set; } = new TestSequence();
        public LayerMask LayerMask;

        public Vector2 Direction = Vector2.right;
        public float Distance, Speed;
        public Rigidbody2D rb;

        protected override void OnPostTick(NodeResult tickResult)
        {
            Debug.Log($"TestIABT::OnPostTick = {tickResult}");
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, Direction * Distance);
        }
    }

    public class TestSequence : SelectorNode<TestIABT>
    {
        protected override void CreateChildren()
        {
            AddChild(new CheckMove());
            AddChild(new Move());
        }
    }

    public class CanMove : LeafNode<TestIABT>
    {
        public override NodeResult Process()
        {
            var results = new RaycastHit2D[1];
            Physics2D.Raycast(
                Manager.transform.position,
                Manager.Direction,
                new ContactFilter2D
                {
                    useLayerMask = true,
                    layerMask = Manager.LayerMask,
                }, results, Manager.Distance
            );

            Debug.Log($"CanMove::Process = {(results[0].collider ? NodeResult.Failure : NodeResult.Success)}");
            return results[0] ? NodeResult.Failure : NodeResult.Success;
        }
    }

    public class Move : LeafNode<TestIABT>
    {
        public override NodeResult Process()
        {
            Manager.rb.position += (Manager.Direction * (Manager.Speed * Time.deltaTime));
            return NodeResult.Running;
        }
    }

    public class ChangeDirection : LeafNode<TestIABT>
    {
        public override NodeResult Process()
        {
            Manager.Direction = Quaternion.Euler(0, 0, Random.value * 359) * Manager.Direction;
            return NodeResult.Success;
        }
    }
    
    public class UpgradeSpeed : LeafNode<TestIABT>
    {
        public override NodeResult Process()
        {
            Manager.Speed *= 1.02f;
            return NodeResult.Success;
        }
    }

    public class CheckMove : SequenceNode<TestIABT>
    {
        protected override void CreateChildren()
        {
            AddChild(new InverterNode<TestIABT>(new CanMove()));
            AddChild(new ChangeDirection());
            AddChild(new UpgradeSpeed());
        }
    }
}