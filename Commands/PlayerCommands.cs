using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.SlashCommands.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;
using Yuzuri.Commons;
using Yuzuri.Helpers;
using Yuzuri.Managers;

namespace Yuzuri.Commands
{
    public class PlayerCommands : BaseSlashCommandModule
    {
        public PlayerCommands(IServiceProvider p) : base(p) { }

        private readonly Random rng = new Random();

        [SlashCommand("start", 338984061341138956), Description("Start your adventure!")]
        public async Task Start(InteractionContext ctx)
        {
            await ctx.ReplyAsync("PING").ConfigureAwait(false);
            await ctx.Interaction.Channel.SendMessageAsync("PING!").ConfigureAwait(false);
            try
            {


                DiscordMember member = (DiscordMember)ctx.Interaction.User;
                if (!PlayerManager.PlayerRoleCheck(ctx.Interaction.Guild, member, out DiscordRole playerRole))
                {
                    var interactivity = Bot.BaseClient.GetInteractivity();

                    var embed = new DiscordEmbedBuilder()
                    {
                        Title = "Weclome New User",
                        Description = "New arrivals, please head towards the registry table."
                    };
                    await ctx.ReplyAsync(embeds: new DiscordEmbed[1] { embed.Build() }).ConfigureAwait(false);
                    
                    var msg = await ctx.Interaction.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
                    await Task.Delay(1300);
                    embed.Description = "Hello new user, please state your name";
                    await msg.ModifyAsync(embed: embed.Build()).ConfigureAwait(false);

                    var response = await interactivity
                        .WaitForMessageAsync(x =>
                            x.Channel == ctx.Interaction.Channel
                            && x.Author == ctx.Interaction.User
                        ).ConfigureAwait(false);



                    if (response.TimedOut)
                    {
                        embed.Title = "Connection timed out";
                        embed.Description = "User has not responded within allocated time. Disconnecting user...";
                        embed.Color = DiscordColor.DarkRed;
                        await msg.ModifyAsync(embed: embed.Build()).ConfigureAwait(false);
                    }
                    else
                    {
                        await response.Result.DeleteAsync().ConfigureAwait(false);
                        embed.Title = $"Welcome {response.Result.Content}";
                        embed.Description = $"**User: {response.Result.Content}\n**" +
                            $"Permission granted. Please hold, we are currently loading your quarters.\n";
                        await msg.ModifyAsync(embed: embed.Build()).ConfigureAwait(false);
                        await Task.Delay(1500);

                        string embedString = $"**User: {response.Result.Content}**\n" +
                            $"Permission granted. Please hold, we are currently loading your quarters.\n" +
                            $"Requesting cloud data... ";

                        embed.Color = DiscordColor.Orange;
                        embed.Description = embedString;
                        await msg.ModifyAsync(embed: embed.Build()).ConfigureAwait(false);
                        await Task.Delay(600);

                        int count = 0;
                        for (int i = 0; i < 3; i++)
                        {
                            count += rng.Next(3, 5);
                            if (i == 2) count = 10;
                            embed.Description = $"{embedString}  {new string('⬛', count)}{new string('⬜', 10 - count)} {10 * count}%";
                            await msg.ModifyAsync(embed: embed.Build()).ConfigureAwait(false);
                            await Task.Delay(800);
                        }

                        await Task.Delay(200);
                        embed.Color = DiscordColor.Green;
                        embed.Description = $"Link connection has been established. Say hello to your new room {response.Result.Content}";
                        await msg.ModifyAsync(embed: embed.Build()).ConfigureAwait(false);

                        await member.GrantRoleAsync(playerRole).ConfigureAwait(false);
                        try
                        {
                            Player player = new Player(ctx.Interaction.User.Id, response.Result.Content);

                            var room = await Bot.PlayerManager.CreatePlayerRoom(ctx.Interaction.Guild, player).ConfigureAwait(false);
                            player.RoomId = room.Id;

                            Bot.PlayerManager.WritePlayerData(player);

                            await room.SendMessageAsync($"{ctx.Interaction.User.Mention} welcome to your room.\nThis is your personal room where you can check the following: Inventory, Skills, Spells and Equipment.").ConfigureAwait(false);
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
                    await ctx.Interaction.Channel.SendMessageAsync($"You're already a player!").ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        [SlashCommand("stats"), Description("View your stats"), RequireRoles(RoleCheckMode.Any, new string[] { "Player" })]
        public async Task Stats(InteractionContext ctx)
        {
            if (PlayerManager.PlayerRoleCheck(ctx.Interaction.Guild, (DiscordMember)ctx.Interaction.User))
            {
                Player player = Bot.PlayerManager.ReadPlayerData(ctx.Interaction.User.Id);

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
                        Url = ctx.Interaction.User.AvatarUrl
                    },
                    Description = $"  Status: {status}",
                    Color = DiscordColor.Green,
                };

                embed.AddField("\n**Stats**\n",
                    $"{EmojiHelper.GetStatEmoji("HP", Bot.BaseClient)} HP: {player.HP}\n" +
                    $"{EmojiHelper.GetStatEmoji("STR", Bot.BaseClient)} STR: {player.STR}\n" +
                    $"{EmojiHelper.GetStatEmoji("DEX", Bot.BaseClient)} DEX: {player.DEX}\n" +
                    $"{EmojiHelper.GetStatEmoji("SPD", Bot.BaseClient)} SPD: {player.SPD}\n" +
                    $"{EmojiHelper.GetStatEmoji("MPE", Bot.BaseClient)} MPE: {player.MPE}\n" +
                    $"{EmojiHelper.GetStatEmoji("DHL", Bot.BaseClient)} DHL: {player.DHL}\n" +
                    $"{EmojiHelper.GetStatEmoji("HIT", Bot.BaseClient)} HIT: {player.HIT}", true);
                try
                {

                    embed.AddField("**Equipped**",
                        $"{EmojiHelper.GetItemEmoji(ItemCategory.Helmet, Bot.BaseClient)} Helmet: {player.Equipped[Player.EquippedSlots.Helmet].Name}\n" +
                        $"{EmojiHelper.GetItemEmoji(ItemCategory.Chestplate, Bot.BaseClient)} Chest: {player.Equipped[Player.EquippedSlots.Chest].Name}\n" +
                        $"{EmojiHelper.GetItemEmoji(ItemCategory.Arms, Bot.BaseClient)} Gloves: {player.Equipped[Player.EquippedSlots.Arms].Name}\n" +
                        $"{EmojiHelper.GetItemEmoji(ItemCategory.Leggings, Bot.BaseClient)} Legs: {player.Equipped[Player.EquippedSlots.Legs].Name}\n" +
                        $"{EmojiHelper.GetItemEmoji(ItemCategory.Shoes, Bot.BaseClient)} Feet: {player.Equipped[Player.EquippedSlots.Shoes].Name}\n", true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                embed.AddField("**Skills**", "Punch ", true);

                for (int i = 0, x = 1; i < player.Inventory.Count; i += 10, x++)
                {
                    embed.AddField($"**Inventory - {x}**",
                        $"{string.Join("\n", player.Inventory.GetRange(i, (i + 10 > player.Inventory.Count ? player.Inventory.Count - i : 10)).Select(i => $"{EmojiHelper.GetItemEmoji(i.ItemCategory, Bot.BaseClient)} {i.Name}"))}", true);

                }

                embed.WithFooter($"Inventory Slots: {player.Inventory.Count}/100");

                await ctx.ReplyAsync(embeds: new DiscordEmbed[1] { embed.Build() }).ConfigureAwait(false);
                //await ctx.Interaction.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
            }
            else
            {
                await ctx.Interaction.Channel.SendMessageAsync("You haven't started your adventure yet! Use the `start` command!").ConfigureAwait(false);
            }
        }

        [SlashCommand("quit"), Description("End your adventure"), RequireRoles(RoleCheckMode.Any, new string[] { "Player" })]
        public async Task Quit(InteractionContext ctx)
        {
            if (PlayerManager.PlayerRoleCheck(ctx.Interaction.Guild, (DiscordMember)ctx.Interaction.User, out DiscordRole discordRole))
            {
                var interactivity = Bot.BaseClient.GetInteractivity();

                var msg = await ctx.Interaction.Channel.SendMessageAsync("Are you sure you want to end your adventure? React to the tick emoji to confirm.");
                await msg.CreateReactionAsync(DiscordEmoji.FromName(Bot.BaseClient, ":white_check_mark:")).ConfigureAwait(false);

                var reaction = await interactivity
                    .WaitForReactionAsync(x =>
                        x.Channel == ctx.Interaction.Channel
                        && x.User == ctx.Interaction.User
                        && x.Emoji == DiscordEmoji.FromName(Bot.BaseClient, ":white_check_mark:"))
                    .ConfigureAwait(false);

                if (reaction.TimedOut)
                {
                    await ctx.Interaction.Channel.SendMessageAsync($"Request to quit timed out. Returning user to their adventure...");
                }
                else
                {
                    await ctx.Interaction.Channel.SendMessageAsync("Your adventure ends here.").ConfigureAwait(false);
                    await ((DiscordMember)ctx.Interaction.User).RevokeRoleAsync(discordRole).ConfigureAwait(false);
                    await Bot.PlayerManager.RemovePlayerRoom(ctx.Interaction.Guild, Bot.PlayerManager.ReadPlayerData(ctx.Interaction.User.Id)).ConfigureAwait(false);
                }
            }
            else
            {
                await ctx.Interaction.Channel.SendMessageAsync("You haven't started your adventure yet! Use the `start` command!").ConfigureAwait(false);
            }
        }

        [SlashCommand("inventory"), Description("View your inventory"), Aliases(new string[] { "inv" }), RequireRoles(RoleCheckMode.Any, new string[] { "Player" })]
        public async Task Inventory(InteractionContext ctx)
        {
            Player player = Bot.PlayerManager.ReadPlayerData(ctx.Interaction.User.Id);

            var embed = new DiscordEmbedBuilder
            {
                Title = $"{player.Name}'s Stats",
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail()
                {
                    Url = ctx.Interaction.User.AvatarUrl
                },
                Color = DiscordColor.Green,
            };

            for (int i = 0, x = 1; i < player.Inventory.Count; i += 10, x++)
            {
                embed.AddField($"**Inventory - {x}**",
                    $"{string.Join("\n", player.Inventory.GetRange(i, (i + 10 > player.Inventory.Count ? player.Inventory.Count - i : 10)).Select(i => $"{EmojiHelper.GetItemEmoji(i.ItemCategory, Bot.BaseClient)} {i.Name}"))}", true);

            }

            embed.WithFooter($"Inventory Slots: {player.Inventory.Count}/100");

            var msg = await ctx.Interaction.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
        }

        [SlashCommand("ping", 338984061341138956), Description("Sample command")]
        public async Task Ping(InteractionContext ctx)
        {
            try
            {
                await ctx.Interaction.Channel.SendMessageAsync("PONG!").ConfigureAwait(false);
                Console.WriteLine("Command recieved");
                await ctx.ReplyAsync("Pong").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }

        [SlashCommand("pong", 338984061341138956), Description("Sample 2 command")]
        public async Task Pong(InteractionContext ctx)
        {
            try
            {
                await ctx.Interaction.Channel.SendMessageAsync("PING!").ConfigureAwait(false);
                await ctx.ReplyAsync("PING").ConfigureAwait(false);
                await ctx.Interaction.Channel.SendMessageAsync("PONG").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }
    }
}
