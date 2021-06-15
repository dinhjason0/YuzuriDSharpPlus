using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Yuzuri.Commons;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using Yuzuri.Managers;
using System.Linq;
using DSharpPlus.Interactivity.Extensions;
using Yuzuri.Helpers;
using Newtonsoft.Json;
using System.Linq.Expressions;
using Emzi0767.Utilities;
using DSharpPlus.EventArgs;

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
            //Rip coordinate list and remove player's sprite from listing
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
                ImageProcesserManager imageProcesserManager = new ImageProcesserManager();
                Rectangle rec = imageProcesserManager.CropLocation(target);
                var clone = image.Clone(img => img
                .Crop(rec));
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
                .WithContent($"The [{spriteName}] is @ coordinate: [{sprite.SpriteCoords[0]},{sprite.SpriteCoords[1]}]")
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

        [Command("spritedestinationtest"), Description("Test SpriteSheetDestinationList()")]
        [Hidden]
        [RequirePermissions(Permissions.Administrator)]
        public async Task SpriteDestinationTest(CommandContext ctx)
        {
            Console.WriteLine("Trying to write command");
            ImageProcesserManager spriteSheetDecoder = new ImageProcesserManager();
            await Task.Delay(100);
            List<List<int>> spriteDestinationLists = spriteSheetDecoder.SpriteDestinationList();
            string sendMessage = "";
            for (int i = 0; i < spriteDestinationLists.Count(); i++)
            {
                List<int> tempList = spriteDestinationLists[i];
                sendMessage += $"[{tempList[0]} , {tempList[1]}]\n";
            }

            await ctx.Channel.SendMessageAsync(sendMessage);
            await Task.Delay(100);
        }

        [Command("loadsprite"), Description("This is to test the image writing and tracking in the Sheet Assistant")]
        [Hidden]
        [RequirePermissions(Permissions.Administrator)]
        public async Task LoadSprite(CommandContext ctx, DiscordMember member)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);
            Console.WriteLine("Accessing loadsprite command");
            if (PlayerManager.PlayerRoleCheck(ctx.Guild, ctx.Member))
            {
                ImageProcesserManager processerManager = new ImageProcesserManager();
                //Search through PlayerSheetAssistant to see if user.Id is in Assistant.json
                //If user.Id is found in Assistant.json
                //Rip coordinates from .json
                //-Locate the coordinates through the sprite-sheet
                //--Find sprite through sheet, rip, and save as an Image/Streamwriter
                if (processerManager.SpriteExistCheck(member.Id.ToString()))
                {
                    Console.WriteLine($"{member.Id}.png is attempting to be loaded");
                    //Get .json
                    FileStream file = processerManager.CompoundedMessage((member.Id).ToString());
                    await new DiscordMessageBuilder()
                    .WithContent("Generated Sprite")
                    .WithFile(file)
                    .SendAsync(ctx.Channel);
                    file.Close();
                    File.Delete($"data/Sprite_Resources/{member.Id}temp.png");
                    await Task.Delay(100);
                }
                //If it isn't found, generate a new nude-player-model
                //-Incrementally check each row for a user.Id ---- Sprite Sheet .json[0] should be linearly sorted
                //--If open slot is available within the column, take that position in the Assist and SpriteSheet
                //---If no open slots are available, create a new row and write it into the column
                else
                {
                    Console.WriteLine($"{member.Id}'s portrait isn't found, generating new portrait");
                    //Get .json
                    ImageProcesserManager spriteSheetDecoder = new ImageProcesserManager();
                    await Task.Delay(100);
                    List<string> spriteNames = spriteSheetDecoder.SpriteNames();
                    List<Sprite> playerSpriteDestinationLists = new List<Sprite>();
                    //Read PlayerSheetAssistant.json
                    //Pull out all coordinates into a list
                    for (int i = 0; i < spriteNames.Count(); i++)
                    {
                        Sprite storedSprite = new Sprite(spriteNames[i]);
                        Console.WriteLine(storedSprite.SpriteName);
                        if (storedSprite.GetSpriteCoordX() == 12)
                        {
                            playerSpriteDestinationLists.Add(storedSprite);
                            Console.WriteLine($"{storedSprite.GetSpriteName()} was added to Player Sprite Destination Lists");
                        }
                    }
                    Console.WriteLine($"\nPlayer Sprite Destination Lists For Players:");
                    if (playerSpriteDestinationLists.Count() != 0)
                        foreach (Sprite spritedata in playerSpriteDestinationLists)
                        {
                            Console.WriteLine($"{spritedata.GetSpriteName()}");
                        }
                    else
                    {
                        Console.WriteLine("LOL THERE ARE NONE");
                    }
                    Console.WriteLine();
                    //Check all x/coordinates[0] as 12 position
                    //Return y/coordinates[1] position incrementally
                    //If there is "available__loading" before the latest, use that sprite
                    List<int> borrowedCoords = new List<int>();
                    if (playerSpriteDestinationLists.Count() != 0)
                        for (int i = 0; i < playerSpriteDestinationLists.Count(); i++)
                        {
                            if (playerSpriteDestinationLists[i].SpriteName == "available__loading")
                            {
                                borrowedCoords = playerSpriteDestinationLists[i].SpriteCoords;
                                processerManager.AddPlayerSpriteInfo(playerSpriteDestinationLists[i].SpriteName);
                                break;
                            }

                            if (i == playerSpriteDestinationLists.Count() - 1)
                            {
                                borrowedCoords.Add(playerSpriteDestinationLists[i].SpriteCoords[0]);
                                borrowedCoords.Add(playerSpriteDestinationLists[i].SpriteCoords[1] + 1);
                                processerManager.AddPlayerSpriteInfo(member.Id.ToString(), borrowedCoords);
                                break;
                            }
                        }
                    else
                    {
                        borrowedCoords.Add(12);
                        borrowedCoords.Add(0);
                        processerManager.AddPlayerSpriteInfo(member.Id.ToString(), borrowedCoords);
                        List<List<int>> sprDesLi = spriteSheetDecoder.SpriteDestinationList();
                        foreach (List<int> lint in sprDesLi)
                        {
                            if (borrowedCoords[1] < lint[1])
                                borrowedCoords[1] = lint[1];
                        }
                        processerManager.ResizePlayerSheetAssistant(borrowedCoords);
                        borrowedCoords = processerManager.SpriteDestination(member.Id.ToString());
                    }
                    //If there is no "available__loading" within the limit, create a new coordinate set for that sprite
                    spriteSheetDecoder.WriteToSrpiteSheet("Belt_Pants_Tall_Male", borrowedCoords);
                    spriteSheetDecoder.WriteToSrpiteSheet("Naked_Torso", borrowedCoords);
                    spriteSheetDecoder.WriteToSrpiteSheet("Bald_Head", borrowedCoords);

                    FileStream file = processerManager.CompoundedMessage((member.Id).ToString());

                    await new DiscordMessageBuilder()
                    .WithContent("Here it is!")
                    .WithFile(file)
                    .SendAsync(ctx.Channel);
                    file.Close();
                    File.Delete($"data/Sprite_Resources/{member.Id}temp.png");
                    processerManager.CompoundedMessage(member.Id.ToString()).Close();
                    await Task.Delay(100);
                }
                //If player leaves
                //-Locate the coordinates through the sprite-sheet
                //--Write over the sprite in the sheet with transparency
                //---Turn the snowflake id within the Assistant.json into "0x24+1-48sqrt(44)"
                //----If players are named this, just boot them lmfao
            }
            else
            {
                await ctx.Channel.SendMessageAsync($"{member.DisplayName} isn't a player").ConfigureAwait(false);
            }
        }

        [Command("playwithimgsize"), Description("Reloads Item Dictionary")]
        [Hidden]
        [RequirePermissions(Permissions.Administrator)]
        public async Task PlayWithImgSize(CommandContext ctx)
        {
            List<int> coordssetinteger = new List<int>();
            coordssetinteger.Add(12);
            coordssetinteger.Add(12);
            ImageProcesserManager imageProcess = new ImageProcesserManager();
            imageProcess.ResizePlayerSheetAssistant(coordssetinteger);
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
        public async Task GiveItem(CommandContext ctx, DiscordMember member, [RemainingText] string itemName)
        {
            if (PlayerManager.PlayerRoleCheck(ctx.Guild, member))
            {
                Player player = Bot.PlayerManager.ReadPlayerData(member.Id);

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
                await ctx.Channel.SendMessageAsync($"{member.DisplayName} isn't a player").ConfigureAwait(false);
            }
        }

        [Command("setstatus"), Description("Sets a player status")]
        [Hidden]
        [RequirePermissions(Permissions.Administrator)]
        public async Task SetStatus(CommandContext ctx, DiscordMember member, string status)
        {
            if (PlayerManager.PlayerRoleCheck(ctx.Guild, member))
            {
                try
                {
                    Enum.TryParse(status, out StatusEffects statusEffect);
                    Player player = Bot.PlayerManager.ReadPlayerData(member.Id);

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
                await ctx.Channel.SendMessageAsync($"{member.DisplayName} isn't a player").ConfigureAwait(false);
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

        private async Task ImageLoader(CommandContext ctx, Image img)
        {
            await ctx.Channel.SendMessageAsync("WIP").ConfigureAwait(false);
            using (StreamReader reader = new StreamReader($"data/Sprite_Resources/PlayerSheetAssistant.json"))
            using (JsonTextReader fileContent = new JsonTextReader(reader))
            {

            }
        }
    }
}
