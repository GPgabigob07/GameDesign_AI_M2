using System;
using System.Collections.Generic;
using UnityEngine;

namespace Structures
{
    [CreateAssetMenu(fileName = "StagedStructureData", menuName = "Mechanics/Structures/StagedStructureData")]
    public class StagedStructureData: ScriptableObject
    {
        public int maxCycles;
        [Range(0f, 1f)]
        public float consumptionChance = 0.5f;

        public List<StructureStage> stages;

        private void OnValidate()
        {
            if (stages == null) return;
            
            maxCycles = Mathf.Max(maxCycles, stages.Count);
        }
    }

    [Serializable]
    public class StructureStage
    {
        public Sprite sprite;
        public float advanceChance = 1f;
    }
}