using Structures;
using Unity.Properties;
using UnityEngine;

namespace UI.Controllers.Structure_List
{
    public class StructureTabViewObject
    {
        private readonly StructureType Type;
        private readonly StructureTabController _controller;

        public StructureTabViewObject(StructureType type, StructureTabController controller)
        {
            Type = type;
            _controller = controller;
        }
        
        [CreateProperty]
        public Sprite Icon => _controller.Icons[Type];

        [CreateProperty] public Color TintColor => _controller.IsSelected(Type) ? Color.yellow : Color.white;
    }
}