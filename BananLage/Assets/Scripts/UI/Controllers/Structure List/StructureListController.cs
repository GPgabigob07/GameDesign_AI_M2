using System;
using System.Collections.Generic;
using System.Linq;
using Mechanics.Village;
using Structures;
using UI.Controllers.Object_Details;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Controllers.Structure_List
{
    public class StructureListController : UIFragment
    {
        private static readonly string StructuresPath = "Data/Structures";
        protected override string RootId => "structureListFragment";

        private StructureData _selectedStructure;

        public StructureData SelectedStructure
        {
            get => _selectedStructure;
            private set
            {
                _selectedStructure = value;
                if (value)
                    detailsController.ConfigureBuild(value);
            }
        }

        [SerializeField] private ObjectDetailsController detailsController;
        [SerializeField] private VisualTreeAsset tabTemplate;
        [SerializeField] private VisualTreeAsset structureItemTemplate;
        [SerializeField] private VillageBuildController buildController;
        [SerializeField] private List<StructureTypeIcon> structureTypeIcons;

        private VisualElement _tabs;
        private ScrollView _tabContents;

        private Dictionary<StructureType, List<StructureViewObject>> _viewObjects;
        private StructureTabController _controller;

        protected override void OnCreateView()
        {
            GetResources();

            _controller = new StructureTabController
            {
                Icons = structureTypeIcons.ToDictionary(e => e.type, e => e.icon),
            };

            _tabs = RootElement.Q("buttonsContainer");
            _tabContents = RootElement.Q<ScrollView>();

            _tabs.Clear();
            foreach (var (type, _) in _viewObjects)
            {
                var tab = tabTemplate.CloneTree()[0];
                ConfigureTab(tab, type);
                _tabs.Add(tab);
                Debug.Log($"Created tab {type}");
            }

            ChangeTabTo(0);
        }

        private void GetResources()
        {
            var all = Resources.LoadAll<StructureData>(StructuresPath);
            _viewObjects = new();
            foreach (var data in all)
            {
                if (!data.isBuildable) continue;

                var list = _viewObjects.TryGetValue(data.structureType, out var l) ? l : new();
                list.Add(new StructureViewObject(data));
                _viewObjects[data.structureType] = list;
            }
        }

        private void ConfigureTab(VisualElement tab, StructureType type)
        {
            tab.RegisterCallback<ClickEvent>(_ => ChangeTabTo(type));
            tab.RegisterCallback<ClickEvent>(_ => SoundEngine.PlaySFX(SoundEngine.bus.Sounds.uiClick, transform));

            tab.dataSource = new StructureTabViewObject(type, _controller);
        }

        private void ChangeTabTo(StructureType type)
        {
            _controller.Selected = type;
            _tabContents.Clear();

            foreach (var structureViewObject in _viewObjects[type])
            {
                var item = CreateStructureItemView();
                BindStructureItemView(item, structureViewObject);
                _tabContents.Add(item);
            }
        }

        private void BindStructureItemView(VisualElement e, StructureViewObject vo)
        {
            e.dataSource = vo;
            e.RegisterCallback<ClickEvent>(_ => SelectedStructure = vo.data);
            e.RegisterCallback<ClickEvent>(_ => SoundEngine.PlaySFX(SoundEngine.bus.Sounds.uiClick, transform));
        }

        public void ClearSelection() => SelectedStructure = null;

        private VisualElement CreateStructureItemView()
        {
            return structureItemTemplate.CloneTree()[0];
        }
    }

    public class StructureTabController
    {
        public StructureType Selected;
        public Dictionary<StructureType, Sprite> Icons;

        public bool IsSelected(StructureType type) => type == Selected;
    }

    [Serializable]
    public struct StructureTypeIcon
    {
        public StructureType type;
        public Sprite icon;
    }
}