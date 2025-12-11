using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Controllers
{
    public class AdvanceCycleModalController: UIFragment
    {
        protected override string RootId => "advanceCycleModal";
        protected override void OnCreateView()
        {
            Hide();
        }

        public void Show()
        {
            RootElement.style.display = DisplayStyle.Flex;
        }
        
        public void Hide()
        {
            RootElement.style.display = DisplayStyle.None;
        }

        public void SetMessage(string message)
        {
            var label = RootElement.Q<Label>();
            if (label != null) label.text = message;
        }
    }
}