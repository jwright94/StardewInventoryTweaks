using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace StardewInventoryTweaks
{
    class InventoryUIHelper
    {
        public Item HeldItem
        {
            get { return heldItemReflectedField?.GetValue(); }
        }

        private IReflectedField<Item> heldItemReflectedField;
        private GameMenu gameMenu;

        private ModEntry modEntry;

        private int previousTab = -1;

        public InventoryUIHelper(ModEntry modEntry)
        {
            this.modEntry = modEntry;
        }

        public void OnMenuChange()
        {
            if (Game1.activeClickableMenu is GameMenu menu)
            {
                gameMenu = menu;
                UpdateReflectionReferences();
            }
        }

        public void OnUpdate()
        {
            if (gameMenu != null && gameMenu.currentTab != previousTab)
            {
                previousTab = gameMenu.currentTab;
                UpdateReflectionReferences();
            }
        }

        private void UpdateReflectionReferences()
        {
            var pages = modEntry.Helper.Reflection.GetField<List<IClickableMenu>>(gameMenu, "pages").GetValue();
            var currentPage = pages[gameMenu.currentTab];

            heldItemReflectedField = null;

            // Update heldItem
            if (currentPage is InventoryPage inventoryPage)
            {
                heldItemReflectedField = modEntry.Helper.Reflection.GetField<Item>(inventoryPage, "heldItem");
                modEntry.Monitor.Log("Inventory page acquired!");
            }
            else if (currentPage is CraftingPage craftingPage)
            {
                heldItemReflectedField = modEntry.Helper.Reflection.GetField<Item>(craftingPage, "heldItem");
                modEntry.Monitor.Log("Crafting page acquired!");
            }
        }
    }
}
