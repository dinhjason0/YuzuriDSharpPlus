﻿using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yuzuri.Commons;
using Yuzuri.Helpers;
using Yuzuri.Managers;

namespace Yuzuri.Commands
{
    public class PlayerCommands : SlashCommandModule
    {
        public Random Rng { get; private set; }
        public PlayerManager PlayerManager {get; private set; }

        public PlayerCommands(IServiceProvider provider)
        {
            Rng = provider.GetRequiredService<Random>();
            PlayerManager = provider.GetRequiredService<PlayerManager>();
        }

        [SlashCommand("start", "Start your adventure!")]
        public async Task Start(InteractionContext ctx)
        {
            try
            {
                DiscordMember member = (DiscordMember)ctx.User;
                if (!PlayerManager.PlayerRoleCheck(ctx.Guild, member, out DiscordRole playerRole))
                {
                    var interactivity = ctx.Client.GetInteractivity();

                    var embed = new DiscordEmbedBuilder()
                    {
                        Title = "Weclome New User",
                        Description = "New arrivals, please head towards the registry table."
                    };

                    //var msg = await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
                    await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, 
                        new DiscordInteractionResponseBuilder().AddEmbed(embed.Build())).ConfigureAwait(false);

                    await Task.Delay(1300);

                    embed.Description = "Hello new user, please state your name";
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build())).ConfigureAwait(false);
                    
                    var response = await interactivity
                        .WaitForMessageAsync(x =>
                            x.Channel == ctx.Channel
                            && x.Author == ctx.User
                        ).ConfigureAwait(false);

