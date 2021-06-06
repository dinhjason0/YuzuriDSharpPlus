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
using System.Threading.Tasks;
using Yuzuri.Commons;
using Yuzuri.Helpers;

namespace Yuzuri.Commands
{
    public class PlayerCommands : BaseCommandModule
    {
        private readonly Random rng = new Random();

        [Command("start"), Description("Start your adventure!"), Aliases("adventure")]
        public async Task Start(CommandContext ctx)
        {
            if (!PlayerRoleCheck(ctx.Guild, ctx.Member, out DiscordRole playerRole))
            {
                var interactivity = ctx.Client.GetInteractivity();

                var embed = new DiscordEmbedBuilder()
                { 
                    Title = "Weclome New User",
                    Description = "New arrivals, please head towards the registry table."
                };

                var msg = await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
                await Task.Delay(1300);
                embed.Description = "Hello new user, please state your name";
                await msg.ModifyAsync(embed: embed.Build()).ConfigureAwait(false);

                /*
                await ctx.Channel.SendMessageAsync($"New arrivals, please head towards the registry table.").ConfigureAwait(false);
                await Task.Delay(1100);
                await ctx.Channel.SendMessageAsync($"Hello new user, please state your name").ConfigureAwait(false);
                */

                var response = await interactivity
                    .WaitForMessageAsync(x =>
                        x.Channel == ctx.Channel
                        && x.Author == ctx.User
                    ).ConfigureAwait(false);

                await response.Result.DeleteAsync().ConfigureAwait(false);

                if (response.TimedOut)
                {
                    embed.Description = "User has not responded within allocated time. Disconnecting user...";
                    await msg.ModifyAsync(embed: embed.Build()).ConfigureAwait(false);
                    //await ctx.Channel.SendMessageAsync($"User has not responded within allocated time. Returning user back...").ConfigureAwait(false);
                }
                else
                {
                    embed.Title = $"Welcome {response.Result.Content}";
                    embed.Description = $"**User: {response.Result.Content}\n**" +
                        $"Permission granted. Please hold, we are currently loading your quarters.\n";
                    await msg.ModifyAsync(embed: embed.Build()).ConfigureAwait(false);
                    await Task.Delay(1500);

                    string embedString = $"**User: {response.Result.Content}**\n" +
                        $"Permission granted. Please hold, we are currently loading your quarters.\n" +
                        $"Requesting cloud data... ";

                    embed.Description = embedString;
                    await msg.ModifyAsync(embed: embed.Build()).ConfigureAwait(false);
                    await Task.Delay(600);

                    int count = 0;
                    for (int i = 0; i < 3; i++)
                    {
                        count += rng.Next(1, 5);
                        Console.WriteLine(count);
                        if (i == 2) count = 10;
                        embed.Description = $"{embedString}  {new string('⬛', count)}{new string('⬜', 10-count)} {10*count}%";
                        await msg.ModifyAsync(embed: embed.Build()).ConfigureAwait(false);
                        await Task.Delay(800);
                    }

                    /*
                    await ctx.Channel.SendMessageAsync($"Permission granted. Please hold, we are currently loading your quarters.").ConfigureAwait(false);
                    await Task.Delay(1500);
                    await ctx.Channel.SendMessageAsync($"Requesting cloud data... Permission granted. linking to profile.").ConfigureAwait(false);
                    await Task.Delay(900);
                    await ctx.Channel.SendMessageAsync($"Link connection has been established. Say hello to your new room {response.Result.Content}").ConfigureAwait(false);
                    */
                    await Task.Delay(200);
                    embed.Description = $"Link connection has been established. Say hello to your new room {response.Result.Content}";
                    await msg.ModifyAsync(embed: embed.Build()).ConfigureAwait(false);

                    await ctx.Member.GrantRoleAsync(playerRole).ConfigureAwait(false);
                    try
                    {
                        Player player = new Player(ctx.User.Id, response.Result.Content);

                        var room = await Bot.PlayerManager.CreatePlayerRoom(ctx.Guild, player).ConfigureAwait(false);
                        player.RoomId = room.Id;

                        Bot.PlayerManager.WritePlayerData(player);

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
                await ctx.Channel.SendMessageAsync($"You're already a player!").ConfigureAwait(false);
            }
        }

        [Command("stats"), Description("View your stats")]
        public async Task Stats(CommandContext ctx)
        {
            if (PlayerRoleCheck(ctx.Guild, ctx.Member, out _))
            {
                Player player = Bot.PlayerManager.ReadPlayerData(ctx.User.Id);

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
                    $"{DiscordEmoji.FromName(ctx.Client, ":sparkling_heart:")} HP: {player.HP}\n" +
                    $"{DiscordEmoji.FromName(ctx.Client, ":crossed_swords:")} STR: {player.STR}\n" +
                    $"{DiscordEmoji.FromName(ctx.Client, ":bow_and_arrow:")} DEX: {player.DEX}\n" +
                    $"{DiscordEmoji.FromName(ctx.Client, ":dash:")} SPD: {player.SPD}\n" +
                    $"{DiscordEmoji.FromName(ctx.Client, ":crystal_ball:")} MPE: {player.MPE}\n" +
                    $"{DiscordEmoji.FromName(ctx.Client, ":game_die:")} DHL: {player.DHL}\n" +
                    $"{DiscordEmoji.FromName(ctx.Client, ":dart:")} HIT: {player.HIT}", true);
                try
                {

                    embed.AddField("**Equipped**",
                        $"{DiscordEmoji.FromName(ctx.Client, ":billed_cap:")} Helmet: {player.Equipped[Player.EquippedSlots.Helmet].Name}\n" +
                        $"{DiscordEmoji.FromName(ctx.Client, ":shirt:")} Chest: {player.Equipped[Player.EquippedSlots.Chest].Name}\n" +
                        $"{DiscordEmoji.FromName(ctx.Client, ":gloves:")} Gloves: {player.Equipped[Player.EquippedSlots.Arms].Name}\n" +
                        $"{DiscordEmoji.FromName(ctx.Client, ":jeans:")} Legs: {player.Equipped[Player.EquippedSlots.Legs].Name}\n" +
                        $"{DiscordEmoji.FromName(ctx.Client, ":athletic_shoe:")} Feet: {player.Equipped[Player.EquippedSlots.Shoes].Name}\n", true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                embed.AddField("**Skills**", "Punch ", true);

                for (int i = 0, x = 1; i < player.Inventory.Count; i += 10, x++)
                {
                    embed.AddField($"**Inventory - {x}**",
                        $"{string.Join("\n", player.Inventory.GetRange(i, (i + 10 > player.Inventory.Count ? player.Inventory.Count - i : 10)).Select(i => $"{EmojiHelper.GetItemEmoji(i.ItemCategory, ctx.Client)} {i.Name}"))}", true);

                }

                embed.WithFooter($"Inventory Slots: {player.Inventory.Count}/100");

                await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
            }
            else
            {
                await ctx.Channel.SendMessageAsync("You haven't started your adventure yet! Use the `start` command!").ConfigureAwait(false);
            }
        }

        [Command("quit"), Description("End your adventure"), RequireRoles(RoleCheckMode.Any, new string[] { "Player" })]
        public async Task Quit(CommandContext ctx)
        {
            if (PlayerRoleCheck(ctx.Guild, ctx.Member, out DiscordRole discordRole))
            {
                var interactivity = ctx.Client.GetInteractivity();

                var msg = await ctx.Channel.SendMessageAsync("Are you sure you want to end your adventure? React to the tick emoji to confirm.");
                await msg.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:")).ConfigureAwait(false);
                
                var reaction = await interactivity
                    .WaitForReactionAsync(x =>
                        x.Channel == ctx.Channel
                        && x.User == ctx.User
                        && x.Emoji == DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"))
                    .ConfigureAwait(false);

                if (reaction.TimedOut)
                {
                    await ctx.Channel.SendMessageAsync($"Request to quit timed out. Returning user to their adventure...");
                }
                else
                {
                    await ctx.Channel.SendMessageAsync("Your adventure ends here.").ConfigureAwait(false);
                    await ctx.Member.RevokeRoleAsync(discordRole).ConfigureAwait(false);
                    await Bot.PlayerManager.RemovePlayerRoom(ctx.Guild, Bot.PlayerManager.ReadPlayerData(ctx.User.Id)).ConfigureAwait(false);
                }
            }
            else
            {
                await ctx.Channel.SendMessageAsync("You haven't started your adventure yet! Use the `start` command!").ConfigureAwait(false);
            }
        }


        [Command("ping")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("Pong!").ConfigureAwait(false);
        }

        private bool PlayerRoleCheck(DiscordGuild guild, DiscordMember member, out DiscordRole playerRole)
        {
            YuzuGuild yuzuGuild = Bot.GuildManager.ReadGuildData(guild.Id);
            playerRole = guild.GetRole(yuzuGuild.RoleId);
            
            return member.Roles.Contains(playerRole);
        }
        
    }
}
