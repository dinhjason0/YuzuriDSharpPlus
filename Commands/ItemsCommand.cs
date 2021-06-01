using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yuzuri.Commons;
using Yuzuri.Managers;

namespace Yuzuri.Commands
{
    public class ItemsCommand : BaseCommandModule
    {

        [Command("items"), Description("View all available items")]
        public async Task Items(CommandContext ctx)
        {
            try
            {
                var interactivity = ctx.Client.GetInteractivity();

                int pageCount = (int)Math.Ceiling(ItemManager.Items.Count / 13.0);

                Page[] pages = new Page[pageCount];

                for (int i = 0, x = 0; i < ItemManager.Items.Count; i += 13, x++)
                {
                    
                    var embed = new DiscordEmbedBuilder
                    {
                        Title = $"Item Dictionary",
                        Description = $"Total Available Items: {ItemManager.Items.Count}",
                        Color = DiscordColor.Gold,
                        Footer = new DiscordEmbedBuilder.EmbedFooter()
                        { 
                            Text = $"Page {x+1}/{pageCount}"
                        }
                    };

                    embed.AddField("**Items**",
                        $"{string.Join("\n", ItemManager.Items.GetRange(i, (i + 13 > ItemManager.Items.Count ? ItemManager.Items.Count - i : 13)).Select(i => $"{GetItemEmoji(i.ItemCategory, ctx.Client)} {i.Name}"))}");

                    pages[x] = new Page("", embed);
                    Console.WriteLine("cut");
                }

                await interactivity.SendPaginatedMessageAsync(ctx.Channel, ctx.User, pages,
                    behaviour: PaginationBehaviour.WrapAround, deletion: PaginationDeletion.DeleteEmojis,
                    timeoutoverride: TimeSpan.FromMinutes(2)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private DiscordEmoji GetItemEmoji(ItemCategory category, DiscordClient client)
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