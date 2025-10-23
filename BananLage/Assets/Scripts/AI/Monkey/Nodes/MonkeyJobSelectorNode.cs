using System.Collections.Generic;
using Behaviour_Tree;
using Mechanics;
using UnityEngine;

namespace AI.Monkey.Nodes
{
    public class MonkeyJobSelectorNode : Node<MonkeyCharacterBT>
    {
        protected MonkeyData Data => Manager.CycleData;
        internal TaskType? NextTask { get; set; }

        private readonly MonkeyJobEvaluator _evaluator = new();
        private readonly Dictionary<TaskType, MonkeyJobScheduleNode> _jobs = new();

        public override NodeResult Process()
        {
            //maybe create selector|sequence loop node?
            //node would be working + do work
            if (Manager.CurrentJob != null)
            {
                var current = Manager.CurrentJob;
                var task = current.TaskType;
                
                if (Manager.debug)
                {
                    Debug.Log($"[{GetType().Name}] Has Job[{task}], dispatching directly");
                }

                return _jobs.TryGetValue(task, out var currentJob) ? currentJob.Process() : NodeResult.Failure;
            }

            //node not working + loop [choose, evaluate]?
            var time = 0f;
            while (NextTask == null || !_evaluator.Evaluate((TaskType)NextTask, Data, Manager))
            {
                var attempt = Data.Prowess.ResolveNew();
                if (time > 10f || attempt is null)
                {
                    if (Manager.debug)
                        Debug.Log($"[{GetType().Name}] Could not resolve task, bail out");
                    
                    return NodeResult.Failure;
                }
                time += Time.deltaTime;
                NextTask = attempt;
            }
            
            Data.CurrentTask = NextTask;
            if (Manager.debug)
                Debug.Log($"[{GetType().Name}] New Task: {Data.CurrentTask}");
            

            //leaf do work?
            return _jobs.TryGetValue((TaskType)NextTask, out var job) ? job.Process() : NodeResult.Failure;
        }

        protected override void CreateChildren()
        {
            foreach (TaskType task in typeof(TaskType).GetEnumValues())
            {
                var node = new MonkeyJobScheduleNode(task);
                _jobs.Add(task, node);
                AddChild(node);
            }
        }
    }
}