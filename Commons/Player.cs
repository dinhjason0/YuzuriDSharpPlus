using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Yuzuri.Commons
{
    public class Player
    {
        public ulong UserId { get; set; }
        public string Name { get; set; }
        public int HP { get; set; }
        public int STR { get; set; }
        public int DEX { get; set; }
        public int SPD { get; set; }
        public int MPE { get; set; }
        public int HIT { get; set; }

        public Item[] Inventory { get; set; }
        public List<int> Favourites { get; set; }

        public int AS { get; set; }
        public int WW { get; set; }
        public int DMG { get; set; }
        public int DR { get; set; }
        public int DGD { get; set; }

        public ulong RoomId { get; set; }

        public Player(ulong userId, string name)
        {
            UserId = userId;
            Name = name;
            HP = 10;
            STR = 1;
            DEX = 1;
            SPD = 1;
            MPE = 1;
            HIT = 1;

            Inventory = new Item[50];
            Favourites = new List<int>();

            AS = 1;
            WW = 0;
            DMG = 0;
            DR = 0;
            DGD = 1;
        }

        public Item GetItem(int invIndex)
        {
            return Inventory[invIndex];
        }

        public void SetItem(int invIndex, Item item)
        {
            Inventory[invIndex] = item;
        }

        public void GiveItem(Item item)
        {
            Inventory[Inventory.Length] = item;
        }

        public void RemoveItem(Item item)
        {
            for(int i = 0; i < Inventory.Length; i++)
            {
                if (Inventory[i] == item)
                {
                    Inventory[i] = null;
                    return;
                }
            }
        }

        public void SetFavouriteInv(int favIndex)
        {
            if (!Favourites.Contains(favIndex))
                Favourites.Add(favIndex);
        }

        public void RemoveFavouriteInv(int favIndex)
        {
            Favourites.Remove(favIndex);
        }
    }
}
