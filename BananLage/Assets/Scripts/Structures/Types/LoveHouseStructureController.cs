using Mechanics;
using Mechanics.Jobs.Types;

namespace Structures.Types
{
    public class LoveHouseStructureController : StructureController<MatingJobContext>
    {
        public override bool IsAvailable => _currentJob is null or { HasSpace: true, HasEnded: false };

        protected override void Update()
        {
            base.Update();
        }
        
        public override int CalculateCost()
        {
            return StructureData.productionCycleConfiguration.aVCost;
        }

        protected override MatingJobContext CreateSpecializedJob(MonkeyData monkey)
        {
            _currentJob ??= new MatingJobContext(this, StructureData.taskType);

            return _currentJob.CanJoin(monkey) ? _currentJob.Add<MatingJobContext>(monkey) : null;
        }

        public void ClearJob()
        {
            _currentJob = null;
        }
    }
}