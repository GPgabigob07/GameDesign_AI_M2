using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mechanics.Village;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Mechanics
{
    [Serializable]
    public class MonkeyProwess : 
        IEquatable<MonkeyProwess>,
        ISerializationCallbackReceiver, 
        IEnumerable<ProwessTaskEntry>
    {

        #region Direct Access Stats

        public int Combat
        {
            get => this[TaskType.Combat];
            set => this[TaskType.Combat] = value;
        }

        public int Build
        {
            get => this[TaskType.Build];
            set => this[TaskType.Build] = value;
        }

        public int Appeal
        {
            get => this[TaskType.Mate];
            set => this[TaskType.Mate] = value;
        }

        public int Farm
        {
            get => this[TaskType.Farm];
            set => this[TaskType.Farm] = value;
        }

        public int Gather
        {
            get => this[TaskType.Gather];
            set => this[TaskType.Gather] = value;
        }
        
        public int Idle
        {
            get => this[TaskType.Idle];
            set => this[TaskType.Idle] = value;
        }

        public int Total => indexer.Values.Aggregate(0, (i, entry) => i + entry.points);

        #endregion

        private Dictionary<TaskType, ProwessTaskEntry> indexer;

        [SerializeField] private List<ProwessTaskEntry> taskEntries;

        private Dictionary<TaskType, int> executedTasksThisCycle;

        public MonkeyProwess()
        {
            indexer ??= new Dictionary<TaskType, ProwessTaskEntry>();
            executedTasksThisCycle ??= new Dictionary<TaskType, int>();
            taskEntries ??= new List<ProwessTaskEntry>();
            foreach (TaskType task in typeof(TaskType).GetEnumValues())
            {
                AddProwess(task);
            }
        }

        public TaskType Most()
        {
            var stats = taskEntries;
            var highest = stats.First();
            highest = stats.Aggregate(highest, (current, stat) => stat.points > current.points ? stat : current);

            return highest.task;
        }

        public TaskType Least()
        {
            var stats = taskEntries;
            var lowest = stats.First();
            lowest = stats.Aggregate(lowest, (current, stat) => stat.points < current.points ? stat : current);

            return lowest.task;
        }

        public TaskType ResolveNew(bool debug = false)
        {
            var qtdWeight = VillageManager.GetComputedDecisionWeightsByQuantity();
            var necessityWeight = VillageManager.GetComputedDecisionWeightsByNecessity();
            
            var totalWeights = taskEntries
                .Where(e => e.enabled)
                .Select(e => (e.points * qtdWeight[e.task] * necessityWeight[e.task]).Floor())
                .Aggregate(0f, (total, stat) => total + stat);
            
            Allow(TaskType.Idle, true);
            
            var dice = Random.value;
            var sum = 0f;
            var attempts = taskEntries.Count(e => e.enabled);
            var next = TaskType.Idle;
            var iterator = taskEntries.GetEnumerator();
            var time = 0f;
            while (iterator.MoveNext())
            {
                if ((time += Time.deltaTime) > 3f) break;//failsafe
                
                var  item = iterator.Current;
                if (item == null) continue;
                
                sum += (item.points * qtdWeight[item.task]).Floor() / totalWeights;
                if (debug)
                    Debug.Log($"[Prowess] Resolving... {item.task}[{item.points / totalWeights}][enabled:{item.enabled}]: {sum}, dice={dice}");
                
                if (sum < dice) continue;
                
                if (indexer.TryGetValue(item.task, out var e) && e.enabled)
                {
                    if (debug)
                        Debug.Log($"[Prowess] Resolved! {item.task}[{item.points / totalWeights}][enabled:{item.enabled}]: {sum}, dice={dice}");
                    next = item.task;
                    break;
                }
                
                attempts--;
                dice = Random.value;
                sum = 0f;
                iterator.Dispose();
                iterator = taskEntries.GetEnumerator();
            }
            
            return next;
        }

        public int this[TaskType task]
        {
            get => indexer.TryGetValue(task, out var entry) ? entry.points : AddProwess(task).points;
            set => indexer[task].points = value;
        }

        private ProwessTaskEntry AddProwess(TaskType task)
        {
            var entry = new ProwessTaskEntry
            {
                enabled = true, points = 0, task = task
            };
            indexer.Add(task, entry);
            taskEntries.Add(entry);
            return entry;
        }

        public void AddTaskPerformed(TaskType task)
        {
            Debug.Log($"[Prowess] Added task {task} +1, current = {executedTasksThisCycle.GetValueOrDefault(task)}");
            if (executedTasksThisCycle.TryGetValue(task, out var count))
            {
                executedTasksThisCycle[task] = count + 1;
                return;
            }
            
            executedTasksThisCycle.Add(task, 1);
        }

        public bool Can(TaskType task) => indexer.TryGetValue(task, out var entry) && entry.enabled;
        public bool Allow(TaskType task, bool allow) => indexer.TryGetValue(task, out var e) && (e.enabled = allow);

        public void OnBeforeSerialize()
        {
            taskEntries = indexer.Values.OrderBy(e => (int)e.task).ToList();
        }

        public void OnAfterDeserialize()
        {
            indexer = new();
            taskEntries ??= new();
            
            foreach (var prowessTaskEntry in taskEntries)
            {
                indexer[prowessTaskEntry.task] = prowessTaskEntry;
            }
        }

        public int Executed(TaskType task) => executedTasksThisCycle.GetValueOrDefault(task);

        public bool Equals(MonkeyProwess other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(indexer, other.indexer) && Equals(taskEntries, other.taskEntries);
        }

        public IEnumerator<ProwessTaskEntry> GetEnumerator()
        {
            return taskEntries.GetEnumerator();
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((MonkeyProwess)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(indexer, taskEntries);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public MonkeyProwess Refresh()
        {
            var clone = Clone();
            
            var config = VillageManager.GetMinimumExecutionForIncrease().Clone();
            foreach (var p in config.taskEntries)
            {
                var increase = clone.executedTasksThisCycle.GetValueOrDefault(p.task) / config[p.task];
                clone[p.task] += increase;
            }

            clone.executedTasksThisCycle.Clear();
            
            return clone;
        }

        public MonkeyProwess Clone()
        {
            return new MonkeyProwess
            {
                executedTasksThisCycle = executedTasksThisCycle,
                indexer = indexer,
                taskEntries = taskEntries,
            };
        }
        
        public static MonkeyProwess operator +(MonkeyProwess one, MonkeyProwess other)
        {
            var p = new MonkeyProwess();
            foreach (TaskType task in typeof(TaskType).GetEnumValues())
            {
                p[task] =  one[task] + other[task];
            }

            return p;
        }
    }
    
    [Serializable]
    public class ProwessTaskEntry
    {
        public TaskType task;
        public int points;
        public bool enabled;
    }

    [Serializable]
    public struct ProwessProductionAmplification
    {
        public TaskType task;
        public List<PPA_Ranges> ranges;
    }

    [Serializable]
    public struct PPA_Ranges
    {
        [Range(0, 100)] public int upTo;
        public float amplification;
    }
    
}