using Unity.Properties;
using UnityEngine;

namespace UI.Controllers.Object_Details
{
    public interface IObjectDetailsViewObject
    {
        public string Name { get; }

        public Sprite Icon { get; }

        public IDetailsCameraOptions CameraOptions { get; }
        public IContentDetails Content { get; }

        [CreateProperty] public string Description1 { get; }

        [CreateProperty] public string Description2 { get; }

        [CreateProperty] public string Description3 { get; }
    }
}