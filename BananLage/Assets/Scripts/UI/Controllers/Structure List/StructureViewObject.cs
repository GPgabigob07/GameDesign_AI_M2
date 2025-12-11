using Structures;
using Unity.Properties;

namespace UI.Controllers.Structure_List
{
    public class StructureViewObject
    {
        [CreateProperty(ReadOnly =  true)]
        public readonly StructureData data;

        [CreateProperty] private int width => 64;
        [CreateProperty] private int height => 64;
        
        public StructureViewObject(StructureData data)
        {
            this.data = data;
        }
    }
}