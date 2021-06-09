using System;
using System.Collections.Generic;
using System.Text;

namespace Yuzuri.Commons
{
    public class Item
    {
        public string Name { get; set; }
        public int STR { get; set; }
        public int MPE { get; set; }
        public int DEX { get; set; }
        public int DR { get; set; }

        public string Desc { get; set; }

        /// <summary>
        /// Special effect of item
        /// </summary>
        public List<ItemEffect> ItemEffects { get; set; }
        public ItemCategory ItemCategory { get; set; }
        public Rarity Rarity { get; set; }

        public Item()
        {
            Name = "";
            STR = 0;
            MPE = 0;
            DEX = 0;
            DR = 0;

            Desc = "";

            ItemEffects = new List<ItemEffect>
            {
                ItemEffect.None
            };

            ItemCategory = ItemCategory.None;

            Rarity = Rarity.Common;
        }

        public Item(string name)
        {
            Name = name;
            STR = 0;
            MPE = 0;
            DEX = 0;
            DR = 0;
            
            Desc = "";

            ItemEffects = new List<ItemEffect>();

            ItemCategory = ItemCategory.None;

            Rarity = Rarity.Common;
        }

    }
}
