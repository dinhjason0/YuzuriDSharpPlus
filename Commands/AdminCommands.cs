using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Yuzuri.Commons;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Memory;

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

        [Command("generate"), Description("Tests the sprite generation")]
        [Hidden]
        [RequirePermissions(DSharpPlus.Permissions.Administrator)]
        public async Task Generate(CommandContext ctx)
        {

            using var fs = new FileStream($"data\\Sprite_Resources\\PlayerSheet.png", FileMode.Open, FileAccess.Read);
            using (MemoryStream outStream = new MemoryStream())
            using (var image = Image.Load(fs))
            {
                {
                    var pngEncoder = new PngEncoder();
                    var clone = image.Clone(img => img
                    .Crop(new Rectangle(36, 1, 35, 35)));
                    clone.Save(outStream, pngEncoder);
                        var msg = await new DiscordMessageBuilder()
                    .WithContent("Generated Sprite")
                    .WithFile((FileStream)(Stream)outStream)
                    .SendAsync(ctx.Channel);
                }
            }
        }

        [Command("reloaditems"), Description("Reloads Item Dictionary")]
        public async Task ReloadItems(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("Reloading Items...");
            Bot.ReloadItems();
        }
    }
}
