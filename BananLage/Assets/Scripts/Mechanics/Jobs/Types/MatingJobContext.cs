using Mechanics.Village;
using Structures;
using Structures.Types;

namespace Mechanics.Jobs.Types
{
    public class MatingJobContext: MultiWorkerJobContext
    {

        private MonkeyData[] breed;
        private MonkeyData father, mother;

        public override int CyclesToComplete => 1;

        public MatingJobContext(BaseStructure structure, TaskType taskType) : base(structure, taskType)
        {
        }

        protected override void OnBegin()
        {
            foreach (var m in Monkeys.Values)
            {
                m.Self.Sprite.enabled = false;
            }
            
            VillageManager.ComputeMonkeyBreed(this, father, mother, ref breed);
            
        }

        protected override void OnFinish()
        {
            foreach (var m in Monkeys.Values)
            {
                m.Self.Sprite.enabled = true;
            }

            VillageManager.InstantiateNewMonkeys(Structure, breed);

            if (Structure is LoveHouseStructureController lhsc)
                lhsc.ClearJob();
        }

        protected override void OnTick()
        {
            IsFinished = CyclesElapsed > 0;
        }

        public override bool HasSpace => Monkeys.Count < 2;
        protected override int AvPerWorker => int.MaxValue;
        protected override void OnMonkeyAdded(MonkeyData monkey, int remainingAv)
        {
            if (monkey.IsMale) father = monkey;
            if (monkey.IsFemale) mother = monkey;
        }

        public bool CanJoin(MonkeyData monkey)
        {
            if (!HasSpace) return false;
            
            return (monkey.IsMale && father is null) || (monkey.IsFemale && mother is null); 
        }

        public override bool MonkeyHandshake(BaseStructure structureController)
        {
            return base.MonkeyHandshake(structureController) && father is not null && mother is not null;
        }
    }
}