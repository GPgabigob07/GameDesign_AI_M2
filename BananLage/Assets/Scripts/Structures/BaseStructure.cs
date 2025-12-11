using System;
using System.Collections;
using System.Net.Http.Headers;
using Mechanics;
using Mechanics.ItemManagement;
using Mechanics.Jobs;
using Mechanics.Village;
using UnityEngine;

namespace Structures
{
    public abstract class BaseStructure : MonoBehaviour, 
        IJobAware
    {
        public abstract JobContext JobContext { get; }
        public abstract TaskType JobType { get; }

        public virtual Vector3 MonkeyWorkingArea => transform.position
                                                    + StructureData.workingAreaOffset.To3D();
        
        public abstract JobContext CreateJob(MonkeyData monkey);

        public Inventory Inventory => InventoryManager.GetInventory(this);
        
        internal event Action OnCycleChange;
        
        public abstract bool IsAvailable { get; }

        [field: SerializeField] public StructureData StructureData { get; set; }
        [field: SerializeField] public SpriteRenderer SpriteRenderer { get; private set; }
    
        
        protected StructureStageController StageController {get; private set;}
        
        private BoxCollider2D _boxCollider;

        public StructureInstanceData CycleData { get; protected set; }
        public string ID { get; } = Guid.NewGuid().ToString();

        protected virtual IEnumerator Start()
        {
            if (StructureData == null) yield break;
            
            if (SpriteRenderer)
                SpriteRenderer.sprite = StructureData.uiSprite;
            
            if (!gameObject.TryGetComponent(out _boxCollider))
                _boxCollider = gameObject.AddComponent<BoxCollider2D>();

            if (StructureData.structureStage)
                StageController = gameObject.AddComponent<StructureStageController>();
            
            ConfigureHitbox(_boxCollider);

            var pos = transform.position;
            pos.z = 0;
            transform.position = pos;
            
            yield return null;
            MapController.Assign(this);
        }

        protected virtual void ConfigureHitbox(BoxCollider2D box)
        {
            box.size = StructureData.worldSize;
        }

        public virtual bool InWorkingArea(Vector3 pos)
        {
            var b = new Bounds(MonkeyWorkingArea, StructureData.workingArea.To3D());
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

        private void OnValidate()
        {
            if (Application.isPlaying) return;
            
            if (SpriteRenderer && StructureData) SpriteRenderer.sprite = StructureData.uiSprite;
        }

        public abstract int CalculateCost();

        public virtual void OnCycleChanged()
        {
            JobContext?.CycleChanged();
            OnCycleChange?.Invoke();
        }

        private void OnDestroy()
        {
            VillageManager.Map.Unassign(this);
        }
    }
}