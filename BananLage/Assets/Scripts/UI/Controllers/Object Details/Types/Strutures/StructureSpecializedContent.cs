using UI.Controllers.Object_Details.Settings;
using UnityEngine.UIElements;

namespace UI.Controllers.Object_Details.Types
{
    abstract class StructureSpecializedContent : IContentDetails
    {
        protected readonly BasicStructureContent Basics;

        protected StructureSpecializedContent(BasicStructureContent basics)
        {
            Basics = basics;
        }

        public virtual void Update(IObjectDetailsViewObject obj, GameUISettings settings, VisualElement content)
        {
            Basics.Update(obj, settings, content);
        }

        public VisualElement GetElement()
        {
            var ve = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                    flexGrow = 0,
                    paddingBottom = 2,
                    paddingLeft = 2,
                    paddingRight = 2,
                    paddingTop = 2,
                },
            };
            
            ve.Add(Basics.GetElement());
            ve.Add(GetSpecializedElement());
            
            return ve;
        }

        protected abstract VisualElement GetSpecializedElement();
    }
}