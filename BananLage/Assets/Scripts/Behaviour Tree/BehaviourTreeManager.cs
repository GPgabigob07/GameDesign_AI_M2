using UnityEngine;

namespace Behaviour_Tree
{
    public abstract class BehaviourTreeManager<Self> : MonoBehaviour
        where Self : BehaviourTreeManager<Self>
    {
        public bool debug = false;
        public bool nodeDebug = false;
        public abstract Node<Self> Root { get; protected set; }
        protected virtual void Start()
        {
            Root?.Setup(this as Self);
        }

        // Update is called once per frame
        void Update()
        {
            Tick();
        }

        protected virtual void OnPreTick()
        {
        }

        protected virtual void OnPostTick(NodeResult tickResult)
        {
        }

        private void Tick()
        {
            if (Root == null) return;
            OnPreTick();

            var result = Root.Process();

            switch (result)
            {
                case NodeResult.Unknown:
                    return;
                case NodeResult.Success:
                    break;
                case NodeResult.Running:
                    break;
                case NodeResult.Failure:
                    break;
            }

            OnPostTick(result);
        }
    }

    public enum NodeResult
    {
        Unknown,
        Success,
        Running,
        Failure
    }
}