using System.Drawing;
using Behaviour_Tree;
using Map;
using Mechanics;
using Mechanics.PathFinding;
using Mechanics.Village;

namespace AI.Monkey.Nodes
{
    public class MonkeyGoingToDie : LeafNode<MonkeyCharacterBT>
    {
        private bool isSet = false;

        public override NodeResult Process()
        {
            if (Manager.CycleData.Hp > 0) return NodeResult.Failure;

            if (isSet && !Manager.Agent.Finished) return NodeResult.Running;
            if (isSet && Manager.Agent.Finished)
            {
                VillageManager.UnregisterWorker(Manager.OriginalData);
                Manager.enabled = false;
                Manager.Animations.PlayDead();
                return NodeResult.Success;
            }

            isSet = true;

            PathManager.FindPathAsync(Manager.transform.position.To2D(),
                MapMatrix.GetInstance().RandomPointFarFromCenter(), Manager.Agent);

            return NodeResult.Running;
        }
    }
}