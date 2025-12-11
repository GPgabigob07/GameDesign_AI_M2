using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace UI
{
    public abstract class UIFragment : MonoBehaviour
    {
        public VisualElement RootElement { get; private set; }
        protected abstract string RootId { get; }

        public void Create(VisualElement activity)
        {
            RootElement = activity.Q(RootId);
            Assert.IsNotNull(RootElement);
            RootElement.pickingMode = PickingMode.Position;

            OnCreateView();
        }

        protected abstract void OnCreateView();
    }
}