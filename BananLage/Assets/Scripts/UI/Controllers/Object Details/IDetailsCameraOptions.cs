using UnityEngine;

namespace UI.Controllers.Object_Details
{
    public interface IDetailsCameraOptions
    {
        public Vector3 Position { get; }
        public float LensSize { get; }
    }
}