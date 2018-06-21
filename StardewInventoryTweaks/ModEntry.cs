using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace StardewInventoryTweaks
{
    public class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            SaveEvents.AfterLoad += SaveEvents_AfterLoad;
            PlayerEvents.InventoryChanged += PlayerEventsOnInventoryChanged;


            //GameEvents
            Monitor.Log("Ayy mod loaded");
        }

        private void SaveEvents_AfterLoad(object sender, EventArgs e)
        {
            // Create the inventory table on startup
            RebuildInventoryTable();
        }

        private void DumpInventory()
        {
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
            // Check if something was removed
            if (eventArgsInventoryChanged.Removed.Count > 0)
            {
                var evnt = eventArgsInventoryChanged.Removed.First();
                var removedItem = evnt.Item;
                
                HandleRemovedItem(removedItem);
            }

            //var msg = new HUDMessage("Inventory Changed", HUDMessage.newQuest_type);
            //Game1.addHUDMessage(msg);
        }
        
        private void HandleRemovedItem(Item removedItem)
        {
            // Find what slot it was removed from
            var removedItemInventorySlot = GetRemovedItemSlot(removedItem);

            // should always be true but you never know
            if (!removedItemInventorySlot.HasValue)
                return;

            Game1.showGlobalMessage($"Removed {removedItem.Stack}x {removedItem.DisplayName} from inventory @{removedItemInventorySlot}");

            
            // if it is from the hotbar..
            if (removedItemInventorySlot < 12)
            {
                // Find if theres anything similar to replace it with
                var similarItems = ItemSimilarity.GetSimilarItems(removedItem, Game1.player.Items);
                var mostSimilarItem = similarItems.FirstOrDefault();

                foreach (var item in similarItems)
                {
                    Monitor.Log($"{item.Similarity}: {item.Item.Stack}x {item.Item.DisplayName} {item.Item.Name}");
                }

                if (mostSimilarItem != null)
                {
                    // Replace the item
                    MoveInventoryItem(Game1.player.getIndexOfInventoryItem(mostSimilarItem.Item),
                        removedItemInventorySlot.Value);
                }
            }

            // Update the inventory table
            RebuildInventoryTable();
        }

        private void MoveInventoryItem(int from, int to)
        {
            var temp = Game1.player.Items[to];
            Game1.player.Items[to] = Game1.player.Items[from];
            Game1.player.Items[from] = temp;
        }
    }
}
