using System.Linq;
using Mechanics;
using Mechanics.Village;

namespace AI.Monkey
{
    public class MonkeyJobEvaluator
    {
        public bool Evaluate(TaskType task, MonkeyData data, MonkeyCharacterBT manager)
        {
            var structures = VillageManager.AvailableStructuresFor(task);
            return structures.Count(e => e.StructureData.executionAV <= data.ActionValue) > 0;
        }
    }
}