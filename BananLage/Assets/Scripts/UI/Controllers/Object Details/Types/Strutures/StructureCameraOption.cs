using Structures;
using UnityEngine;

namespace UI.Controllers.Object_Details.Types
{
    class StructureCameraOption : IDetailsCameraOptions
    {
        public Vector3 Position { get; }
        public float LensSize { get; }
        
        private readonly BaseStructure _structure;

        public StructureCameraOption(BaseStructure structure, float lensSize)
        {
            _structure = structure;
            Position = structure.transform.position;
            
            var data = structure.StructureData;
            
            var size = Mathf.Max(data.worldSize.x, data.worldSize.y);
            LensSize = lensSize * size;
        }
    }
}