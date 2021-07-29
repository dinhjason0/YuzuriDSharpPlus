using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Yuzuri.Commons;

namespace Yuzuri.Helpers
{
    public class EmojiHelper
    {
        /// <summary>
        /// Get DiscordEmoji for ItemCategories
        /// </summary>
        /// <param name="category">ItemCategory of emoji</param>
        /// <returns>DiscordEmoji of the ItemCategory. Returns ? if invalid</returns>
        public static DiscordEmoji GetItemEmoji(ItemCategory category)
        {
            DiscordClient client = Bot.Client;
            return category switch
            {
                ItemCategory.Helmet => DiscordEmoji.FromName(client, ":billed_cap:"),
                ItemCategory.Chestplate => DiscordEmoji.FromName(client, ":shirt:"),
                ItemCategory.Arms => DiscordEmoji.FromName(client, ":gloves:"),
                ItemCategory.Leggings => DiscordEmoji.FromName(client, ":jeans:"),
                ItemCategory.Shoes => DiscordEmoji.FromName(client, ":athletic_shoe:"),
                ItemCategory.Weapon => DiscordEmoji.FromName(client, ":dagger:"),
                ItemCategory.OffHand => DiscordEmoji.FromName(client, ":shield:"),
                ItemCategory.Consumable => DiscordEmoji.FromName(client, ":wine_glass:"),
                ItemCategory.Ring => DiscordEmoji.FromName(client, ":ring:"),
                _ => DiscordEmoji.FromName(client, ":question:")
            };
        }

        /// <summary>
        /// Get DiscordEmohi for Stats
        /// </summary>
        /// <param name="stat">Stat of emoji</param>
        /// <returns>DiscordEmoji of Stat. Returns ? if invalid</returns>
        public static DiscordEmoji GetStatEmoji(string stat)
        {
            DiscordClient client = Bot.Client;
            return stat.ToUpper() switch
            {
                "HP" => DiscordEmoji.FromName(client, ":sparkling_heart:"),
                "STR" => DiscordEmoji.FromName(client, ":crossed_swords:"),
                "DEX" => DiscordEmoji.FromName(client, ":bow_and_arrow:"),
                "SPD" => DiscordEmoji.FromName(client, ":dash:"),
                "MPE" => DiscordEmoji.FromName(client, ":crystal_ball:"),
                "DHL" => DiscordEmoji.FromName(client, ":game_die:"),
                "HIT" => DiscordEmoji.FromName(client, ":dart:"),
                "RING" => DiscordEmoji.FromName(client, ":ring:"),
                _ => DiscordEmoji.FromName(client, ":question:")

            };
        }

        /// <summary>
        /// Get DiscordEmoji of Item Stats
        /// </summary>
        /// <param name="stat">Stat of Item emoji</param>
        /// <returns>DiscordEmoji of item stat. Returns ? if invalid</returns>
        public static DiscordEmoji GetItemEmoji(string stat)
        {
            DiscordClient client = Bot.Client;
            return stat.ToUpper() switch
            {
                "STR" => GetStatEmoji(stat),
                "MPE" => GetStatEmoji(stat),
                "DEX" => GetStatEmoji(stat),
                "DR" => DiscordEmoji.FromName(client, ":small_red_triangle_down:"),
                "RARITY" => DiscordEmoji.FromName(client, ":sparkles:"),
                "DESC" => DiscordEmoji.FromName(client, ":book:"),
                "ITEMCATEGORY" => DiscordEmoji.FromName(client, ":card_box:"),
                "ITEMEFFECT" => DiscordEmoji.FromName(client, ":sparkler:"),
                _ => DiscordEmoji.FromName(client, ":question:")
            };
        }

        public static DiscordEmoji GetCombatEmoji(string emoji)
        {
            DiscordClient client = Bot.Client;

            return emoji.ToUpper() switch
            {
                "SHIELD" => DiscordEmoji.FromName(client, ":shield:"),
                "DODGE" => DiscordEmoji.FromName(client, ":cyclone:"),
                "" => DiscordEmoji.FromName(client, ":person_fencing:"),
                _ => DiscordEmoji.FromName(client, ":question:")
            };
        }

        /// <summary>
        /// DiscordEmoji that doesn't belong to a main category
        /// </summary>
        /// <param name="emoji"></param>
        /// <returns></returns>
        public static DiscordEmoji GetMiscEmoji(string emoji)
        {
            DiscordClient client = Bot.Client;
            return emoji.ToUpper() switch
            {
                "CLOSE" => DiscordEmoji.FromName(client, ":x:"),
                "TICK" => DiscordEmoji.FromName(client, ":white_check_mark:"),
                _ => DiscordEmoji.FromName(client, ":question:")
            };
        }
    }
}