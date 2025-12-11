using System.Collections.Generic;
using Mechanics;
using Structures;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class BuildingHotbarController : UIFragment
    {
        private ScrollView structuresScroll;

        [Header("Buildings Container")] [SerializeField]
        private VisualTreeAsset buildingItemTemplate;

        private List<StructureData> Structures;
        private static string StructuresDataPath = "Data/Structures";
        protected override string RootId => "structureScrollView";
        public StructureData SelectedStructure { get; private set; }
        
        protected override void OnCreateView()
        {
            Structures = new List<StructureData>();
            Structures.AddRange(Resources.LoadAll<StructureData>(StructuresDataPath));
        
            structuresScroll = RootElement as ScrollView;
        }
        
        void Start()
        {
            SetupBuildingsList();
        }

        private void SetupBuildingsList()
        {
            structuresScroll.contentContainer.Clear();
            foreach (var structureData in Structures)
            {
                var item = buildingItemTemplate.CloneTree()[0];
                item.dataSource = structureData;
                item.RegisterCallback<ClickEvent>(_ => SelectedStructure = structureData);
                item.RegisterCallback<ClickEvent>(_ => SoundEngine.PlaySFX(SoundEngine.bus.Sounds.uiClick, transform));
                structuresScroll.contentContainer.Add(item);
            }

            structuresScroll.RegisterCallback<PointerEnterEvent>(e => structuresScroll.ToggleInClassList("expanded"));
            structuresScroll.RegisterCallback<PointerLeaveEvent>(e => structuresScroll.RemoveFromClassList("expanded"));
        }

        private void OnCancelBuilding()
        {
            SelectedStructure = null;
        }
    }
}