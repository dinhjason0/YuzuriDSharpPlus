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
        public static DiscordEmoji GetItemEmoji(ItemCategory category, DiscordClient client)
        {
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
                _ => DiscordEmoji.FromName(client, ":question:"),
            };
        }
    }
}
