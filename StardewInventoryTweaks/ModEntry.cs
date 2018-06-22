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
        private class RemovedItem
        {
            public Item Item;
            public int? Slot;
        }

        private GameMenu gameMenu;
        private InventoryPage inventoryPage;

        private IReflectedField<Item> heldItemReflectedField;

        private Item HeldItem
        {
            get { return heldItemReflectedField?.GetValue(); }
        }

        public override void Entry(IModHelper helper)
        {
            SaveEvents.AfterLoad += SaveEvents_AfterLoad;
            PlayerEvents.InventoryChanged += PlayerEventsOnInventoryChanged;
            GameEvents.UpdateTick += GameEventsOnUpdateTick;
            
            MenuEvents.MenuChanged += MenuEventsOnMenuChanged;
            //GameEvents
            Monitor.Log("Ayy mod loaded");
        }

        private void MenuEventsOnMenuChanged(object sender, EventArgsClickableMenuChanged eventArgsClickableMenuChanged)
        {
            if (Game1.activeClickableMenu is GameMenu menu)
            {
                var pages = this.Helper.Reflection.GetField<List<IClickableMenu>>(menu, "pages").GetValue();
                var page = pages[menu.currentTab];

                // Example for getting the MapPage
                inventoryPage = pages[menu.currentTab] as InventoryPage;
                heldItemReflectedField = null;

                if (inventoryPage != null)
                {
                    heldItemReflectedField = Helper.Reflection.GetField<Item>(inventoryPage, "heldItem");
                    Monitor.Log("Inventory page acquired!");
                }
            }
        }

        private void SaveEvents_AfterLoad(object sender, EventArgs e)
        {
            // Create the inventory table on startup
            RebuildInventoryTable();
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

        // A dictionary that holds item positions so I can find what location they were removed from
        private Dictionary<Item, int> inventoryTable = new Dictionary<Item, int>();

        private Queue<RemovedItem> removedItemQueue = new Queue<RemovedItem>();

        private void RebuildInventoryTable()
        {
            inventoryTable.Clear();
            
            // for each item
            foreach (var item in Game1.player.Items)
            {
                if (item == null)
                    continue;

                var itemIndex = Game1.player.getIndexOfInventoryItem(item);
                inventoryTable[item] = itemIndex;
            }
        }

        private int? GetRemovedItemSlot(Item item)
        {
            int index;
            if (inventoryTable.TryGetValue(item, out index))
                return index;

            return null;
        }
        

        private void PlayerEventsOnInventoryChanged(object sender, EventArgsInventoryChanged eventArgsInventoryChanged)
        {
            Monitor.Log("Inventory Event!");
            Monitor.Log($"Added: {eventArgsInventoryChanged.Added.Count}");
            Monitor.Log($"Removed: {eventArgsInventoryChanged.Removed.Count}");

            // Check if something was removed
            if (eventArgsInventoryChanged.Removed.Count > 0)
            {
                var evnt = eventArgsInventoryChanged.Removed.First();
                var removedItem = new RemovedItem()
                {
                    Item = evnt.Item,
                    Slot = GetRemovedItemSlot(evnt.Item)
                };
                
                removedItemQueue.Enqueue(removedItem);
            }
            else RebuildInventoryTable();
        }
        
        private void HandleRemovedItem(RemovedItem removedItem)
        {
            // if the player is holding the item in the UI don't replace it
            if (HeldItem == removedItem.Item)
                return;

            // should always be true but you never know
            if (!removedItem.Slot.HasValue)
                return;

            Monitor.Log($"Removed {removedItem.Item.Stack}x {removedItem.Item.DisplayName} from inventory {removedItem.Slot}");

            
            // Make sure the item was in the hotbar and the slot is currently empty
            // Prevents item replace on item swap
            if (removedItem.Slot < 12 && Game1.player.Items[removedItem.Slot.Value] == null)
            {
                // Find if theres anything similar to replace it with
                var similarItems = ItemSimilarity.GetSimilarItems(removedItem.Item, Game1.player.Items);
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
                        removedItem.Slot.Value);
                }
            }

            // Update the inventory table
            RebuildInventoryTable();
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
            if(removedItemQueue.Count > 0)
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
