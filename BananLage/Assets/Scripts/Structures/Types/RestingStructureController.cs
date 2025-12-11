using Mechanics;
using Mechanics.Jobs.Types;
using Mechanics.Village;
using UnityEngine;

namespace Structures.Types
{
    public class RestingStructureController: StructureController<RestingJobContext>
    {
        [SerializeField, Range(0.01f, 1f)]
        private float efficiency = 0.5f;
        public override bool IsAvailable => VillageManager.ImmediateResolve;
        public override int CalculateCost() => 0;

        protected override RestingJobContext CreateSpecializedJob(MonkeyData monkey)
        {
            _currentJob ??= new RestingJobContext(this, StructureData.taskType, efficiency);
            
            _currentJob.Add<RestingJobContext>(monkey);
            return _currentJob;
        }
    }
}