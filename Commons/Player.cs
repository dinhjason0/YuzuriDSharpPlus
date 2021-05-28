﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Yuzuri.Commons
{
    public class Player
    {
        public string Name { get; set; }
        public int HP { get; set; }
        public int STR { get; set; }
        public int DEX { get; set; }
        public int SPD { get; set; }
        public int MPE { get; set; }
        public int HIT { get; set; }

        public Item[] Inventory { get; set; }
        public List<int> Favourites { get; set; }

        public Player(string name)
        {
            Name = name;
            HP = 10;
            STR = 1;
            DEX = 1;
            SPD = 1;
            MPE = 1;
            HIT = 1;

            Inventory = new Item[50];
            Favourites = new List<int>();
        }

        public Item GetItem(int invIndex)
        {
            return Inventory[invIndex];
        }

        public void SetItem(int invIndex, Item item)
        {
            Inventory[invIndex] = item;
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
