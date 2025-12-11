using System;
using Mechanics.Village;
using Unity.Properties;

namespace UI.Controllers
{
    [Serializable, GeneratePropertyBag]
    public class VillageViewObject
    {
        
        [CreateProperty]
        public int MonkeyCount => VillageManager.Monkeys?.Count ?? 0;

        [CreateProperty] public string CurrentCycle => $"CICLO {VillageManager.Cycle}";

    }
}