using System;
using System.Collections.Generic;
using System.Linq;
using Structures;
using UnityEngine;

namespace Mechanics.ItemManagement
{
    public class Inventory
    {
        public int maxCapacity;
        private Dictionary<ResourceData, int> _slots = new();
        public static int lastId = 0;
        
        public IReadOnlyDictionary<ResourceData, int> Slots => _slots;
        private int id = lastId++;

        public Inventory()
        {
            maxCapacity = int.MaxValue;
            InventoryManager.Initialize(this);
        }

        public Inventory(int maxCapacity) : this()
        {
            this.maxCapacity = maxCapacity;
        }

        private int TotalItems => _slots.Values.Aggregate(0, (acc, e) => acc + e);
        public bool IsFull => TotalItems >= maxCapacity;

        public bool TryAdd(ResourceData resource, int amount, bool allowPartial = false)
        {
            if (IsFull) return false;

            var overflow = TotalItems + amount > maxCapacity;

            if (!allowPartial && overflow) return false;
            var effectiveAmount = allowPartial && overflow ? maxCapacity - TotalItems : amount;
            
            if (!_slots.TryAdd(resource, effectiveAmount))
                _slots[resource] += effectiveAmount;
            else 
                _slots[resource] = effectiveAmount;
            
            return true;
        }

        public int this[ResourceData resource]
        {
            get => _slots.ContainsKey(resource) ? _slots[resource] : 0;
            set
            {
                if (!_slots.ContainsKey(resource) && TryAdd(resource, value)) return;
                _slots[resource] = value;
            }
        }
        
        public int this[string resourceName]
        {
            get => TryGetResource(resourceName, out var resource) && _slots.ContainsKey(resource) ? _slots[resource] : 0;
            set
            {
                if (!TryGetResource(resourceName, out var resource)) return;
                if (!_slots.ContainsKey(resource) && TryAdd(resource, value)) return;
                _slots[resource] = value;
            }
        }
        
        public bool Has(ResourceData what, int howMuch)
        {
            return this[what] >= howMuch;
        }

        public bool Consume(StructureResourceData[] dataBuildCosts)
        {
            var hasAll = dataBuildCosts.Aggregate(true, (t, e) => t && Has(e.resource, e.amount));
            if (!hasAll) return false;
            
            foreach (var cost in dataBuildCosts)
            {
                this[cost.resource] -= cost.amount;
            }
            return true;
        }

        private bool TryGetResource(string resourceName, out ResourceData resource)
        {
            var clear = resourceName.Trim().ToLowerInvariant();
            var known = _slots.Keys.Select(e => (e.displayName.Trim().ToLowerInvariant(), e));
            foreach (var (name, data) in known)
            {
                if (name != clear) continue;
                resource = data;
                return true;
            }
            
            resource = null;
            return false;
        }
    }
}