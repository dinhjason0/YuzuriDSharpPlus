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
    public class PlayersStartUp : BaseCommandModule
    {

        [Command("start"), Description("Start your adventure!")]
        public async Task Start(CommandContext ctx, [Description("Name you wish to start with")] string name = "")
        {
            DiscordRole role = (DiscordRole)ctx.Member.Roles.Where(r => r.Name == "Player");
            
            if (role != null)
            {
                await ctx.Channel.SendMessageAsync($"You're already a player!").ConfigureAwait(false);
            }
            else
            {
                await ctx.Channel.SendMessageAsync($"Link connection has been established. Say hello to your new room {ctx.User.Username}").ConfigureAwait(false);
                await ctx.Guild.Members[ctx.User.Id].GrantRoleAsync(role).ConfigureAwait(false);

                if (name.Length == 0) name = ctx.User.Username;

                Player player = new Player(ctx.User, name);

                Bot.PlayerManager.WritePlayerData(player);
            }

        }

    }
}
