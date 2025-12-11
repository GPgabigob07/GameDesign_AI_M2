using System;
using System.Collections;
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

        protected override IEnumerator Start()
        {
            VillageManager.RegisterStructure(this);
            yield return base.Start();
        }

        protected virtual void Update()
        {
            if (CurrentJob is null) return;
            if (!JobContext.MonkeyHandshake(this)) return;
            
            CurrentJob.Tick();
        }
        
        protected virtual void OnDestroy()
        {
            VillageManager.UnregisterStructure(this);
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
            
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(p, worldSize);
            
            Gizmos.color = new Color(0, 0, 1f, 0.1f);
            Gizmos.DrawCube(p, worldSize);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(wc, 0.1f);
        }
    }
}