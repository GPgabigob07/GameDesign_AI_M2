using System;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Serialization;

namespace Mechanics
{
    [CreateAssetMenu(fileName = "Mechanics", menuName = "Mechanics/StructureData"), Serializable]
    public class StructureData: ScriptableObject
    {
        [CreateProperty(ReadOnly =  true)]
        public string structureName;
        
        [CreateProperty(ReadOnly =  true)]
        public Sprite uiSprite;
        
        public GameObject gamePrefab;
        public StructureResourceData[] buildCosts;

        [Header("Build Costs (Action Value)")] 
        public float buildTimePerAv = 2f;
        public int buildAV;
        public int executionAV;
        public int maxAvPerCycle;
        public float TotalBuildTime => buildTimePerAv * buildAV;
        
        [Header("World Configuration")]
        public Vector2Int worldSize;
        public Vector2Int workingArea;
        public Vector2Int workingAreaOffset;

        [Header("Production")]
        public TaskType taskType;
        [Range(1f, 10f)]
        public float productionTimeAmp = 1f;
        public bool canProduceContinuously = false;
        public StructureResourceData[] outputs;
    }

    [Serializable]
    public struct StructureResourceData
    {
        public int amount;
        public ResourceData resource;
    }
}