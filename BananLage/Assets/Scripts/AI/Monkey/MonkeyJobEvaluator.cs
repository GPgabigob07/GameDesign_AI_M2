using System.Collections.Generic;
using System.Linq;
using Mechanics;
using Mechanics.Village;
using UnityEngine;

namespace AI.Monkey
{
    public class MonkeyJobEvaluator
    {
        private Dictionary<TaskType, int> _attemps;
        private int totalAttempts;
        
        public bool HasAttempts => totalAttempts > 0;

        public MonkeyJobEvaluator(MonkeyData monkeyData)
        {
            _attemps = new Dictionary<TaskType, int>();
            foreach (var t in typeof(TaskType).GetEnumValues())
            {
                var v = monkeyData.Prowess[(TaskType)t] / 10f;
                var v2 = Mathf.Max(1, v).Floor();
                _attemps.TryAdd((TaskType)t, v2);
                totalAttempts+=v2;
            }
        }

        public TaskType Evaluate(TaskType task, MonkeyData data)
        {
            return TryConsumeAttempt(task, data) ? task : TaskType.Idle;
        }

        private bool TryConsumeAttempt(TaskType task, MonkeyData data)
        {
            totalAttempts--;

            if (task == TaskType.Idle) return true;

            var attempts = _attemps[task]--;
            if (attempts < 0) return false;
            
            if (totalAttempts == 0) return false;
            
            var structures = VillageManager.AvailableStructuresFor(task);
            return structures
                .Where(e => e.StructureData)
                .Where(e => e.StructureData.GetConfigFor(task).canExecuteContinuously || e != data.lastWorkedStructure)
                .Count(e => VillageManager.CanExecuteTask(data, task, e)) > 0;
        }
        
    }
}