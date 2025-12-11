using System;
using System.Collections.Generic;
using System.Linq;
using Mechanics;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Controllers.Object_Details.Settings
{
    [CreateAssetMenu(fileName = "UISettings", menuName = "Object Details/Monkey Settings")]
    public class GameUISettings: ScriptableObject, ISerializationCallbackReceiver
    {
        private Dictionary<TaskType, ProwessUI> _prowess;
        public float lensSizePerUnit = 0.65f;
        
        [Header("Monkey")]
        public List<ProwessUI> prowessSetting;
        public VisualTreeAsset prowessTemplate;
        public VisualTreeAsset monkeyTemplate;

        [Header("Structure")] public VisualTreeAsset productionStructureTemplate;
        public VisualTreeAsset structureBasicsTemplate;
        public VisualTreeAsset productionOutputItemTemplate;
        public VisualTreeAsset buildingResourcesTemplate;

        public void OnBeforeSerialize()
        {
            prowessSetting = _prowess.Values.OrderBy(e => e.type).ToList();
        }

        public void OnAfterDeserialize()
        {
            _prowess = new();
            foreach (TaskType enumValue in typeof(TaskType).GetEnumValues())
            {
                _prowess[enumValue] = null;
            }
            
            prowessSetting ??= new();
            foreach (var prowessUI in prowessSetting)
            {
                _prowess[prowessUI.type] = prowessUI;
            }
        }

        public Sprite GetProwessIcon(TaskType task)
        {
            return  _prowess[task].icon;
        }
    }

    [Serializable]
    public class ProwessUI
    {
        public TaskType type;
        public Sprite icon;
    }
}