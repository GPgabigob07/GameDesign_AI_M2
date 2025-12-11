using System;
using AI.Monkey;
using AI.Monkey.Nodes;
using Behaviour_Tree;
using UnityEngine;

namespace Mechanics.Jobs
{
    public class MonkeyPerformJobNode : LeafNode<MonkeyCharacterBT>
    {
        public override NodeResult Process()
        {
            var job = Manager.CurrentJob;
            if (job == null)
            {
                Debug.LogError("Job cannot be null at this stage!!!!");
                return NodeResult.Failure;
            }
            
            //
            if (Manager.debug)
            {
                Debug.Log($"[{GetType().Name}] Job Processed: {job}");
            }
            
            return job.IsFinished && job.HasEnded ? NodeResult.Success : NodeResult.Running;
        }
    }
}