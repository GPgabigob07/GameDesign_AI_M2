using Behaviour_Tree;
using Mechanics.Village;

namespace AI.Monkey.Nodes
{
    public class MonkeyCollectOutputNode: LeafNode<MonkeyCharacterBT>
    {
        public override NodeResult Process()
        {
            if (Manager.CurrentJob is null or { HasEnded: false }) return NodeResult.Failure;

            //VillageManager.CollectOutputs(Manager.CurrentJob);
            //Manager.CycleData.ActionValue -= Manager.CurrentJob.Structure.StructureData.executionAV;
            Manager.CurrentJob = null;
            return NodeResult.Success;
        }
    }
}