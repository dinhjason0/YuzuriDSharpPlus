using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yuzuri.Commons;

namespace Yuzuri.Commands
{
    public class Players : BaseCommandModule
    {

        [Command("adventure")]
        public async Task Start(CommandContext ctx)
        {
            Console.WriteLine("role");
            DiscordRole role = (DiscordRole)ctx.Member.Roles.Where(r => r.Name == "Player");
            Console.WriteLine("role found");
            if (role != null)
            {
                await ctx.Channel.SendMessageAsync($"You're already a player!").ConfigureAwait(false);
            }
            else
            {
                await ctx.Channel.SendMessageAsync($"Link connection has been established. Say hello to your new room {ctx.User.Username}").ConfigureAwait(false);
                await ctx.Guild.Members[ctx.User.Id].GrantRoleAsync(role).ConfigureAwait(false);

                //if (name.Length == 0) name = ctx.User.Username;

                Player player = new Player(ctx.User.Id, ctx.User.Username);

                Bot.PlayerManager.WritePlayerData(player);
            }
        }

        [Command("stats"), Description("View your stats")]
        public async Task Stats(CommandContext ctx)
        {
            Player player = Bot.PlayerManager.ReadPlayerData(ctx.User.Id);

            var embed = new DiscordEmbedBuilder
            {
                Title = $"{player.Name}'s Stats",
                Url = ctx.User.AvatarUrl,
                Color = DiscordColor.Green,
            };

            embed.AddField("**Stats**", $"HP: {player.HP}\n" +
                $"STR: {player.STR}\n" +
                $"DEX: {player.DEX}\n" +
                $"SPD: {player.SPD}\n" +
                $"MPE: {player.MPE}\n" +
                $"HIT: {player.HIT}");

            await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
        }

        [Command("ping")]
        public async Task Ping(CommandContext ctx)
        {
            ctx.Channel.SendMessageAsync("Pong!").ConfigureAwait(false);
        }
    }
}
