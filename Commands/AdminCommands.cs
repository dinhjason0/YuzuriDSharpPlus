using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Yuzuri.Commons;

namespace Yuzuri.Commands
{
    public class AdminCommands : BaseCommandModule
    {
        [Command("reset")]
        [Hidden]
        [RequirePermissions(DSharpPlus.Permissions.Administrator)]
        public async Task Reset(CommandContext ctx, DiscordMember member)
        {
            Player player = Bot.PlayerManager.ReadPlayerData(member.Id);
            YuzuGuild guild = Bot.GuildManager.ReadGuildData(ctx.Guild.Id);

            await Bot.PlayerManager.RemovePlayerRoom(ctx.Guild, player).ConfigureAwait(false);
            //DiscordMember member = ctx.Guild.getMe;

            await member.RevokeRoleAsync(ctx.Guild.GetRole(guild.RoleId)).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync($"Removed {player.Name}'s player status.");
        }
    }
}
