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
using Yuzuri.Helpers;

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
        public async Task Generate(CommandContext ctx, [RemainingText] string target)
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

        [Command("findsprite"), Description("Tests sprite coordinates")]
        [Hidden]
        [RequirePermissions(Permissions.Administrator)]
        public async Task FindSprite(CommandContext ctx, [RemainingText] string spriteName)
        {
            Console.WriteLine($"{spriteName}");
            Sprite sprite = new Sprite(spriteName);
            await Task.Delay(100);
            var msg = await new DiscordMessageBuilder()
                .WithContent("The target sprite is @ coordinate: [" + sprite.Coordinate[0] + "," + sprite.Coordinate[1] + "]")
                .SendAsync(ctx.Channel);
            await Task.Delay(100);
        }

        [Command("calldecoder"), Description("Test SpriteSheetDecoder")]
        [Hidden]
        [RequirePermissions(Permissions.Administrator)]
        public async Task CallDecoder(CommandContext ctx)
        {
            Managers.ImageProcesserManager spriteSheetDecoder = new ImageProcesserManager();
            string targetSprite = "Beauty_Dress";
            List<int> coordinateSet = spriteSheetDecoder.SpriteDestination(targetSprite);
            var msg = await new DiscordMessageBuilder()
                .WithContent($"The target sprite [{targetSprite}] is @ coordinate: [{coordinateSet[0]} , {coordinateSet[1]}]")
                .SendAsync(ctx.Channel);
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
            Item item = new Item(itemName);
            await ItemEditor(ctx, item).ConfigureAwait(false);
        }

        [Command("edititem"), Description("Edit an item")]
        [Hidden]
        [RequirePermissions(Permissions.Administrator)]
        public async Task EditItem(CommandContext ctx, [RemainingText] string itemName)
        {
            try
            {
                Item item = Bot.ItemManager.GetItem(itemName);
                Console.WriteLine(item.ItemEffects.Count);
                if (item == null)
                {
                    await ctx.Channel.SendMessageAsync("Can't find item").ConfigureAwait(false);
                    return;
                }

                await ItemEditor(ctx, item).ConfigureAwait(false);
            }
            catch
            {
                await ctx.Channel.SendMessageAsync("Can't find item").ConfigureAwait(false);
            }
        }

        private DiscordEmbedBuilder ItemEmbedBuilder(DiscordEmbedBuilder embed, Item item, DiscordClient client)
        {
            embed.Title = $"Create new Item: {item.Name}";
            embed.Description = $"Current Stats\n" +
                            $"{EmojiHelper.GetItemEmoji("STR", client)} STR: {item.STR}\n" +
                            $"{EmojiHelper.GetItemEmoji("MPE", client)} MPE: {item.MPE}\n" +
                            $"{EmojiHelper.GetItemEmoji("DEX", client)} DEX: {item.DEX}\n" +
                            $"{EmojiHelper.GetItemEmoji("DR", client)} DR: {item.DR}\n" +
                            $"{EmojiHelper.GetItemEmoji("DESC", client)} Desc: {item.Desc}\n" +
                            $"{EmojiHelper.GetItemEmoji("RARITY", client)} Rarity: {item.Rarity}\n" +
                            $"{EmojiHelper.GetItemEmoji("ITEMCATEGORY", client)} Item Category: {item.ItemCategory}\n" +
                            $"{EmojiHelper.GetItemEmoji("ITEMEFFECT", client)} ItemEffects: {string.Join(", ", item.ItemEffects)}";
            
            embed.Footer = new DiscordEmbedBuilder.EmbedFooter()
            {
                Text = "Reply to the message with the following format to change values E.G `STR = 10`\nType `done` when you want to create the item"
            };

            return embed;
        }

        private async Task ItemEditor(CommandContext ctx, Item item)
        {
            await ctx.Channel.SendMessageAsync("WIP").ConfigureAwait(false);

            var interactivity = ctx.Client.GetInteractivity();

            var embed = new DiscordEmbedBuilder()
            {
                Title = $"Create new Item: {item.Name}"
            };

            string originalName = item.Name;

            embed.AddField("Available ItemEffects", $"{string.Join("\n", (ItemEffect[])Enum.GetValues(typeof(ItemEffect)))}", true);
            embed.AddField("Available Equippable Slots", $"{string.Join("\n", (ItemCategory[])Enum.GetValues(typeof(ItemCategory)))}", true);
            embed.AddField("Available Rarity", $"{string.Join("\n", (Rarity[])Enum.GetValues(typeof(Rarity)))}");

            embed = ItemEmbedBuilder(embed, item, ctx.Client);

            var embedMsg = await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);

            var response = await interactivity
                .WaitForMessageAsync(x =>
                    x.Channel == ctx.Channel
                    && x.Author == ctx.User,
                    timeoutoverride: TimeSpan.FromMinutes(5)
                ).ConfigureAwait(false);

            if (response.TimedOut)
            {
                embed.Color = DiscordColor.DarkRed;
                await embedMsg.ModifyAsync("Item creation timed out.", embed: embed.Build()).ConfigureAwait(false);
                return;
            }

            while (!string.Equals("done", response.Result.Content, StringComparison.OrdinalIgnoreCase))
            {
                string[] responses = response.Result.Content.Split('=');

                try
                {
                    switch (responses[0].Trim().ToUpper())
                    {
                        case "NAME":
                            item.Name = string.Join(" ", responses[1..]);
                            break;
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


                embed = ItemEmbedBuilder(embed, item, ctx.Client);

                await embedMsg.ModifyAsync(embed: embed.Build()).ConfigureAwait(false);

                await response.Result.DeleteAsync().ConfigureAwait(false);

                response = await interactivity
                .WaitForMessageAsync(x =>
                    x.Channel == ctx.Channel
                    && x.Author == ctx.User,
                    timeoutoverride: TimeSpan.FromMinutes(5)
                ).ConfigureAwait(false);

                if (response.TimedOut)
                {
                    embed.Color = DiscordColor.DarkRed;
                    await embedMsg.ModifyAsync("Item creation timed out.", embed: embed.Build()).ConfigureAwait(false);
                }
            }
            embed.Color = DiscordColor.Green;
            embed.RemoveFieldRange(0, 3);

            await embedMsg.ModifyAsync("Item created", embed: embed.Build()).ConfigureAwait(false);
            await ctx.Channel.SendMessageAsync($"Created Item: {item.Name}").ConfigureAwait(false);

            if (string.Equals(item.Name, originalName, StringComparison.OrdinalIgnoreCase))
            {
                Bot.ItemManager.WriteItem(item);
            }
            else
            {
                Bot.ItemManager.WriteNewItem(item, originalName);
            }

            
            Bot.ReloadItems();
        }
    }
}
