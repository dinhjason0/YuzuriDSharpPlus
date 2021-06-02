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
        public List<int> Equipped { get; set; }

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
            Equipped = new List<int>();

            StatusEffects = StatusEffects.None;

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

    }
}
