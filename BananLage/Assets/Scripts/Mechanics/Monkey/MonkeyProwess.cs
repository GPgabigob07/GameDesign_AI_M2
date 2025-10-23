using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Mechanics
{
    [Serializable]
    public class MonkeyProwess : IEquatable<MonkeyProwess>, ISerializationCallbackReceiver
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

        #endregion

        private Dictionary<TaskType, ProwessTaskEntry> indexer;

        [SerializeField] private List<ProwessTaskEntry> taskEntries;

        public MonkeyProwess()
        {
            indexer ??= new Dictionary<TaskType, ProwessTaskEntry>();
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

        public TaskType? ResolveNew()
        {
            var totalWeights = taskEntries
                .Select(e => e.points)
                .Aggregate(0f, (total, stat) => total + stat);
            
            var dice = Random.value;
            var sum = 0f;
            var attempts = taskEntries.Count(e => e.enabled);
            for (var i = 0; i < indexer.Count || attempts > 0; i++)
            {
                var item = indexer.ElementAt(i).Value;
                sum += item.points / totalWeights;
                if (sum < dice) continue;
                if (indexer.TryGetValue(item.task, out var e) && e.enabled)
                    return item.task;

                attempts--;
                dice = Random.value;
                sum = 0f;
                i = 0;
            }

            return null;
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

        public bool Equals(MonkeyProwess other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(indexer, other.indexer) && Equals(taskEntries, other.taskEntries);
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