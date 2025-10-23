using System.Collections.Generic;
using System.Linq;
using Behaviour_Tree;
using Mechanics;
using Mechanics.Village;
using Structures;
using UnityEngine;

namespace AI.Monkey
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
            var structures = VillageManager.AvailableStructuresFor(_task);
            
            var structure = GetNearest(structures);
            
            if (!structure) return NodeResult.Failure;

            Manager.CurrentJob = structure.CreateJob(Manager.CycleData);
            
            return NodeResult.Success;
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