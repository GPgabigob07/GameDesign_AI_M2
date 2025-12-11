using System;
using System.Collections.Generic;
using System.Linq;
using UI.Controllers.Object_Details.Settings;
using Unity.Properties;
using UnityEngine.UIElements;

namespace UI.Controllers.Object_Details.Types.Strutures
{
    class ProductionStructureContent : StructureSpecializedContent
    {
        private readonly VisualTreeAsset _template;

        public ProductionStructureContent(BasicStructureContent basics, VisualTreeAsset template) : base(basics)
        {
            _template = template;
        }

        public override void Update(IObjectDetailsViewObject obj, GameUISettings settings, VisualElement content)
        {
            base.Update(obj, settings, content);
            if (obj is not StructureDetailsViewObject viewObject) return;
            var str = viewObject.Structure;
            var data = new ProductionStructureViewObject
            {
                outputs = str.StructureData.outputs.Select(x => new StructureResourceViewObject
                {
                    data = x.resource,
                    chance = x.chance,
                    amount = x.amount,
                }).ToList()
            };
            
            content.dataSource = data;
        }

        protected override VisualElement GetSpecializedElement()
        {
            return _template.CloneTree()[0];
        }
    }

    [Serializable, GeneratePropertyBag]
    public struct ProductionStructureViewObject
    {
        [CreateProperty]
        public List<StructureResourceViewObject> outputs;
    }
}