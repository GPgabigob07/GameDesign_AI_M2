using Mechanics.Village;
using UI.Controllers.Object_Details.Settings;
using UnityEngine.UIElements;

namespace UI.Controllers.Object_Details.Types
{
    class BasicStructureContent : IContentDetails
    {
        private readonly VisualTreeAsset _template;

        public BasicStructureContent(VisualTreeAsset template)
        {
            _template = template;
        }

        public void Update(IObjectDetailsViewObject obj, GameUISettings settings, VisualElement content)
        {
            if (obj is not StructureDetailsViewObject viewObject) return;
            var str =  viewObject.Structure;
            
            var destroy = content.Q("destroyStructure");
            destroy.RegisterCallback<ClickEvent>(_ => VillageManager.DestroyStructure(str));
        }

        public VisualElement GetElement()
        {
            return _template.CloneTree()[0];
        }
    }
}