using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;

namespace StardewInventoryTweaks
{
    internal class SimilarItem
    {
        public float Similarity;
        public Item Item;
    }

    internal static class ItemSimilarity
    {
        public static IEnumerable<SimilarItem> GetSimilarItems(Item item, IList<Item> otherItems)
        {
            var obj = item as StardewValley.Object;

            IEnumerable<SimilarItem> results;

            if (obj != null)
            {
                results = otherItems
                    .Where(x => x != null)
                    .Select(otherItem => GetSimilarity(obj, otherItem))
                    .Where(x => x.Similarity > 0);
            }
            else
            {
                results = otherItems
                    .Where(x => x != null)
                    .Select(otherItem => GetSimilarity(item, otherItem))
                    .Where(x => x.Similarity > 0);
            }

            return results.OrderByDescending(x => x.Similarity);
        }

        public static SimilarItem GetSimilarity(Item a, Item otherItem)
        {
            float similarity = a.Name == otherItem.Name ? 100 : 0;

            if (a.canStackWith(otherItem))
                similarity += 100;

            return new SimilarItem()
            {
                Similarity = similarity,
                Item = otherItem
            };
        }

        public static SimilarItem GetSimilarity(StardewValley.Object obj, Item otherItem)
        {
            var otherObj = otherItem as StardewValley.Object;
            if (otherObj == null)
                return GetSimilarity((Item)obj, otherItem);

            float similarity = 0;

            if (obj.canStackWith(otherItem))
                similarity += 100;

            similarity += (obj.Name == otherObj.Name) ? 100 : 0;
            similarity += (obj.Category == otherObj.Category) ? 2 : 0;
            similarity += (obj.Type == otherObj.Type) ? 5 : 0;
            similarity += -Math.Abs(obj.Quality - otherObj.Quality);

            //float qualityScore = a.SpecialVariable;
            return new SimilarItem()
            {
                Similarity = similarity,
                Item = otherItem
            };
        }
    }
}
