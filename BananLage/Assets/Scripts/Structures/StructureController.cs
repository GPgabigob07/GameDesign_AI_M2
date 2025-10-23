using System;
using JetBrains.Annotations;
using Mechanics;
using Mechanics.Jobs;
using Mechanics.Village;
using UnityEngine;

namespace Structures
{
    public abstract class StructureController<T> : BaseStructure,
        IJobProvider<T>
        where T : JobContext
    {
        public bool cooldown = false;

        [CanBeNull] protected T _currentJob;

        [CanBeNull]
        public T CurrentJob
        {
            set
            {
                if (_currentJob?.Structure || _currentJob != null)
                    throw new Exception("Cannot set job context while another context exists");
                _currentJob = value;
            }
            get => _currentJob;
        }

        public override JobContext JobContext => CurrentJob;

        public override TaskType JobType => StructureData.taskType;

        protected virtual void Start()
        {
            VillageManager.RegisterStructure(this);
        }

        protected virtual void OnDestroy()
        {
            VillageManager.UnregisterStructure(this);
        }

        public int EvaluateAVCost()
        {
            return 0;
        }

        protected abstract T CreateSpecializedJob(MonkeyData monkey);
        public override JobContext CreateJob(MonkeyData monkey) => CreateSpecializedJob(monkey);

        private void OnDrawGizmosSelected()
        {
            if (!StructureData) return;
            //draw work area preview

            var p = transform.position;
            var wc = MonkeyWorkingArea;

            var worldSize = StructureData.worldSize.To3D().ToFloat();
            
            if (worldSize.x % 2 != 0) p.x += .5f;
            if (worldSize.y % 2 != 0) p.y += .5f;
            
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(p, worldSize);
            
            Gizmos.color = new Color(0, 0, 1f, 0.1f);
            Gizmos.DrawCube(p, worldSize);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(wc, 0.45f);

            Gizmos.color = new Color(0.0f, 1f, 0.0f, 0.5f);
            Gizmos.DrawCube(wc, StructureData.workingArea.ToFloat());

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(wc, StructureData.workingArea.ToFloat());
        }
    }
}