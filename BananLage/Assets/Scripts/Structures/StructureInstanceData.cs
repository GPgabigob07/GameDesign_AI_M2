namespace Structures
{
    public class StructureInstanceData
    {
        public StructureState State;

        public bool IsBuilt => !IsDestroyed && State >= StructureState.Built;
        public bool CanBuild => State == StructureState.Placed;

        public bool Busy => IsBuilt && State == StructureState.Working;
        public bool CanWork => IsBuilt && State == StructureState.Idling;
        
        public bool IsDestroyed => State == StructureState.Destroyed;

        public int AvalableAV;
    }

    //Represents Structure Lifecycle-aware state
    //Linearly: Placed -> Built -> [Idling <-> Working] -> Destroyed
    public enum StructureState
    {
        Placed,
        Built,
        Idling,
        Working,
        Destroyed
    }
}