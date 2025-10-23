using Mechanics;
using Mechanics.Jobs.Types;
using Mechanics.Village;
using Mechanics.Village.Pools;

namespace Structures.Types
{
    public class YieldStructureController: StructureController<ProductionJobContext>
    {
        private ItemOutputProgress[] _progresses;
        
        public void NotifyJobProgress()
        {
            var ctx = _currentJob;
            if (ctx == null) return;

            if (ctx.IsFinished)
            {
                _currentJob = null;
                cooldown = true;
                VillageManager.DismissProgress(ref _progresses);
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
    }
}