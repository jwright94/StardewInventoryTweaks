using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;

namespace StardewInventoryTweaks
{
    /// <summary>
    /// Watches an inventory for changes and triggers events,
    /// For my own sanity it makes the assumption that an item was removed when it's slot becomes null
    /// rather than if it was removed and replaced in the same update.
    /// All updates are sent at once
    /// </summary>
    public class InventoryWatcher
    {
        public IList<Item> Inventory { get; }

        public IList<ItemPos> Added => addedItems;
        public IList<ItemPos> Removed => removedItems;
        public IList<ItemPos> Moved => movedItems;

        // Events
        public Action OnInventoryResize;
        public Action OnInventoryChange;

        // internal stuff
        private List<Item> previousItems;

        // for keeping track of where items were 
        private Dictionary<Item, int> inventoryPositionTable = new Dictionary<Item, int>();

        private List<ItemPos> addedItems;
        private List<ItemPos> removedItems;
        private List<ItemPos> movedItems;

        public InventoryWatcher(IList<Item> inventory)
        {
            Inventory = inventory;
            previousItems = new List<Item>();
            
            previousItems.AddRange(inventory);

            ResizePreviousItemsList();

            RebuildInventoryPositionTable();

            addedItems = new List<ItemPos>();
            removedItems = new List<ItemPos>();
            movedItems = new List<ItemPos>();
        }

        public void Update()
        {
            // Check for inventory resizing
            if (previousItems.Count != Inventory.Count)
            {
                ResizePreviousItemsList();
                OnInventoryResize?.Invoke();
            }

            bool shouldUpdate = false;

            removedItems.Clear();
            addedItems.Clear();
            movedItems.Clear();

            // Add removed items
            var itemExceptions = previousItems.Except(Inventory);

            foreach (var removedItem in itemExceptions)
            {
                if (removedItem != null && inventoryPositionTable.TryGetValue(removedItem, out var itemPos))
                {
                    removedItems.Add(new ItemPos(removedItem, itemPos));
                }
            }

            if (removedItems.Count > 0)
                shouldUpdate = true;

            // Check for items that weren't there before
            for (int i = 0; i < Inventory.Count; i++)
            {
                var item = Inventory[i];
                var prevItem = previousItems[i];

                previousItems[i] = Inventory[i];

                if (item != prevItem)
                {
                    // Something has changed
                    if (item != null)
                    {
                        shouldUpdate = true;
                        if (!inventoryPositionTable.ContainsKey(item))
                        {
                            // if it was not in the array before it is new
                            addedItems.Add(new ItemPos(item, i));
                        }
                        else
                        {
                            // if it was it has moved
                            movedItems.Add(new ItemPos(item, i));
                        }
                    }
                }
            }

            if(shouldUpdate)
                OnInventoryChange?.Invoke();

            RebuildInventoryPositionTable();
        }

        private void ResizePreviousItemsList()
        {
            // Grow the list
            while (previousItems.Count < Inventory.Count)
                previousItems.Add(null);

            // Shrink the list
            if(previousItems.Count > Inventory.Count)
                previousItems.RemoveRange(Inventory.Count, Inventory.Count - previousItems.Count);
        }

        private void RebuildInventoryPositionTable()
        {
            inventoryPositionTable.Clear();

            // for each item
            for (int i = 0; i < Inventory.Count; i++)
            {
                var item = Inventory[i];

                if (item != null)
                    inventoryPositionTable[item] = i;
            }
        }
    }
}
