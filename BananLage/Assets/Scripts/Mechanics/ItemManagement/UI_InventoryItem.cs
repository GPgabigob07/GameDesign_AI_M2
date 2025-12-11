using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mechanics.ItemManagement
{
    public class UIInventoryItem
    {
        [CreateProperty]
        public readonly ResourceData resource;
        
        [CreateProperty]
        public int quantity => _inventory != null ? _inventory[resource] : -1;
        
        [CreateProperty]
        public readonly int maxQuantity;
        
        [CreateProperty]
        public bool ShouldDisplay =>  quantity > 0;
        
        private readonly Inventory _inventory;
        
        public UIInventoryItem(ResourceData resource, Inventory targetInventory)
        {
            this.resource = resource;
            _inventory = targetInventory;
        }

        [CreateProperty] public string QuantityDisplay => ShouldDisplay ? Utils.ToKnFormat(quantity) : " ";
        [CreateProperty] public Sprite DisplayIcon => ShouldDisplay && resource ? resource.uiSprite : null;
    }
}