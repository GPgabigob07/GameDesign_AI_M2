using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Extras;
using Mechanics;
using Mechanics.ItemManagement;
using Mechanics.Village;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class PlayerInventoryController : UIFragment
    {
        protected override string RootId => "playerInventoryRoot";
        private const string ItemPath = "UI/PlayerInventory/ItemResourceDocument";

        private Dictionary<ResourceData, UIInventoryItem> _items = new();

        private Debounce _expandDebounce;

        private IEnumerable<ResourceData> EssentialItems =>
            _items.Where(e => e.Key.basicResource)
                .Select(e => e.Key);

        private VisualElement _tab, _full;

        private List<VisualElement> _fullResources = new();
        [SerializeField] private int minSlotCount = 3 * 6;

        protected override void OnCreateView()
        {
            _tab = RootElement.Q<VisualElement>("inventoryTab");
            _full = RootElement.Q<ScrollView>("fullInventory").contentContainer;
        }

        private IEnumerator Start()
        {
            _expandDebounce = this.CreateDebounce();

            //wait a few frames so inventories register themselves
            yield return null;
            yield return null;

            RootElement.RegisterCallback<PointerEnterEvent>(Expand);

            RootElement.RegisterCallback<PointerLeaveEvent>(_ => _expandDebounce.Run(.2f, Collapse));

            var allResources = InventoryManager.AllResources;
            var inventory = VillageManager.VillageInventory;

            _items.Clear();
            foreach (var res in allResources)
            {
                _items[res] = GetUI(res, inventory);
            }

            SetFullList();

            yield return new WaitForSecondsRealtime(16.6f * 30 / 1000f); //~30 frames
            RootElement.schedule.Execute(Collapse);
        }

        private void Collapse()
        {
            var offset = RootElement.resolvedStyle.height - _tab.resolvedStyle.height;
            RootElement.style.translate = new Translate(0, offset);
            RootElement.RemoveFromClassList("expanded");
        }

        private void Expand(PointerEnterEvent evt)
        {
            RootElement.style.translate = new Translate(0, 0);
            RootElement.AddToClassList("expanded");
        }

        private void SetFullList()
        {
            _full.Clear();
            var asset = Resources.Load<VisualTreeAsset>(ItemPath);
            foreach (var uiInventoryItem in _items)
            {
                var view = asset.CloneTree()[0];
                view.dataSource = uiInventoryItem.Value;
                _fullResources.Add(view);
                _full.Add(view);
            }

            if (_items.Count >= minSlotCount) return;

            //fill the remaining slots as empty
            for (var i = 0; i < minSlotCount - _items.Count; i++)
            {
                var empty = asset.CloneTree()[0];
                empty.dataSource = new UIInventoryItem(null, null);
                _fullResources.Add(empty);
                _full.Add(empty);
            }
        }

        private UIInventoryItem GetUI(ResourceData res, Inventory villageInventory)
        {
            return _items.TryGetValue(res, out var item) ? item : new UIInventoryItem(res, villageInventory);
        }
    }
}