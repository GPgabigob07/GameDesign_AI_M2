using Mechanics;
using Structures;
using UI.Controllers.Object_Details.Settings;
using UI.Controllers.Object_Details.Types.Strutures;
using UnityEngine;
using UnityEngine.Rendering;

namespace UI.Controllers.Object_Details.Types
{
    public class StructureDetailsViewObject:  IObjectDetailsViewObject
    {
        public string Name { get; }
        public Sprite Icon { get; }
        public IDetailsCameraOptions CameraOptions { get; }
        public IContentDetails Content { get; }
        public string Description1 { get; }
        public string Description2 { get; }
        public string Description3 { get; }

        public readonly BaseStructure Structure;

        public StructureDetailsViewObject(BaseStructure structure, GameUISettings settings, bool preview)
        {
            Structure = structure;
            Name = structure.StructureData.structureName;
            Icon = preview ? structure.StructureData.uiSprite : null;
            CameraOptions = !preview ? CreateCamera(structure, settings) : null;
            Content = CreateSpecializedContent(structure, settings);
        }

        private StructureSpecializedContent CreateSpecializedContent(BaseStructure structure, GameUISettings settings)
        {
            var basics = new BasicStructureContent(settings.structureBasicsTemplate);
            return structure.JobType switch
            {
                TaskType.Farm => new ProductionStructureContent(basics, settings.productionStructureTemplate),
                _ => null,
            };
        }
        
        private IDetailsCameraOptions CreateCamera(BaseStructure structure, GameUISettings settings)
        {
            return new StructureCameraOption(structure, settings.lensSizePerUnit);
        }
    }
}