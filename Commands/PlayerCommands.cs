﻿using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
                // If player is already a player
                DiscordMember member = (DiscordMember)ctx.User;
                if (!PlayerManager.PlayerRoleCheck(ctx.Guild, member, out DiscordRole playerRole))
                {
                    var interactivity = ctx.Client.GetInteractivity();

                    var embed = new DiscordEmbedBuilder()
                    {
                        Title = "Weclome New User",
                        Description = "New arrivals, please head towards the registry table."
                    };

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

                        // Loading bar
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

                        // Assign new player info
                        await member.GrantRoleAsync(playerRole).ConfigureAwait(false);
                        try
                        {
                            Player player = PlayerManager.NewPlayer(ctx.User.Id, response.Result.Content);

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

        [SlashCommand("stats", "View your stats")]
        public async Task Stats(InteractionContext ctx)
        {
            // If player is already a player
            if (PlayerManager.PlayerRoleCheck(ctx.Guild, (DiscordMember)ctx.User))
            {
                await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent("Loading Player data...")).ConfigureAwait(false);

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

                player.AddStatEmbed(embed);

                player.AddEquippedEmbed(embed);

                embed.AddField("**Skills**", "Punch ", true);

                player.AddItemEmbed(embed);

                embed.WithFooter($"Inventory Slots: {player.Inventory.Count}/100");

                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build())).ConfigureAwait(false);
                //await ctx.Interaction.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
            }
            else
            {
                await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                    .WithContent("You haven't started your adventure yet! Use the `start` command!")).ConfigureAwait(false);
            }
        }

        [SlashCommand("quit", "End your adventure"), RequireRoles(RoleCheckMode.Any, new string[] { "Player" })]
        public async Task Quit(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource).ConfigureAwait(false);
            if (PlayerManager.PlayerRoleCheck(ctx.Guild, (DiscordMember)ctx.User, out DiscordRole discordRole))
            {
                var interactivity = ctx.Client.GetInteractivity();

                var msg = await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Are you sure you want to end your adventure? React to the tick emoji to confirm.")).ConfigureAwait(false);
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

        [SlashCommand("inventory", "View your inventory"), Aliases(new string[] { "inv" })]
        public async Task Inventory(InteractionContext ctx)
        {
            // If player is already a player
            if (PlayerManager.PlayerRoleCheck(ctx.Guild, (DiscordMember)ctx.User))
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

                player.AddItemEmbed(embed);

                embed.WithFooter($"Inventory Slots: {player.Inventory.Count}/100");

                await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent("Loading inventory...")).ConfigureAwait(false);
                try
                {
                    var builder = new DiscordWebhookBuilder()
                        .AddEmbed(embed.Build())
                        .AddComponents(DiscordComponentHelper.EquipmentButtonComponents_1)
                        .AddComponents(DiscordComponentHelper.EquipmentButtonComponents_2);

                    //var msg = await builder.SendAsync(ctx.Channel).ConfigureAwait(false);
                    var msg = await ctx.EditResponseAsync(builder).ConfigureAwait(false);

                    var result = await msg.WaitForButtonAsync(ctx.User, TimeSpan.FromMinutes(2)).ConfigureAwait(false);

                    // Loop waiting for next inputs 
                    while (!result.TimedOut && !result.Result.Interaction.Data.CustomId.Equals("Close"))
                    {
                        await result.Result.Interaction.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredMessageUpdate).ConfigureAwait(false);

                        embed.ClearFields();

                        string title = "";
                        List<Item> items = new List<Item>();

                        // Get specific items based on category
                        if (result.Result.Interaction.Data.CustomId.Equals("None"))
                        {
                            title = "Inventory";
                            player.AddItemEmbed(embed);
                        }
                        else
                        {
                            title = result.Result.Interaction.Data.CustomId;
                            player.AddItemEmbed(embed, Enum.Parse<ItemCategory>(result.Result.Interaction.Data.CustomId));
                        }

                        builder.Clear();
                        builder.AddEmbed(embed.Build());

                        await ctx.EditResponseAsync(builder).ConfigureAwait(false);

                        // Wait for next button input to continue looping
                        result = await msg.WaitForButtonAsync(ctx.User, TimeSpan.FromMinutes(2)).ConfigureAwait(false);
                    }

                    // Update to remove buttons
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed)).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            else
            {
                await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                    .WithContent("You haven't started your adventure yet! Use the `start` command!")).ConfigureAwait(false);
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

        [SlashCommand("battletest", "Battle Input tests")]
        public async Task BattleTest(InteractionContext ctx)
        {
            try {
                await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource).ConfigureAwait(false);

                DiscordWebhookBuilder builder = new DiscordWebhookBuilder()
                .WithContent("Combat test, give button inputs")
                    .AddComponents(DiscordComponentHelper.EquipmentButtonComponents_1)
                    .AddComponents(DiscordComponentHelper.EquipmentButtonComponents_2);


                var timeout = DateTime.Now + TimeSpan.FromMinutes(2);

                Dictionary<ulong, string> action = new Dictionary<ulong, string>();

                // TODO: PlayerManager to handle the getting input loop
                // TODO: Proper prototype multiple turn loops
                while (DateTime.Now < timeout)
                {
                    var msg = await ctx.EditResponseAsync(builder).ConfigureAwait(false);

                    var result = await msg.WaitForButtonAsync(TimeSpan.FromMinutes(2)).ConfigureAwait(false);

                    if (!result.TimedOut)
                    {
                        await result.Result.Interaction.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredMessageUpdate).ConfigureAwait(false);

                        action[result.Result.Interaction.User.Id] = result.Result.Interaction.Data.CustomId;
                    }
                }

                await ctx.Channel.SendMessageAsync(string.Join(", ", action.Select(x => $"{x.Key}: {x.Value}")));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        [SlashCommand("equipment", "Equip specific types of gear")]
        public async Task Equipment(InteractionContext ctx)
        {
            // If player is already a player
            if (PlayerManager.PlayerRoleCheck(ctx.Guild, (DiscordMember)ctx.User))
            {
                try
                {
                    await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource).ConfigureAwait(false);

                    Player player = PlayerManager.ReadPlayerData(ctx.User.Id);

                    var embed = new DiscordEmbedBuilder
                    {
                        Title = $"{player.Name}'s Inventory",
                        Description = "Click the button to equip the type of gear",
                        Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail()
                        {
                            Url = ctx.User.AvatarUrl
                        },
                        Color = DiscordColor.Green,
                    };

                    player.AddEquippedEmbed(embed, false);
                    player.AddEquippableItemEmbed(embed);

                    var builder = new DiscordWebhookBuilder()
                        .AddEmbed(embed.Build())
                        .AddComponents(DiscordComponentHelper.EquipmentButtonComponents_1)
                        .AddComponents(DiscordComponentHelper.EquipmentButtonComponents_3);

                    var invMsg = await ctx.EditResponseAsync(builder).ConfigureAwait(false);

                    var btnResponse = await invMsg.WaitForButtonAsync(ctx.User, TimeSpan.FromMinutes(2)).ConfigureAwait(false);

                    // Loop waiting for next inputs 
                    while (!btnResponse.TimedOut && !btnResponse.Result.Interaction.Data.CustomId.Equals("Close"))
                    {
                        await btnResponse.Result.Interaction.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredMessageUpdate).ConfigureAwait(false);

                        embed.ClearFields();
                        player.AddEquippedEmbed(embed, false);

                        string title = "";

                        // Get specific items based on category
                        if (btnResponse.Result.Interaction.Data.CustomId.Equals("None"))
                        {
                            title = "Inventory";
                            player.AddEquippableItemEmbed(embed);
                        }
                        else
                        {
                            title = btnResponse.Result.Interaction.Data.CustomId;
                            player.AddItemEmbed(embed, Enum.Parse<ItemCategory>(btnResponse.Result.Interaction.Data.CustomId));
                        }


                        builder = new DiscordWebhookBuilder()
                            .AddEmbed(embed.Build())
                            .AddComponents(DiscordComponentHelper.EquipmentButtonComponents_1)
                            .AddComponents(DiscordComponentHelper.EquipmentButtonComponents_3);

                        await ctx.EditResponseAsync(builder).ConfigureAwait(false);

                        List<Item> items = player.GetItems(Enum.Parse<ItemCategory>(btnResponse.Result.Interaction.Data.CustomId));
                        var menuResponse = btnResponse;

                        if (items.Count > 0)
                        {
                            var selectMenu = new DiscordFollowupMessageBuilder()
                                .WithContent("Select the equipment from the select menu you wish to equip")
                                .AddComponents(new DiscordSelectComponent("equipment", title,
                                    DiscordComponentHelper.EquipmentMenuSelectOption(items)));

                            var menuMsg = await btnResponse.Result.Interaction.CreateFollowupMessageAsync(selectMenu).ConfigureAwait(false);

                            // Wait for TimeOut (2min), Buttons or Select Menu
                            await Task.WhenAny(
                                Task.Delay(TimeSpan.FromMinutes(2)).ContinueWith(async x =>
                                {
                                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed)).ConfigureAwait(false);
                                    await ctx.DeleteFollowupAsync(menuMsg.Id).ConfigureAwait(false);
                                    return Task.CompletedTask;
                                }),
                                invMsg.WaitForButtonAsync(ctx.User, TimeSpan.FromMinutes(3)).ContinueWith(async x =>
                                {
                                //if (timedout) return;
                                btnResponse = x.Result;

                                    await btnResponse.Result.Interaction.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredMessageUpdate).ConfigureAwait(false);

                                    await menuResponse.Result.Interaction.DeleteFollowupMessageAsync(menuMsg.Id).ConfigureAwait(false);


                                }),
                                menuMsg.WaitForSelectAsync(ctx.User, "equipment", TimeSpan.FromMinutes(3)).ContinueWith(async x =>
                                {
                                //if (timedout) return;
                                menuResponse = x.Result;
                                    await menuResponse.Result.Interaction.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredMessageUpdate).ConfigureAwait(false);

                                    Item item = player.GetItem(menuResponse.Result.Interaction.Data.Values[0].Split("_")[1]);
                                    player.EquipItem(item.ItemCategory, item);

                                    await menuResponse.Result.Interaction.DeleteFollowupMessageAsync(menuMsg.Id).ConfigureAwait(false);
                                    embed.ClearFields();
                                    player.AddEquippedEmbed(embed);
                                    player.AddItemEmbed(embed, false);

                                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                                        .AddEmbed(embed)
                                        .AddComponents(DiscordComponentHelper.EquipmentButtonComponents_1)
                                        .AddComponents(DiscordComponentHelper.EquipmentButtonComponents_2))
                                        .ConfigureAwait(false);


                                })
                            );
                        }

                        // If user didn't hit close, check what the next button is
                        if (!btnResponse.Result.Interaction.Data.CustomId.Equals("Close"))
                            btnResponse = await invMsg.WaitForButtonAsync(ctx.User, TimeSpan.FromMinutes(3)).ConfigureAwait(false);
                    }

                    // Update to remove buttons
                    embed.ClearFields();
                    player.AddEquippedEmbed(embed);
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed)).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            else
            {
                await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                    .WithContent("You haven't started your adventure yet! Use the `/start` command!")).ConfigureAwait(false);
            }
        }

        [SlashCommand("explore", "Explore the current floor for mobs")]
        public async Task Explore(InteractionContext ctx)
        {
            if (PlayerManager.PlayerRoleCheck(ctx.Guild, ctx.Member))
            {
                if (GuildManager.FloorCheck(ctx.Channel))
                {
                    await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                        .WithContent("Used in a channel")).ConfigureAwait(false);


                    await CombatManager.StartCombat(ctx);


                }
                else
                {
                    await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                        .WithContent("This command can only work in a floor channel!")).ConfigureAwait(false);
                }
            }
            else
            {
                await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                    .WithContent("You haven't started your adventure yet! Use the `/start` command!")).ConfigureAwait(false);
            }
        }
    }
}
