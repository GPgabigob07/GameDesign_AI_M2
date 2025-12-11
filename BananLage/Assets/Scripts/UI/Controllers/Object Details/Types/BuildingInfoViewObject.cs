using Structures;
using UI.Controllers.Object_Details.Settings;
using UI.Controllers.Object_Details.Types.Strutures;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Controllers.Object_Details.Types
{
    public class BuildingInfoViewObject : IObjectDetailsViewObject
    {
        public StructureData Data { get; }
        public string Name => Data.structureName;
        public Sprite Icon => Data.uiSprite;
        public IDetailsCameraOptions CameraOptions { get; } = null;
        public IContentDetails Content { get; }
        public string Description1 { get; }
        public string Description2 { get; }
        public string Description3 { get; }

        public BuildingInfoViewObject(StructureData data, GameUISettings uiSettings)
        {
            Data = data;
            Content = new ResourceDetailsViewObject(uiSettings.buildingResourcesTemplate.CloneTree()[0]);
        }
    }

    public class ResourceDetailsViewObject : IContentDetails
    {
        private readonly VisualElement _root, _cost, _outputs, _outputsRoot, _vines;

        public ResourceDetailsViewObject(VisualElement root)
        {
            _root = root;
            _cost = root.Q<VisualElement>("costContainer");
            _outputs = root.Q<VisualElement>("outputsContainer");
            _outputsRoot = root.Q<VisualElement>("outputsRoot");
            _vines = root.Q<VisualElement>("subVines");
        }

        public void Update(IObjectDetailsViewObject obj, GameUISettings settings, VisualElement content)
        {
            if (obj is not BuildingInfoViewObject vo) return;

            var str = vo.Data;
            _cost.Clear();
            _outputs.Clear();

            foreach (var buildCost in str.buildCosts)
            {
                var element = settings.productionOutputItemTemplate.CloneTree()[0];
                element.dataSource = new StructureResourceViewObject
                {
                    amount = buildCost.amount,
                    chance = 0f,
                    data = buildCost.resource
                };

                _cost.Add(element);
            }

            if (str.outputs.Length <= 0)
            {
                _vines.style.display = DisplayStyle.None;
                _outputsRoot.style.display = DisplayStyle.None;
                return;
            }

            foreach (var output in str.outputs)
            {
                var element = settings.productionOutputItemTemplate.CloneTree()[0];
                element.dataSource = new StructureResourceViewObject
                {
                    amount = output.amount,
                    chance = output.chance,
                    data = output.resource
                };

                _outputs.Add(element);
            }
        }

        public VisualElement GetElement()
        {
            return _root;
        }
    }
}