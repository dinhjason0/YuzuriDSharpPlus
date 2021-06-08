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
using DSharpPlus.Interactivity.Extensions;

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
                .Crop(new Rectangle(0, 0, 35, 35)));
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
                        await Task.Delay(100);

                        fstemp2.Close();
                    }
                }
                fs.Close();
            }

            File.Delete($"data/Sprite_Resources/PlayerSheet2.png");
            await Task.Delay(100);
            Console.WriteLine("Deleted PlayerSheet2");
        }

        [Command("generatebaseplayer"), Description("Tests the sprite generation")]
        [Hidden]
        [RequirePermissions(Permissions.Administrator)]
        public async Task GenerateBasePlayer(CommandContext ctx)
        {
            await Task.Delay(100);
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

        [Command("createitem"), Description("Create an item")]
        [Hidden]
        [RequirePermissions(Permissions.Administrator)]
        public async Task CreateItem(CommandContext ctx, [RemainingText] string itemName)
        {
            await ctx.Channel.SendMessageAsync("WIP").ConfigureAwait(false);

            var interactivity = ctx.Client.GetInteractivity();

            Item item = new Item(itemName);

            var embed = new DiscordEmbedBuilder()
            {
                Title = $"Create new Item: {item.Name}"
            };

            embed.AddField("Available ItemEffects", $"{string.Join("\n", (ItemEffect[])Enum.GetValues(typeof(ItemEffect)))}", true);
            embed.AddField("Available Equippable Slots", $"{string.Join("\n", (ItemCategory[])Enum.GetValues(typeof(ItemCategory)))}", true);
            embed.AddField("Available Rarity", $"{string.Join("\n", (Rarity[])Enum.GetValues(typeof(Rarity)))}");

            embed = ItemEmbedBuilder(embed, item);

            var embedMsg = await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);

            var response = await interactivity
                .WaitForMessageAsync(x =>
                    x.Channel == ctx.Channel
                    && x.Author == ctx.User
                ).ConfigureAwait(false);

            if (response.TimedOut) return;
            
            while (!string.Equals("done", response.Result.Content, StringComparison.OrdinalIgnoreCase))
            {
                string[] responses = response.Result.Content.Split('=');

                try
                {
                    switch (responses[0].Trim().ToUpper())
                    {
                        case "STR":
                            item.STR = int.Parse(responses[1].Trim());
                            break;
                        case "DEX":
                            item.DEX = int.Parse(responses[1].Trim());
                            break;
                        case "MPE":
                            item.MPE = int.Parse(responses[1].Trim());
                            break;
                        case "DR":
                            item.DR = int.Parse(responses[1].Trim());
                            break;
                        case "DESC":
                            item.Desc = string.Join(" ", responses[1..]);
                            break;
                        case "ITEMEFFECT":
                            Enum.TryParse(responses[1].Trim(), out ItemEffect itemEffect);
                            if (item.ItemEffects.Contains(itemEffect)) item.ItemEffects.Remove(itemEffect);
                            else item.ItemEffects.Add(itemEffect);

                            if (item.ItemEffects.Count == 0) item.ItemEffects.Add(ItemEffect.None);
                            else if (item.ItemEffects.Count > 0) item.ItemEffects.Remove(ItemEffect.None);
                            break;
                        case "ITEMCATEGORY":
                            Enum.TryParse(responses[1].Trim(), out ItemCategory itemCategory);
                            item.ItemCategory = itemCategory;
                            break;
                        case "RARITY":
                            Enum.TryParse(responses[1].Trim(), out Rarity rarity);
                            item.Rarity = rarity;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                

                embed = ItemEmbedBuilder(embed, item);

                await embedMsg.ModifyAsync(embed: embed.Build()).ConfigureAwait(false);

                await response.Result.DeleteAsync().ConfigureAwait(false);

                response = await interactivity
                .WaitForMessageAsync(x =>
                    x.Channel == ctx.Channel
                    && x.Author == ctx.User
                ).ConfigureAwait(false);
            }

            await ctx.Channel.SendMessageAsync($"Created Item: {item.Name}").ConfigureAwait(false);

            Bot.ItemManager.WriteItem(item);

        }

        private DiscordEmbedBuilder ItemEmbedBuilder(DiscordEmbedBuilder embed, Item item)
        {
            embed.Description = $"Current Stats\n" +
                            $"STR: {item.STR}\n" +
                            $"MPE: {item.MPE}\n" +
                            $"DEX: {item.DEX}\n" +
                            $"DR: {item.DR}\n" +
                            $"Desc: {item.Desc}\n" +
                            $"Rarity: {item.Rarity}\n" +
                            $"Item Category: {item.ItemCategory}\n" +
                            $"ItemEffects: {string.Join(", ", item.ItemEffects)}";
            
            embed.Footer = new DiscordEmbedBuilder.EmbedFooter()
            {
                Text = "Reply to the message with the following format to change values E.G `STR = 10`"
            };

            return embed;
        }
    }
}
