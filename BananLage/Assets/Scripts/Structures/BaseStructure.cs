using Mechanics;
using Mechanics.Jobs;
using UnityEngine;

namespace Structures
{
    public abstract class BaseStructure : MonoBehaviour, IJobAware
    {
        public abstract JobContext JobContext { get; }
        public abstract TaskType JobType { get; }

        public virtual Vector3 MonkeyWorkingArea => transform.position
                                                    + StructureData.workingAreaOffset.To3D();

        public abstract JobContext CreateJob(MonkeyData monkey);

        [field: SerializeField] public StructureData StructureData { get; set; }
        [field: SerializeField] public SpriteRenderer SpriteRenderer { get; private set; }

        public StructureInstanceData CycleData { get; protected set; }

        protected virtual void Start()
        {
            SpriteRenderer.sprite = StructureData.uiSprite;
        }

        public virtual bool InWorkingArea(Vector3 pos)
        {
            var b = new Bounds(MonkeyWorkingArea, StructureData.worldSize.To3D());
            return b.Contains(pos);
        }

        public virtual bool InStructureArea(Vector3 pos)
        {
            var b = new Bounds(transform.position, StructureData.worldSize.To3D());
            return b.Contains(pos);
        }

        public void RefreshCycleData(StructureInstanceData data)
        {
            CycleData = data;
        }
    }
}