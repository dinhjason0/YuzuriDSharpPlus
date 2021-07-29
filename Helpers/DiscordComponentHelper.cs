using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Yuzuri.Commons;

namespace Yuzuri.Helpers
{
    public class DiscordComponentHelper
    {
        public static DiscordButtonComponent MainHand => new DiscordButtonComponent(ButtonStyle.Secondary, "Weapon", "Weapons", false, new DiscordComponentEmoji(EmojiHelper.GetItemEmoji(ItemCategory.Weapon)));
        public static DiscordButtonComponent Helmet => new DiscordButtonComponent(ButtonStyle.Secondary, "Helmet", "Helmet", false, new DiscordComponentEmoji(EmojiHelper.GetItemEmoji(ItemCategory.Helmet)));
        public static DiscordButtonComponent Chestplate => new DiscordButtonComponent(ButtonStyle.Secondary, "Chestplate", "Chestplate", false, new DiscordComponentEmoji(EmojiHelper.GetItemEmoji(ItemCategory.Chestplate)));
        public static DiscordButtonComponent Arms => new DiscordButtonComponent(ButtonStyle.Secondary, "Arms", "Arms", false, new DiscordComponentEmoji(EmojiHelper.GetItemEmoji(ItemCategory.Arms)));
        public static DiscordButtonComponent Leggings => new DiscordButtonComponent(ButtonStyle.Secondary, "Leggings", "Leggings", false, new DiscordComponentEmoji(EmojiHelper.GetItemEmoji(ItemCategory.Leggings)));
        public static DiscordButtonComponent Shoes => new DiscordButtonComponent(ButtonStyle.Secondary, "Shoes", "Shoes", false, new DiscordComponentEmoji(EmojiHelper.GetItemEmoji(ItemCategory.Shoes)));
        public static DiscordButtonComponent Ring => new DiscordButtonComponent(ButtonStyle.Secondary, "Ring", "Ring", false, new DiscordComponentEmoji(EmojiHelper.GetItemEmoji(ItemCategory.Ring)));
        public static DiscordButtonComponent Consumable => new DiscordButtonComponent(ButtonStyle.Secondary, "Consumable", "Consumable", false, new DiscordComponentEmoji(EmojiHelper.GetItemEmoji(ItemCategory.Consumable)));
        public static DiscordButtonComponent None => new DiscordButtonComponent(ButtonStyle.Secondary, "None", "None", false, new DiscordComponentEmoji(EmojiHelper.GetItemEmoji(ItemCategory.None)));
        public static DiscordButtonComponent RedClose => new DiscordButtonComponent(ButtonStyle.Danger, "Close", "Close", false, new DiscordComponentEmoji(EmojiHelper.GetMiscEmoji("Close")));
        public static DiscordButtonComponent GrayClose => new DiscordButtonComponent(ButtonStyle.Secondary, "Close", "Close", false, new DiscordComponentEmoji(EmojiHelper.GetMiscEmoji("Close")));

        public static DiscordButtonComponent BlockStance => new DiscordButtonComponent(ButtonStyle.Secondary, "Block", "Block Stance ", false, new DiscordComponentEmoji(EmojiHelper.GetCombatEmoji("Shield")));
        public static DiscordButtonComponent ParryStance => new DiscordButtonComponent(ButtonStyle.Secondary, "Parry", "Parry Stance", false, new DiscordComponentEmoji(EmojiHelper.GetCombatEmoji("Parry")));
        public static DiscordButtonComponent DodgeStance => new DiscordButtonComponent(ButtonStyle.Secondary, "Dodge", "Dodge Stance", false, new DiscordComponentEmoji(EmojiHelper.GetCombatEmoji("Dodge")));
        public static DiscordButtonComponent AttackStance => new DiscordButtonComponent(ButtonStyle.Secondary, "Attack", "Attack Stance", false, new DiscordComponentEmoji(EmojiHelper.GetItemEmoji(ItemCategory.Weapon)));

        public static readonly DiscordComponent[] OffTurnComponents = new DiscordComponent[] { BlockStance, ParryStance, DodgeStance, AttackStance };

        public static DiscordButtonComponent LightAttack => new DiscordButtonComponent(ButtonStyle.Secondary, "LightAttack", "Light Attack (0.8x)", false, new DiscordComponentEmoji(EmojiHelper.GetItemEmoji(ItemCategory.Weapon)));
        public static DiscordButtonComponent NormalAttack => new DiscordButtonComponent(ButtonStyle.Secondary, "NormalAttack", "Normal Attack (1.0x)", false, new DiscordComponentEmoji(EmojiHelper.GetItemEmoji(ItemCategory.Weapon)));
        public static DiscordButtonComponent HeavyAttack => new DiscordButtonComponent(ButtonStyle.Secondary, "HeavyAttack", "Heavy Attack (1.3x)", false, new DiscordComponentEmoji(EmojiHelper.GetItemEmoji(ItemCategory.Weapon)));

        public static readonly DiscordComponent[] AttackTurnComponents = new DiscordComponent[] { LightAttack, NormalAttack, HeavyAttack };


        /// <summary>
        /// MainHand, Helmet, Chestplate, Arms, Leggings
        /// </summary>
        public static readonly DiscordComponent[] EquipmentButtonComponents_1 = new DiscordComponent[]{ MainHand, Helmet, Chestplate, Arms, Leggings };
        /// <summary>
        /// Shoes, Ring, Consumable, None, RedClose
        /// </summary>
        public static readonly DiscordComponent[] EquipmentButtonComponents_2 = new DiscordComponent[] { Shoes, Ring, Consumable, None, RedClose };
        /// <summary>
        /// Shoes, Ring, RedClose
        /// </summary>
        public static readonly DiscordComponent[] EquipmentButtonComponents_3 = new DiscordComponent[] { Shoes, Ring, RedClose };

        /// <summary>
        /// Return Select options with items from list
        /// </summary>
        /// <param name="items">List of items to make select with</param>
        /// <returns>Select with item options</returns>
        public static DiscordSelectComponentOption[] EquipmentMenuSelectOption(List<Item> items)
        {
            //_ = items.Distinct().ToList();

            DiscordSelectComponentOption[] options = new DiscordSelectComponentOption[items.Count];

            for (int i = 0; i < Math.Min(items.Count, 24); i++)
            {
                options[i] = new DiscordSelectComponentOption(items[i].Name, $"{i}_{items[i].Name}", emoji: new DiscordComponentEmoji(EmojiHelper.GetItemEmoji(items[i].ItemCategory)));
            }

            return options;
        }
    }
}
