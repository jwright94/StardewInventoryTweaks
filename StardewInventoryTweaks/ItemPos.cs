using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;

namespace StardewInventoryTweaks
{
    public class ItemPos
    {
        public Item Item;
        public int? Slot;

        public ItemPos(Item item)
        {
            Item = item;
        }

        public ItemPos(Item item, int slot)
        {
            Item = item;
            Slot = slot;
        }
    }
}
