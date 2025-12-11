using System;
using Mechanics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Structures
{
    public class StructureStageController: MonoBehaviour
    {
        
        BaseStructure _parent;
        StagedStructureData _stage;
        
        [SerializeField]
        private int currentStage, currentCycle;

        public bool Ended => didStart && currentStage == _stage.stages.Count;

        private void Start()
        {
            _parent = GetComponent<BaseStructure>();
            _stage = _parent.StructureData.structureStage;

            if (!_stage)
            {
                Destroy(this);
                return;
            }

            _parent.OnCycleChange += AdvanceStage;
            currentStage = _stage.stages.Count;
            EnforceNextStage();
        }

        private void AdvanceStage()
        {
            if (_parent.JobContext is not null) return;
            
            if (currentStage > _stage.stages.Count) return;
            if (currentCycle++ >= _stage.maxCycles)
            {
                EnforceNextStage();
                return;
            }

            if (Random.value < _stage.stages[Mathf.Clamp(currentStage, 0, _stage.stages.Count -1)].advanceChance)
            {
                ChangeStage();
            }
        }

        private void EnforceNextStage()
        {
            currentStage = Mathf.Clamp(currentStage + 1, 0, _stage.stages.Count - 1);
            ChangeStage();
        }

        public void Begin()
        {
            if (Random.value < _stage.consumptionChance) return;
            
            currentStage = 0;
            currentCycle = 0;
            ChangeStage();
        }

        private void ChangeStage()
        {
            currentCycle = 0;
            var s = _stage.stages[currentStage];
            _parent.SpriteRenderer.sprite = s.sprite;
            currentStage = Mathf.Clamp(currentStage + 1, 0, _stage.stages.Count);
        }
    }
}