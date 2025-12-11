using Mechanics;
using Mechanics.Jobs.Types;
using Mechanics.Village;
using Mechanics.Village.Pools;

namespace Structures.Types
{
    public class YieldStructureController : StructureController<ProductionJobContext>
    {
        private ItemOutputProgress[] _progresses;

        public void NotifyJobProgress()
        {
            var ctx = _currentJob;
            if (ctx == null) return;

            if (ctx.IsFinished)
            {
                _currentJob = null;
                VillageManager.DismissProgress(ref _progresses);

                if (StructureData.structureStage && StageController)
                    StageController.Begin();
                
                return;
            }

            _progresses ??= VillageManager.GetItemProgressDisplay(ctx);
        }

        protected override ProductionJobContext CreateSpecializedJob(MonkeyData monkey)
        {
            var job = new ProductionJobContext(monkey, this, JobType);
            _currentJob = job;
            return job;
        }

        public override bool IsAvailable => _currentJob is null && (!StructureData.structureStage || StageController.Ended);

        public override int CalculateCost()
        {
            return StructureData.productionCycleConfiguration.aVCost;
        }
    }
}