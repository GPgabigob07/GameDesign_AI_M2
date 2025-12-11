using System.Collections.Generic;
using Behaviour_Tree;
using Mechanics;
using Mechanics.Village;
using Structures.Types;
using UnityEngine;

namespace AI.Monkey.Nodes
{
    public class MonkeyJobSelectorNode : Node<MonkeyCharacterBT>
    {
        protected MonkeyData Data => Manager.CycleData;

        private MonkeyJobEvaluator _evaluator;
        private readonly Dictionary<TaskType, MonkeyJobScheduleNode> _jobs = new();

        public override NodeResult Process()
        {
            //maybe create selector|sequence loop node?
            //node would be working + do work
            var toRest = Manager.PutToRest;
            if (Manager.CurrentJob != null)
            {
                var result = TryDispatchCurrentJob();
                if (!toRest) return result;
                
                if (Manager.CurrentJob is IdlingJobContext idle) // allow idling monkeys to go to rest without "finish idling"
                    idle.ForceQuit(Data);
                
            }
                                    //no job, village skipping fallback
            var nextTask = (toRest || VillageManager.ImmediateResolve) ? TaskType.Rest : ResolveNextTask();          

            Data.CurrentTask = nextTask;
            if (Manager.debug)
                Debug.Log($"[{Data.Name}] New Task: {Data.CurrentTask}");
            
            //leaf do work?
            return _jobs.TryGetValue(nextTask, out var job) ? job.Process() : NodeResult.Failure;
        }

        private NodeResult TryDispatchCurrentJob()
        {
            var current = Manager.CurrentJob;
            var task = current.TaskType;

            if (Manager.debug)
            {
                Debug.Log($"[{GetType().Name}] Has Job[{task}], dispatching directly");
            }

            return _jobs.TryGetValue(task, out var currentJob) ? currentJob.Process() : NodeResult.Failure;
        }

        private TaskType ResolveNextTask()
        {
            var nextTask = Data.Prowess.ResolveNew(Manager.debug);
            if (Data.ActionValue < 1) nextTask = TaskType.Idle;
            else
            {
                _evaluator = new MonkeyJobEvaluator(Data);
                var time = 0f;
                while (_evaluator.HasAttempts)
                {
                    var candidate = Data.Prowess.ResolveNew(Manager.debug);
                    nextTask = _evaluator.Evaluate(candidate, Data);
                    if (nextTask != TaskType.Idle) break;
                }
            }
            
            return nextTask;
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