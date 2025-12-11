using UI.Controllers.Object_Details.Settings;
using UnityEngine.UIElements;

namespace UI.Controllers.Object_Details
{
    public interface IContentDetails
    {
        public void Update(IObjectDetailsViewObject obj, GameUISettings settings, VisualElement content);
        public VisualElement GetElement();
    }
}