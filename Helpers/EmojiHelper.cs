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
                ItemCategory.MainHand => DiscordEmoji.FromName(client, ":dagger:"),
                ItemCategory.OffHand => DiscordEmoji.FromName(client, ":shield:"),
                ItemCategory.Consumable => DiscordEmoji.FromName(client, ":wine_glass:"),
                ItemCategory.Ring => DiscordEmoji.FromName(client, ":ring:"),
                _ => DiscordEmoji.FromName(client, ":question:")
            };
        }

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

        public static DiscordEmoji GetMiscEmoji(string emoji)
        {
            DiscordClient client = Bot.Client;
            return emoji.ToUpper() switch
            {
                "CLOSE" => DiscordEmoji.FromName(client, ":x:"),
                _ => DiscordEmoji.FromName(client, ":question:")
            };
        }
    }
}