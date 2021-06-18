using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Yuzuri.Managers;

namespace Yuzuri.Commons
{
    public class Player
    {
        public ulong UserId { get; set; }
        public string Name { get; set; }

        /// <summary>
        /// Max HP stat
        /// </summary>
        public int MaxHP { get; set; }
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
        public Dictionary<EquippedSlots, Item> Equipped { get; set; }
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
            MaxHP = 10;
            HP = MaxHP;
            STR = 1;
            DEX = 1;
            SPD = 1;
            MPE = 1;
            HIT = 1;
            DHL = 1;

            Inventory = new List<Item>();

            Equipped = new Dictionary<EquippedSlots, Item>();

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

        public int GetItemIndex(Item item)
        {
            return Inventory.FindIndex(i => string.Equals(i.Name, item.Name, StringComparison.OrdinalIgnoreCase));
        }

        public void EquipItem(EquippedSlots slot, Item newItem)
        {
            //Item oldItem = Equipped[slot];

            //HP -= oldItem.HP;
            //STR -= oldItem.STR;
            //DEX -= oldItem.DEX;
            //SPD -= oldItem.SPD;
            //MPE -= oldItem.MPE;
            //HIT -= oldItem.HIT;
            //DHL -= oldItem.DHL;

            Equipped[slot] = newItem;
            //HP += newItem.HP;
            //STR += newItem.STR;
            //DEX += newItem.DEX;
            //SPD += newItem.SPD;
            //MPE += newItem.MPE;
            //HIT += newItem.HIT;
            //DHL += newItem.DHL;
        }

        public void CalcStats()
        {
            //HP = 10 + Equipped[EquippedSlots.Helmet].
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
            PlayerManager.WritePlayerData(this);
        }

        public enum EquippedSlots
        {
            Helmet = 0,
            Chest = 1,
            Arms = 2,
            Legs = 3,
            Shoes = 4,
            MainHand = 5,
            OffHand = 6,
            Ring = 7
        }
    }
}
