using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yuzuri.Commons;
using Yuzuri.Helpers;
using Yuzuri.Managers;

namespace Yuzuri.Commands
{
    public class ItemsCommand : ApplicationCommandModule
    {

        public ItemManager ItemManager { get; private set; }

        public ItemsCommand(IServiceProvider provider)
        {
            ItemManager = provider.GetRequiredService<ItemManager>();
        }

        [SlashCommand("items", "View all available items")]
        public async Task Items(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent("Loading Item data...")).ConfigureAwait(false);
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
                        $"{string.Join("\n", ItemManager.Items.GetRange(i, (i + 13 > ItemManager.Items.Count ? ItemManager.Items.Count-i : 13)).Select(i => $"{EmojiHelper.GetItemEmoji(i.ItemCategory)} {i.Name}"))}");

                    pages[x] = new Page("", embed);

                }

                await ctx.DeleteResponseAsync().ConfigureAwait(false);

                await interactivity.SendPaginatedMessageAsync(ctx.Channel, ctx.User, pages,
                    behaviour: PaginationBehaviour.WrapAround, deletion: ButtonPaginationBehavior.DeleteButtons,
                    timeoutoverride: TimeSpan.FromMinutes(2)).ConfigureAwait(false);

                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}