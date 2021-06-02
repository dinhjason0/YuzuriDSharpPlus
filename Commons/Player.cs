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
        /// <summary>
        /// Health Stat
        /// </summary>
        public int HP { get; set; }
        /// <summary>
        /// Strength Stat
        /// </summary>
        public int STR { get; set; }
        /// <summary>
        /// Dexterity Stat
        /// </summary>
        public int DEX { get; set; }
        /// <summary>
        /// Speed Stat
        /// </summary>
        public int SPD { get; set; }
        /// <summary>
        /// Magic Power Stat
        /// </summary>
        public int MPE { get; set; }
        /// <summary>
        /// Hit chance Stat
        /// </summary>
        public int HIT { get; set; }
        /// <summary>
        /// Damage roll stat
        /// </summary>
        public int DHL { get; set; }

        public List<Item> Inventory { get; set; }
        public Dictionary<EquippedSlots, int> Equipped { get; set; }

        public StatusEffects StatusEffects { get; set; }

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
            DHL = 1;

            Inventory = new List<Item>();
            Inventory.Add(Bot.ItemManager.GetItem("Leather Helmet"));
            Inventory.Add(Bot.ItemManager.GetItem("Leather Tunic"));
            Inventory.Add(Bot.ItemManager.GetItem("Leather Wrist Guards"));
            Inventory.Add(Bot.ItemManager.GetItem("Leather Pants"));
            Inventory.Add(Bot.ItemManager.GetItem("Leather Boots"));
            Inventory.Add(Bot.ItemManager.GetItem("Potion"));

            Equipped = new Dictionary<EquippedSlots, int>()
            {
                { EquippedSlots.Helmet, 0 },
                { EquippedSlots.Chest, 1 },
                { EquippedSlots.Arms, 2 },
                { EquippedSlots.Legs, 3 },
                { EquippedSlots.Shoes, 4 },
                { EquippedSlots.MainHand, 0 },
                { EquippedSlots.OffHand, 0 }
            };

            StatusEffects = StatusEffects.None;

            AS = 1;
            WW = 0;
            DMG = 0;
            DR = 0;
            DGD = 1;

            foreach (Item item in Inventory)
            {
                Console.WriteLine(item.Name);
            }
        }

        public Item GetItem(int invIndex)
        {
            return Inventory[invIndex];
        }

        public void SetItem(int invIndex, Item item)
        {
            Inventory[invIndex] = item;
        }

        public bool GiveItem(Item item)
        {
            if (Inventory.Count >= 50) return false;
            Inventory.Add(item);

            SaveData();

            return true;
        }

        public void RemoveItem(Item item)
        {
            Inventory.Remove(item);

            SaveData();
        }

        public void SaveData()
        {
            Bot.PlayerManager.WritePlayerData(this);
        }

        public enum EquippedSlots
        {
            Helmet = 0,
            Chest = 1,
            Arms = 2,
            Legs = 3,
            Shoes = 4,
            MainHand = 5,
            OffHand = 6
        }
    }
}
