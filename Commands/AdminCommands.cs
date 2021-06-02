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
using Yuzuri.Managers;
using System.Linq;

namespace Yuzuri.Commands
{
    public class AdminCommands : BaseCommandModule
    {
        [Command("reset")]
        [Hidden]
        [RequirePermissions(Permissions.Administrator)]
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
        [RequirePermissions(Permissions.Administrator)]
        public async Task Generate(CommandContext ctx)
        {
            if (File.Exists($"data/Sprite_Resources/PlayerSheet2.png"))
            File.Delete($"data/Sprite_Resources/PlayerSheet2.png");

            using var fs = new FileStream($"data/Sprite_Resources/PlayerSheet.png", FileMode.Open, FileAccess.Read);
            using MemoryStream outStream = new MemoryStream();
            using var image = Image.Load(fs);
            {
                var pngEncoder = new PngEncoder();
                await Task.Delay(100);
                var clone = image.Clone(img => img
                .Crop(new Rectangle(1, 1, 35, 35)));
                clone.Save(outStream, pngEncoder);
                await Task.Delay(100);
                Console.WriteLine("Cropped Image");

                using (var fstemp = new FileStream($"data/Sprite_Resources/PlayerSheet2.png", FileMode.CreateNew, FileAccess.ReadWrite))
                {
                    outStream.Position = 0;
                    outStream.CopyTo(fstemp);
                    await Task.Delay(100);
                    Console.WriteLine("Generated Image");

                    if (File.Exists($"data/Sprite_Resources/PlayerSheet2.png"))
                    {
                        Console.WriteLine("Found PlayerSheet2");
                        fstemp.Close();
                        var fstemp2 = new FileStream($"data/Sprite_Resources/PlayerSheet2.png", FileMode.Open, FileAccess.Read);
                        await Task.Delay(100);
                        Console.WriteLine("Read PlayerSheet2");

                        var msg = await new DiscordMessageBuilder()
                        .WithContent("Generated Sprite")
                        .WithFile(fstemp2)
                        .SendAsync(ctx.Channel);
                    }
                }
            }

            File.Delete($"data/Sprite_Resources/PlayerSheet2.png");
            await Task.Delay(100);
            Console.WriteLine("Deleted PlayerSheet2");
        }


        [Command("reloaditems"), Description("Reloads Item Dictionary")]
        [Hidden]
        [RequirePermissions(Permissions.Administrator)]
        public async Task ReloadItems(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("Reloading Items...");
            ItemManager.Items.Clear();
            Bot.ReloadItems();
        }

        [Command("giveitem"), Description("Gives a player an item")]
        [Hidden]
        [RequirePermissions(Permissions.Administrator)]
        public async Task GiveItem(CommandContext ctx, DiscordMember user, [RemainingText] string itemName)
        {
            if (user.Roles.Contains(ctx.Guild.GetRole(Bot.GuildManager.ReadGuildData(ctx.Guild.Id).RoleId)))
            {
                Player player = Bot.PlayerManager.ReadPlayerData(user.Id);

                try
                {
                    Console.WriteLine($"t{itemName}2");
                    Item item = Bot.ItemManager.GetItem(itemName);

                    if (player.GiveItem(item))
                    {
                        await ctx.Channel.SendMessageAsync($"Successfully gave {item.Name} to {player.Name}").ConfigureAwait(false);
                    }
                    else
                    {
                        await ctx.Channel.SendMessageAsync($"{player.Name} Inventory is full!").ConfigureAwait(false);
                    }
                }
                catch
                {
                    await ctx.Channel.SendMessageAsync("Can't find item").ConfigureAwait(false);
                }
            }
            else
            {
                await ctx.Channel.SendMessageAsync($"{user.DisplayName} isn't a player").ConfigureAwait(false);
            }
        }

        [Command("setstatus"), Description("Sets a player status")]
        [Hidden]
        [RequirePermissions(Permissions.Administrator)]
        public async Task SetStatus(CommandContext ctx, DiscordMember user, string status)
        {
            if (user.Roles.Contains(ctx.Guild.GetRole(Bot.GuildManager.ReadGuildData(ctx.Guild.Id).RoleId)))
            {
                try
                {
                    Enum.TryParse(status, out StatusEffects statusEffect);
                    Player player = Bot.PlayerManager.ReadPlayerData(user.Id);

                    player.StatusEffects = statusEffect;
                    player.SaveData();

                    await ctx.Channel.SendMessageAsync($"Successfully set {player.Name} status to {statusEffect}").ConfigureAwait(false);
                }
                catch
                {
                    await ctx.Channel.SendMessageAsync("Invalid Status Effect").ConfigureAwait(false);
                }
            }
            else
            {
                await ctx.Channel.SendMessageAsync($"{user.DisplayName} isn't a player").ConfigureAwait(false);
            }
        }
    }
}
