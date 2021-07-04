using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yuzuri.Helpers;
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

        /// <summary>
        /// Get item based on inventory index
        /// </summary>
        /// <param name="invIndex">Index to get item</param>
        /// <returns></returns>
        public Item GetItem(int invIndex)
        {
            return Inventory[invIndex];
        }

        /// <summary>
        /// Get item based on item name
        /// </summary>
        /// <param name="itemName">Item name to be found</param>
        /// <returns></returns>
        public Item GetItem(string itemName)
        {
            return Inventory.Find(i => string.Equals(i.Name, itemName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Get index of item
        /// </summary>
        /// <param name="item">Item to get index of</param>
        /// <returns></returns>
        public int GetItemIndex(Item item)
        {
            return Inventory.FindIndex(i => string.Equals(i.Name, item.Name, StringComparison.OrdinalIgnoreCase));
        }

        public void EquipItem(EquippedSlots slot, Item newItem)
        {
            RemoveItem(newItem);
            GiveItem(Equipped[slot]);
            Equipped[slot] = newItem;

            SaveData();
        }

        public void EquipItem(ItemCategory category, Item newItem)
        {
            switch (category)
            {
                case ItemCategory.Helmet:
                    EquipItem(EquippedSlots.Helmet, newItem);
                    break;
                case ItemCategory.Chestplate:
                    EquipItem(EquippedSlots.Chest, newItem);
                    break;
                case ItemCategory.Arms:
                    EquipItem(EquippedSlots.Arms, newItem);
                    break;
                case ItemCategory.Leggings:
                    EquipItem(EquippedSlots.Legs, newItem);
                    break;
                case ItemCategory.Shoes:
                    EquipItem(EquippedSlots.Shoes, newItem);
                    break;
                case ItemCategory.Ring:
                    EquipItem(EquippedSlots.Ring, newItem);
                    break;
                case ItemCategory.Weapon:
                    EquipItem(EquippedSlots.MainHand, newItem);
                    break;
            }
        }

        /// <summary>
        /// Recalculate player stats
        /// </summary>
        public void CalcStats()
        {
            //HP = 10 + Equipped[EquippedSlots.Helmet].
        }

        /// <summary>
        /// Override an inventory slot with an item
        /// </summary>
        /// <param name="invIndex">Inventory index id</param>
        /// <param name="item">Item to override with</param>
        public void SetItem(int invIndex, Item item)
        {
            Inventory[invIndex] = item;
        }

        /// <summary>
        /// Give player a specific item
        /// </summary>
        /// <param name="item">Item to be given to player</param>
        /// <returns>If item was given successfully</returns>
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

        /// <summary>
        /// Save player data
        /// </summary>
        public void SaveData()
        {
            PlayerManager.WritePlayerData(this);
        }

        /// <summary>
        /// Get items based on Item Category
        /// </summary>
        /// <param name="itemCategory">Item Category of items</param>
        /// <returns></returns>
        public List<Item> GetItems(ItemCategory itemCategory)
        {
            return Inventory.FindAll(i => i.ItemCategory == itemCategory);
        }

        /// <summary>
        /// Adds fields with players equipped items
        /// </summary>
        /// <param name="embed">Embed for fields to be added</param>
        /// <param name="inline">Should embed be inline?</param>
        public void AddEquippedEmbed(DiscordEmbedBuilder embed, bool inline = true)
        {
            embed.AddField("**Equipped**",
                        $"{EmojiHelper.GetItemEmoji(ItemCategory.Weapon)} Main Hand: {Equipped[EquippedSlots.MainHand].Name}\n" +
                        $"{EmojiHelper.GetItemEmoji(ItemCategory.Helmet)} Helmet: {Equipped[EquippedSlots.Helmet].Name}\n" +
                        $"{EmojiHelper.GetItemEmoji(ItemCategory.Chestplate)} Chest: {Equipped[EquippedSlots.Chest].Name}\n" +
                        $"{EmojiHelper.GetItemEmoji(ItemCategory.Arms)} Gloves: {Equipped[EquippedSlots.Arms].Name}\n" +
                        $"{EmojiHelper.GetItemEmoji(ItemCategory.Leggings)} Legs: {Equipped[EquippedSlots.Legs].Name}\n" +
                        $"{EmojiHelper.GetItemEmoji(ItemCategory.Shoes)} Feet: {Equipped[EquippedSlots.Shoes].Name}\n" +
                        $"{EmojiHelper.GetItemEmoji(ItemCategory.Ring)} Ring: {Equipped[EquippedSlots.Ring].Name}\n",
                        inline);
        }

        /// <summary>
        /// Adds fields with players inventory content
        /// </summary>
        /// <param name="embed">Embed for fields to be added</param>
        /// <param name="inline">Should embed be inline?</param>
        public void AddItemEmbed(DiscordEmbedBuilder embed, bool inline = true)
        {
            for (int i = 0, x = 1; i < Inventory.Count; i += 10, x++)
            {
                embed.AddField($"**Inventory - {x}**",
                    $"{string.Join("\n", Inventory.GetRange(i, (i + 10 > Inventory.Count ? Inventory.Count - i : 10)).Select(i => $"{EmojiHelper.GetItemEmoji(i.ItemCategory)} {i.Name}"))}", inline);
            }
        }

        /// <summary>
        /// Adds fields with players equippable inventory content
        /// </summary>
        /// <param name="embed">Embed for fields to be added</param>
        /// <param name="inline">Should embed be inline?</param>
        public void AddEquippableItemEmbed(DiscordEmbedBuilder embed, bool inline = true)
        {
            ItemCategory[] itemCategories = new ItemCategory[6] { ItemCategory.Weapon, ItemCategory.Helmet, ItemCategory.Chestplate, ItemCategory.Arms, ItemCategory.Leggings, ItemCategory.Shoes};
            List<Item> items = Inventory.FindAll(i => itemCategories.Contains(i.ItemCategory));

            for (int i = 0, x = 1; i < items.Count; i += 10, x++)
            {
                embed.AddField($"**Inventory - {x}**",
                    $"{string.Join("\n", items.GetRange(i, (i + 10 > items.Count ? items.Count - i : 10)).Select(i => $"{EmojiHelper.GetItemEmoji(i.ItemCategory)} {i.Name}"))}", inline);
            }
        }

        /// <summary>
        /// Adds fields with players inventory content based on item category
        /// </summary>
        /// <param name="embed">Embed for fields to be added</param>
        /// <param name="category">Item Category for fields to be added</param>
        /// <param name="inline">Should embed be inline?</param>
        public void AddItemEmbed(DiscordEmbedBuilder embed, ItemCategory category, bool inline = true)
        {
            List<Item> items = GetItems(category);

            for (int i = 0, x = 1; i < items.Count; i += 10, x++)
            {
                embed.AddField($"**{category} - {x}**",
                    $"{string.Join("\n", items.GetRange(i, (i + 10 > items.Count ? items.Count - i : 10)).Select(i => $"{EmojiHelper.GetItemEmoji(i.ItemCategory)} {i.Name}"))}", inline);
            }
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
