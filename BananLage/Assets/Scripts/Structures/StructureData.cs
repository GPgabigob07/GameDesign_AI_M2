using System;
using Mechanics;
using Unity.Properties;
using UnityEngine;

namespace Structures
{
    [CreateAssetMenu(fileName = "Mechanics", menuName = "Mechanics/StructureData"), Serializable]
    public class StructureData: ScriptableObject
    {
        [CreateProperty(ReadOnly =  true)]
        public string structureName;
        
        [CreateProperty(ReadOnly =  true)]
        public Sprite uiSprite;
        
        public StructureType structureType;
        public StagedStructureData structureStage;
        public StructureResourceData[] buildCosts;
        
        public CycleConfiguration buildConfiguration;
        public bool isBuildable;
        public float TotalBuildTime => buildConfiguration.timePerAv * buildConfiguration.aVCost;
        
        [Header("World Configuration")]
        public Vector2Int worldSize;
        public Vector2Int workingArea;
        public Vector2 workingAreaOffset;

        [Header("Production")]
        public TaskType taskType;
        public TaskAnimation taskAnimation = TaskAnimation.Default;
        public CycleConfiguration productionCycleConfiguration;
        public StructureResourceData[] outputs;
        
        public CycleConfiguration GetConfigFor(TaskType t) => t is TaskType.Build ? buildConfiguration : productionCycleConfiguration;

        public Bounds GetBoundsAt(Vector2 position)
        {
            var xEven = worldSize.x % 2 == 0;
            var yEven = worldSize.y % 2 == 0;

            var aligned = position.ToInt();
            
            var pos = new Vector2(xEven ? aligned.x + 0.5f : aligned.x, yEven ? aligned.y + 0.5f : aligned.y);
            
            return new Bounds(pos, worldSize.To3D().ToFloat() / 2f);
        }
            
    }

    [Serializable]
    public class StructureResourceData
    {
        public int amount;
        public ResourceData resource;
        [Range(0.01f, 1f)]
        public float chance = 1f;
    }

    [Serializable]
    public enum StructureType
    {
        Production, 
        Infrastructure, 
        Combat,
        Misc
    }

    [Serializable]
    public class CycleConfiguration
    {
        public int maxAvPerWorker;
        public int maxWorkers;
        public int maxConcurrentWorkers;
        public int minCycles;
        public int aVCost;
        public bool canExecuteContinuously;

        [Header("Granularity representation")] 
        public float timePerAv;
    }

    public enum TaskAnimation
    {
        Default, 
        Mining
    }
}