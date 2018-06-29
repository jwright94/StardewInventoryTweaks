using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace StardewInventoryTweaks
{
    public class ModEntry : Mod
    {
        private InventoryWatcher inventoryWatcher;
        private InventoryUIHelper inventoryUiHelper;

        private Queue<ItemPos> removedItemQueue = new Queue<ItemPos>();


        public override void Entry(IModHelper helper)
        {
            SaveEvents.AfterLoad += SaveEvents_AfterLoad;
            GameEvents.SecondUpdateTick += GameEventsOnUpdateTick;
            
            //GameEvents
            Monitor.Log("Ayy mod loaded");
        }

        private void SaveEvents_AfterLoad(object sender, EventArgs e)
        {
            inventoryWatcher = new InventoryWatcher(Game1.player.Items);
            inventoryUiHelper = new InventoryUIHelper(this);

            MenuEvents.MenuChanged += (obj, evnt) =>
            {
                inventoryUiHelper.OnMenuChange();
            };

            inventoryWatcher.OnInventoryChange += OnInventoryChanged;
        }

        private void DumpInventory()
        {
            Monitor.Log("=========================================================");
            for (int i = 0; i < Game1.player.Items.Count; i++)
            {
                var item = Game1.player.Items[i];
                if (item != null)
                {
                    Monitor.Log($"{item.Stack}x {item.DisplayName} {item.Category} {item.GetType()} @{i}");
                    var obj = item as StardewValley.Object;
                    if(obj != null)
                        Monitor.Log($"{obj.Quality} {obj.Type}");
                }
            }
        }

        private void OnInventoryChanged()
        {
            Monitor.Log("Inventory Event!");
            Monitor.Log($"Added: {inventoryWatcher.Added.Count}");
            Monitor.Log($"Removed: {inventoryWatcher.Removed.Count}");
            Monitor.Log($"Moved: {inventoryWatcher.Moved.Count}");

            // Check if something was removed
            if (inventoryWatcher.Removed.Count > 0)
            {
                foreach (var removedItem in inventoryWatcher.Removed)
                    removedItemQueue.Enqueue(removedItem);
            }
        }
        
        private void HandleRemovedItem(ItemPos itemPos)
        {
            // if the player is holding the item in the UI don't replace it
            if (inventoryUiHelper.HeldItem == itemPos.Item)
                return;

            // should always be true but you never know
            if (!itemPos.Slot.HasValue)
                return;

            Monitor.Log($"Removed {itemPos.Item.Stack}x {itemPos.Item.DisplayName} from inventory {itemPos.Slot}");

            
            // Make sure the item was in the hotbar and the slot is currently empty
            // Prevents item replace on item swap
            if (itemPos.Slot < 12 && Game1.player.Items[itemPos.Slot.Value] == null)
            {
                // Find if theres anything similar to replace it with
                var similarItems = ItemSimilarity.GetSimilarItems(itemPos.Item, Game1.player.Items);
                var mostSimilarItem = similarItems.FirstOrDefault();
                
                Monitor.Log("=========================================================");
                foreach (var item in similarItems)
                {
                    Monitor.Log($"{item.Similarity}: {item.Item.Stack}x {item.Item.DisplayName} {item.Item.Name}");
                }

                if (mostSimilarItem != null)
                {
                    // Replace the item
                    MoveInventoryItem(Game1.player.getIndexOfInventoryItem(mostSimilarItem.Item),
                        itemPos.Slot.Value);
                }
            }
        }

        private void HandleRemovedItems()
        {
            Monitor.Log("Removed Items Queue");
            foreach (var item in removedItemQueue)
            {
                Monitor.Log($"{item.Item.DisplayName} {item.Slot}");
            }

            while (removedItemQueue.Count > 0)
            {
                var removedItem = removedItemQueue.Dequeue();
                HandleRemovedItem(removedItem);
            }
        }

        private void GameEventsOnUpdateTick(object sender, EventArgs eventArgs)
        {
            inventoryWatcher?.OnUpdate();
            inventoryUiHelper?.OnUpdate();

            if (removedItemQueue.Count > 0)
                HandleRemovedItems();
        }

        private void MoveInventoryItem(int from, int to)
        {
            var temp = Game1.player.Items[to];
            Game1.player.Items[to] = Game1.player.Items[from];
            Game1.player.Items[from] = temp;
        }
    }
}
