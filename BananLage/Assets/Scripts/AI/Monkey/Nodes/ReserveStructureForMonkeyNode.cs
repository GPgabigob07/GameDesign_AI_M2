using System.Collections.Generic;
using System.Linq;
using Behaviour_Tree;
using Mechanics;
using Mechanics.Village;
using Structures;
using UnityEngine;

namespace AI.Monkey.Nodes
{
    public class ReserveStructureForMonkeyNode : LeafNode<MonkeyCharacterBT>
    {
        private readonly TaskType _task;

        public ReserveStructureForMonkeyNode(TaskType task)
        {
            _task = task;
        }

        public override NodeResult Process()
        {
            if (Manager.CurrentJob is not null) return NodeResult.Success;
            
            var structure = CheckStructure();

            if (!structure) return NodeResult.Failure;

            Manager.CurrentJob = structure.CreateJob(Manager.CycleData);

            return Manager.CurrentJob is null ? NodeResult.Failure : NodeResult.Success;
        }

        private BaseStructure CheckStructure()
        {
            if (_task is TaskType.Idle)
            {
                var village = VillageManager.AvailableStructuresFor(TaskType.Idle)
                    .First(); // ONLY VILLAGE MANAGER MUST BE IDLE
                
                return village != Manager.CycleData.lastWorkedStructure ? village : null;
            }

            var structures = VillageManager.AvailableStructuresFor(_task)
                .Where(e => e.StructureData.GetConfigFor(_task).canExecuteContinuously ||
                            e != Manager.CycleData.lastWorkedStructure)
                .Where(e => e.IsAvailable)
                .ToList();

            return Random.value < 0.5f ? structures.RandomEntry() : GetNearest(structures);
        }

        private BaseStructure GetNearest(List<BaseStructure> structures)
        {
            if (structures.Count == 0) return null;

            var structure = structures.First();
            var distance = Vector3.Distance(Manager.transform.position, structure.transform.position);
            foreach (var next in structures)
            {
                var dist = Vector3.Distance(Manager.transform.position, next.transform.position);
                if (dist > distance) continue;
                distance = dist;
                structure = next;
            }

            return structure;
        }
    }
}