                    if (response.TimedOut)
                    {
                        embed.Title = "Connection timed out";
                        embed.Description = "User has not responded within allocated time. Disconnecting user...";
                        embed.Color = DiscordColor.DarkRed;
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build())).ConfigureAwait(false);
                    }
                    else
                    {
                        await response.Result.DeleteAsync().ConfigureAwait(false);
                        embed.Title = $"Welcome {response.Result.Content}";
                        embed.Description = $"**User: {response.Result.Content}\n**" +
                            $"Permission granted. Please hold, we are currently loading your quarters.\n";
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build())).ConfigureAwait(false);
                        await Task.Delay(1500);

                        string embedString = $"**User: {response.Result.Content}**\n" +
                            $"Permission granted. Please hold, we are currently loading your quarters.\n" +
                            $"Requesting cloud data... ";

                        embed.Color = DiscordColor.Orange;
                        embed.Description = embedString;
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build())).ConfigureAwait(false);
                        await Task.Delay(600);

                        int count = 0;
                        for (int i = 0; i < 3; i++)
                        {
                            count += Rng.Next(3, 5);
                            if (i == 2) count = 10;
                            embed.Description = $"{embedString}  {new string('⬛', count)}{new string('⬜', 10 - count)} {10 * count}%";
                            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build())).ConfigureAwait(false);
                            await Task.Delay(800);
                        }

                        await Task.Delay(200);
                        embed.Color = DiscordColor.Green;
                        embed.Description = $"Link connection has been established. Say hello to your new room {response.Result.Content}";
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build())).ConfigureAwait(false);

                        await member.GrantRoleAsync(playerRole).ConfigureAwait(false);
                        try
                        {
                            Player player = new Player(ctx.User.Id, response.Result.Content);

                            var room = await PlayerManager.CreatePlayerRoom(ctx.Guild, player).ConfigureAwait(false);
                            player.RoomId = room.Id;

                            PlayerManager.WritePlayerData(player);

                            await room.SendMessageAsync($"{ctx.User.Mention} welcome to your room.\nThis is your personal room where you can check the following: Inventory, Skills, Spells and Equipment.").ConfigureAwait(false);
                        }
                        catch
                        (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    
                    }
                }
                else
                {
                    await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder().WithContent("You're already a player!")).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        [SlashCommand("stats", "View your stats"), RequireRoles(RoleCheckMode.Any, new string[] { "Player" })]
        public async Task Stats(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, 
                new DiscordInteractionResponseBuilder().WithContent("Loading Player data...")).ConfigureAwait(false);

            if (PlayerManager.PlayerRoleCheck(ctx.Guild, (DiscordMember)ctx.User))
            {
                Player player = PlayerManager.ReadPlayerData(ctx.User.Id);

                string status = "";

                switch (player.StatusEffects)
                {
                    case StatusEffects.Knockedout:
                        status = $"```apache\nUnconscious\n```";
                        break;
                    case StatusEffects.Dead:
                        status = $"```arm\nDead\n```";
                        break;
                    case StatusEffects.None:
                        status = $"```yaml\nAlive\n```";
                        break;
                }

                var embed = new DiscordEmbedBuilder
                {
                    Title = $"{player.Name}'s Stats",
                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail()
                    {
                        Url = ctx.User.AvatarUrl
                    },
                    Description = $"  Status: {status}",
                    Color = DiscordColor.Green,
                };

                embed.AddField("\n**Stats**\n",
                    $"{EmojiHelper.GetStatEmoji("HP")} HP: {player.HP}\n" +
                    $"{EmojiHelper.GetStatEmoji("STR")} STR: {player.STR}\n" +
                    $"{EmojiHelper.GetStatEmoji("DEX")} DEX: {player.DEX}\n" +
                    $"{EmojiHelper.GetStatEmoji("SPD")} SPD: {player.SPD}\n" +
                    $"{EmojiHelper.GetStatEmoji("MPE")} MPE: {player.MPE}\n" +
                    $"{EmojiHelper.GetStatEmoji("DHL")} DHL: {player.DHL}\n" +
                    $"{EmojiHelper.GetStatEmoji("HIT")} HIT: {player.HIT}", true);
                try
                {
                    embed.AddField("**Equipped**",
                        $"{EmojiHelper.GetItemEmoji(ItemCategory.Weapon)} Main Hand: {player.Equipped[Player.EquippedSlots.MainHand].Name}\n" +
                        $"{EmojiHelper.GetItemEmoji(ItemCategory.Helmet)} Helmet: {player.Equipped[Player.EquippedSlots.Helmet].Name}\n" +
                        $"{EmojiHelper.GetItemEmoji(ItemCategory.Chestplate)} Chest: {player.Equipped[Player.EquippedSlots.Chest].Name}\n" +
                        $"{EmojiHelper.GetItemEmoji(ItemCategory.Arms)} Gloves: {player.Equipped[Player.EquippedSlots.Arms].Name}\n" +
                        $"{EmojiHelper.GetItemEmoji(ItemCategory.Leggings)} Legs: {player.Equipped[Player.EquippedSlots.Legs].Name}\n" +
                        $"{EmojiHelper.GetItemEmoji(ItemCategory.Shoes)} Feet: {player.Equipped[Player.EquippedSlots.Shoes].Name}\n" +
                        $"{EmojiHelper.GetItemEmoji(ItemCategory.Ring)} Ring: {player.Equipped[Player.EquippedSlots.Ring].Name}\n",
                        true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                embed.AddField("**Skills**", "Punch ", true);

                for (int i = 0, x = 1; i < player.Inventory.Count; i += 10, x++)
                {
                    embed.AddField($"**Inventory - {x}**",
                        $"{string.Join("\n", player.Inventory.GetRange(i, (i + 10 > player.Inventory.Count ? player.Inventory.Count - i : 10)).Select(i => $"{EmojiHelper.GetItemEmoji(i.ItemCategory)} {i.Name}"))}", true);

                }

                embed.WithFooter($"Inventory Slots: {player.Inventory.Count}/100");

                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build())).ConfigureAwait(false);
                //await ctx.Interaction.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
            }
            else
            {
                await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, 
                    new DiscordInteractionResponseBuilder().WithContent("You haven't started your adventure yet! Use the `start` command!")).ConfigureAwait(false);
            }
        }

        [SlashCommand("quit", "End your adventure"), RequireRoles(RoleCheckMode.Any, new string[] { "Player" })]
        public async Task Quit(InteractionContext ctx)
        {
            if (PlayerManager.PlayerRoleCheck(ctx.Guild, (DiscordMember)ctx.User, out DiscordRole discordRole))
            {
                var interactivity = ctx.Client.GetInteractivity();

                var msg = await ctx.Interaction.Channel.SendMessageAsync("Are you sure you want to end your adventure? React to the tick emoji to confirm.");
                await msg.CreateReactionAsync(DiscordEmoji.FromName(Bot.Client, ":white_check_mark:")).ConfigureAwait(false);

                var reaction = await interactivity
                    .WaitForReactionAsync(x =>
                        x.Channel == ctx.Channel
                        && x.User == ctx.User
                        && x.Emoji == DiscordEmoji.FromName(Bot.Client, ":white_check_mark:"))
                    .ConfigureAwait(false);

                if (reaction.TimedOut)
                {
                    await ctx.Channel.SendMessageAsync($"Request to quit timed out. Returning user to their adventure...");
                }
                else
                {
                    await ctx.Channel.SendMessageAsync("Your adventure ends here.").ConfigureAwait(false);
                    await ((DiscordMember)ctx.User).RevokeRoleAsync(discordRole).ConfigureAwait(false);
                    await PlayerManager.RemovePlayerRoom(ctx.Guild, PlayerManager.ReadPlayerData(ctx.User.Id)).ConfigureAwait(false);
                }
            }
            else
            {
                await ctx.Channel.SendMessageAsync("You haven't started your adventure yet! Use the `start` command!").ConfigureAwait(false);
            }
        }

        [SlashCommand("inventory", "View your inventory"), Aliases(new string[] { "inv" }), RequireRoles(RoleCheckMode.Any, new string[] { "Player" })]
        public async Task Inventory(InteractionContext ctx)
        {
            Player player = PlayerManager.ReadPlayerData(ctx.User.Id);

            var embed = new DiscordEmbedBuilder
            {
                Title = $"{player.Name}'s Inventory",
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail()
                {
                    Url = ctx.User.AvatarUrl
                },
                Color = DiscordColor.Green,
            };

            for (int i = 0, x = 1; i < player.Inventory.Count; i += 10, x++)
            {
                embed.AddField($"**Inventory - {x}**",
                    $"{string.Join("\n", player.Inventory.GetRange(i, (i + 10 > player.Inventory.Count ? player.Inventory.Count - i : 10)).Select(i => $"{EmojiHelper.GetItemEmoji(i.ItemCategory)} {i.Name}"))}", true);

            }

            embed.WithFooter($"Inventory Slots: {player.Inventory.Count}/100");

            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent("Loading inventory...")).ConfigureAwait(false);
            try
            {
                var builder = new DiscordWebhookBuilder()
                    .AddEmbed(embed.Build())
                    .WithComponents(new DiscordComponent[]
                    {
                    ButtonHelper.MainHand, ButtonHelper.Helmet, ButtonHelper.Chestplate,
                    ButtonHelper.Arms, ButtonHelper.Leggings
                    })
                    .WithComponents(new DiscordComponent[]
                    {
                    ButtonHelper.Shoes, ButtonHelper.Ring, ButtonHelper.Consumable,
                    ButtonHelper.None, ButtonHelper.RedClose
                    });

                //var msg = await builder.SendAsync(ctx.Channel).ConfigureAwait(false);
                var msg = await ctx.EditResponseAsync(builder).ConfigureAwait(false);

                var result = await msg.WaitForButtonAsync(ctx.User, TimeSpan.FromMinutes(2)).ConfigureAwait(false);
           
                //var result = await interactivity.WaitForButtonAsync(msg, TimeSpan.FromMinutes(2));

                while (!result.TimedOut && result.Result.Interaction.Data.CustomId != "Close")
                {
                    await result.Result.Interaction.CreateResponseAsync(DSharpPlus.InteractionResponseType.DefferedMessageUpdate).ConfigureAwait(false);

                    embed.ClearFields();

                    string title = "";
                    List<Item> items = new List<Item>();

                    if (result.Result.Interaction.Data.CustomId.Equals("None"))
                    {
                        title = "Inventory";
                        items = player.Inventory;
                    }
                    else
                    {
                        title = result.Result.Interaction.Data.CustomId;
                        items = player.GetItems(Enum.Parse<ItemCategory>(result.Result.Interaction.Data.CustomId));
                    }

                    

                    for (int i = 0, x = 1; i < items.Count; i += 10, x++)
                    {
                        embed.AddField($"**{title} - {x}**",
                            $"{string.Join("\n", items.GetRange(i, (i + 10 > items.Count ? items.Count - i : 10)).Select(i => $"{EmojiHelper.GetItemEmoji(i.ItemCategory)} {i.Name}"))}", true);

                    }

                    builder = new DiscordWebhookBuilder()
                    .AddEmbed(embed.Build())
                    .WithComponents(new DiscordComponent[]
                    {
                    ButtonHelper.MainHand, ButtonHelper.Helmet, ButtonHelper.Chestplate,
                    ButtonHelper.Arms, ButtonHelper.Leggings
                    })
                    .WithComponents(new DiscordComponent[]
                    {
                    ButtonHelper.Shoes, ButtonHelper.Ring, ButtonHelper.Consumable,
                    ButtonHelper.None, ButtonHelper.RedClose,
                    });

                    await ctx.EditResponseAsync(builder).ConfigureAwait(false);
                    

                    result = await msg.WaitForButtonAsync(ctx.User, TimeSpan.FromMinutes(2)).ConfigureAwait(false);
                    
                }

                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        [SlashCommand("ping", "Sample command")]
        public async Task Ping(InteractionContext ctx)
        {
            try
            {
                await ctx.Channel.SendMessageAsync("PONG!").ConfigureAwait(false);
                Console.WriteLine("Command recieved");
                await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Pong")).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }

        [SlashCommand("testings", "Sample 2 command")]
        public async Task Pong(InteractionContext ctx)
        {
            try
            {
                await ctx.Channel.SendMessageAsync("test!").ConfigureAwait(false);
                await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Pong")).ConfigureAwait(false);
                await ctx.Channel.SendMessageAsync("test").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }
    }
}
