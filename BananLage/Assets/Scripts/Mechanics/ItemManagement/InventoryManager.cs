using System;
using System.Collections.Generic;
using System.Linq;
using Mechanics.Village;
using Structures;
using UnityEngine;

namespace Mechanics.ItemManagement
{
    public class InventoryManager : MonoBehaviour
    {
        private const string ResDataPath = "Data/Items";
        private static InventoryManager _instance;
        [SerializeField] private bool debug;

        private Dictionary<string, Inventory> _monkeyInventory = new();
        private Dictionary<string, Inventory> _structureInventory = new();
        private List<ResourceData> _resources = new();
        public static IReadOnlyList<ResourceData> AllResources => _instance._resources.AsReadOnly();

        public static bool InvDebug => _instance && _instance.debug;
        public static bool Awakened => _instance && _instance.didAwake;

        private void Awake()
        {
            if (!_instance)
            {
                _instance = this;
                _resources = Resources.LoadAll<ResourceData>(ResDataPath)
                    .OrderBy(e => !e.basicResource)
                    .ToList();
                DontDestroyOnLoad(this);
                return;
            }

            Destroy(gameObject);
        }

        public static Inventory Register(MonkeyData monkey)
        {
            return Catching(() =>
            {
                if (InvDebug)
                    Debug.Log("Registering monkey inventory");
                
                _instance._monkeyInventory.TryAdd(monkey.Id, new Inventory());
                return _instance._monkeyInventory[monkey.Id];
            });
        }

        public static Inventory Register(BaseStructure structure)
        {
            return Catching(() =>
            {
                if (InvDebug)
                    Debug.Log($"Registering structure inventory {structure}");
                
                _instance._structureInventory.TryAdd(structure.ID, new Inventory());
                return _instance._structureInventory[structure.ID];
            });
        }

        public static Inventory GetInventory(MonkeyData monkey) => Catching(() =>
            _instance._monkeyInventory.TryGetValue(monkey.Id, out var inventory) ? inventory : Register(monkey));

        public static Inventory GetInventory(BaseStructure str)
        {
            try
            {
                return _instance._structureInventory.TryGetValue(str.ID, out var inventory) ? inventory : Register(str);
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static Inventory Unregister(MonkeyData monkey)
        {
            return Catching(() =>
            {
                var inv = GetInventory(monkey);
                _instance._monkeyInventory.Remove(monkey.Id);
                return inv;
            });
        }

        public static Inventory Unregister(BaseStructure str)
        {
            return Catching(() =>
            {
                var inv = GetInventory(str);
                _instance._monkeyInventory.Remove(str.ID);
                return inv;
            });
        }

        public static void DepositAll(Inventory from, Inventory to)
        {
            foreach (var (resource, _) in from.Slots.ToList())
            {
                if (!Deposit(from, to, resource)) break;
            }
        }

        public static bool Deposit(Inventory from, Inventory to, ResourceData what, bool allowPartial = false)
        {
            if (from == VillageManager.VillageInventory) return false;
            var amount = from[what];
            return Move(from, to, what, amount, allowPartial);
        }

        public static bool Move(Inventory from, Inventory to, ResourceData what, int howMuch,
            bool allowPartial = false)
        {
            var current = to[what];
            var succeeded = to.TryAdd(what, howMuch, allowPartial);
            from[what] = to[what] - current;
            return succeeded;
        }

        private static Inventory Catching(Func<Inventory> func)
        {
            try
            {
                return func();
            }
            catch (Exception e)
            {
                Debug.Log("Failure while executing inventory commmand");
                Debug.LogException(e);
                return null;
            }
        }

        internal static void Initialize(Inventory inventory)
        {
            if (!_instance) return;
            
            foreach (var resourceData in AllResources)
            {
                inventory[resourceData] = 0;
            }
        }
    }
}