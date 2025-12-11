using System;
using Structures;

namespace Mechanics.Production
{
    [Serializable]
    public class ResourceOutput
    {
        public StructureData source;
        public MonkeyData worker;
        public ResourceData output;
        public int expectedAmount;
        public int effectiveAmount;

        public ResourceData Output => output;
        public float Progress { get; set; }
    }
}