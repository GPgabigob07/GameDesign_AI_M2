using System;
using AI.Monkey;
using AI.Monkey.Nodes;
using Behaviour_Tree;
using UnityEngine;

namespace Mechanics.Jobs
{
    public class MonkeyPerformJobNode : LeafNode<MonkeyCharacterBT>
    {

        private Action<MonkeyCharacterBT, JobContext> _dispatcher;
        
        public void SetDispatcher(Action<MonkeyCharacterBT, JobContext> dispatcher)
        {
            if (_dispatcher != null) throw new Exception("Can't set dispatcher twice");
            _dispatcher = dispatcher;
        }

        public override NodeResult Process()
        {
            var job = Manager.CurrentJob;
            if (job == null)
            {
                Debug.LogError("Job cannot be null at this stage!!!!");
                return NodeResult.Failure;
            }
            
            job.Tick();
            if (Manager.debug)
            {
                Debug.Log($"[{GetType().Name}] Job Processed: {job}");
            }
            
            return job.IsFinished && job.HasEnded ? NodeResult.Success : NodeResult.Running;
        }
    }
}