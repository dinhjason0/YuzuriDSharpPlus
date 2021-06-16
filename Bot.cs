using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using System;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Yuzuri.Managers;
using System.Collections.Generic;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Entities;
using Yuzuri.Commons;
using System.Net;
using System.IO.Compression;
using DSharpPlus.SlashCommands;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Yuzuri.Commands;
//Hi xVeles

namespace Yuzuri
{
    public class Bot
    {
        public static DiscordClient BaseClient { get; private set; }
        
        public CommandsNextExtension Commands { get; private set; }
        public SlashCommandsExtension Slash { get; private set; }
        public InteractionCreateEventArgs Interaction { get; private set; }
        public ConfigJson Config { get; protected set; }


        public static PlayerManager PlayerManager { get; private set; }
        public static GuildManager GuildManager { get; private set; }
        public static ItemManager ItemManager { get; private set; }

        public async Task RunAsync()
        {
            DateTime dateTime = DateTime.Now;
            StartUpCheck();

            Config = RegisterConfig().Result;

            Console.WriteLine("Loading Assets...");
            PlayerManager = new PlayerManager();
            GuildManager = new GuildManager();
            //ItemManager = new ItemManager();
            Console.WriteLine("Assets Loaded!");

            try
            {
                BaseClient = new DiscordClient(new DiscordConfiguration
                {
                    Token = Debug.Token2,
                    TokenType = TokenType.Bot,
                    AutoReconnect = true,
                    MinimumLogLevel = LogLevel.Debug,
                    Intents = DiscordIntents.AllUnprivileged
                });

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.Write("Choke @ discordConfig");
            }

            BaseClient.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromSeconds(20),

            });

            ServiceProvider services = new ServiceCollection()
                .AddSingleton<Random>()
                .AddSingleton<ItemManager>()
                .AddSingleton<PlayerManager>()
                .AddSingleton<GuildManager>()
                .BuildServiceProvider();

            ItemManager = services.GetRequiredService<ItemManager>();
            GuildManager = services.GetRequiredService<GuildManager>();
            PlayerManager = services.GetRequiredService<PlayerManager>();

            try
            {
                Commands = BaseClient.UseCommandsNext(new CommandsNextConfiguration
                {
                    StringPrefixes = new string[] { Config.Prefix },
                    Services = services
                });
                //Commands.RegisterCommands<PlayerCommands>();
                //Commands.RegisterCommands<AdminCommands>();
                Commands.RegisterCommands<ItemsCommand>();
                //Commands.RegisterCommands(Assembly.GetExecutingAssembly());

                Slash = BaseClient.UseSlashCommands(new SlashCommandsConfiguration
                {
                    Services = services, 
                });

                Slash.RegisterCommands<PlayerCommands>();
            }
            catch
            {
                Console.WriteLine("Choke at Commands Client");
            }

            RegisterEvents();

            Console.WriteLine($"Yuzuki now Online! Startup took {(DateTime.Now - dateTime).TotalSeconds} seconds");

            await BaseClient.ConnectAsync().ConfigureAwait(false);

            await Task.Delay(-1);

        }

        private async Task<ConfigJson> RegisterConfig()
        {
            var json = string.Empty;
            using (var fs = File.OpenRead("data/config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync().ConfigureAwait(false);

            ConfigJson configJson = JsonConvert.DeserializeObject<ConfigJson>(json);

            Console.WriteLine("Loading Config... OK.");

            return configJson;
        }

        private void RegisterEvents()
        {

            BaseClient.InteractionCreated += (x, y) =>
            {
                BaseClient.Logger.LogInformation("Interaction Created Received");
                return Task.CompletedTask;
            };
            BaseClient.ApplicationCommandCreated += Discord_ApplicationCommandCreated;
            BaseClient.ApplicationCommandDeleted += Discord_ApplicationCommandDeleted;
            BaseClient.ApplicationCommandUpdated += Discord_ApplicationCommandUpdated;
            BaseClient.Ready += OnClientReady;
            BaseClient.GuildAvailable += GuildAvailable;   
        }

        private async Task GuildAvailable(DiscordClient sender, GuildCreateEventArgs e)
        {
            DateTime dateTime = DateTime.Now;
            Console.WriteLine("Performing Guild check...");

            await GuildCheck(e.Guild).ConfigureAwait(false);

            Console.WriteLine($"Discord Requirements check took {(DateTime.Now - dateTime).TotalSeconds} seconds");
        }

        private async Task OnClientReady(DiscordClient sender, ReadyEventArgs e)
        {

            await sender.UpdateStatusAsync(new DiscordActivity($"Starting up...", ActivityType.Playing));
            await Task.Delay(TimeSpan.FromMinutes(5)).ConfigureAwait(false);
            await sender.UpdateStatusAsync(new DiscordActivity($"Exploring floor {new Random().Next(101)}", ActivityType.Playing)).ConfigureAwait(false);
        }

        private void StartUpCheck()
        {
            Console.WriteLine("Performing Startup checks...");

            Directory.CreateDirectory("data/");
            Directory.CreateDirectory("data/Players");
            Directory.CreateDirectory("data/Items");
            Directory.CreateDirectory("data/Floors");
            Directory.CreateDirectory("data/Monsters");
            Directory.CreateDirectory("data/Bosses");
            Directory.CreateDirectory("data/Guilds");
            Directory.CreateDirectory("data/Sprite_Resources");

            Console.WriteLine("Directories... OK.");

            if (!File.Exists("data/config.json"))
            {
                Console.WriteLine("Config... 404 NOT FOUND!");
                ConfigJson configJson = new()
                {
                    Token = "",
                    Prefix = "."
                };

                using StreamWriter w = File.CreateText("data/config.json");
                JsonSerializer searializer = new JsonSerializer
                {
                    Formatting = Formatting.Indented
                };
                searializer.Serialize(w, configJson);
                w.Close();

                Console.WriteLine("Generating Config File... Done.");
            }
            else
            {
                Console.WriteLine("Locating Config... Found!");
            }


            Console.WriteLine("Startup checks completed.");
        }

        private async Task GuildCheck(DiscordGuild guild)
        {
            try
            {
                
                YuzuGuild yuzuGuild;

                if (!File.Exists($"data/Guilds/{guild.Id}.json"))
                {
                    Console.WriteLine($"Checking {guild.Name} data... 404 NOT FOUND!");
                    yuzuGuild = new YuzuGuild(guild.Id);
                }
                else
                {
                    Console.WriteLine($"Checking {guild.Name} data... OK.");
                    yuzuGuild = GuildManager.ReadGuildData(guild.Id);
                }

                // Check if already exists
                bool hasRoomCategory = yuzuGuild.RoomId != 0;
                bool hasFloorsCategory = yuzuGuild.FloorId != 0;
                ulong resourcesChannel = yuzuGuild.Resources.Count == 0 ? 0 : yuzuGuild.Resources[0];
               /* ulong resourcesChannel = 0;
                if (yuzuGuild.Resources.Count == 0)
                    resourcesChannel = yuzuGuild.Resources[0];*/

                if (!hasRoomCategory || !hasFloorsCategory || resourcesChannel == 0)
                {
                    foreach (KeyValuePair<ulong, DiscordChannel> channel in guild.Channels)
                    {
                        if (channel.Value.IsCategory)
                        {
                            // Skip if one already exists
                            switch (channel.Value.Name)
                            {
                                case "Player-Rooms":
                                    if (hasRoomCategory) break;

                                    Console.WriteLine($"{guild.Name}'s Player-Rooms... Found!");
                                    hasRoomCategory = true;
                                    yuzuGuild.RoomId = channel.Value.Id;
                                    break;
                                case "Floors":
                                    if (hasFloorsCategory) break;

                                    Console.WriteLine($"{guild.Name}'s Floors... Found!");
                                    hasFloorsCategory = true;
                                    yuzuGuild.FloorId = channel.Value.Id;
                                    break;
                            }

                        }
                        else if (channel.Value.Name.Equals("resources", StringComparison.OrdinalIgnoreCase) && resourcesChannel == 0)
                        {
                            Console.WriteLine($"{guild.Name}'s Resource channel... Found!");
                            resourcesChannel = channel.Value.Id;
                            yuzuGuild.Resources.Add(resourcesChannel);
                        }
                    }
                    

                    // Add new categories
                    if (!hasRoomCategory)
                    {
                        DiscordOverwriteBuilder[] discordOverwrite = new DiscordOverwriteBuilder[1];
                        discordOverwrite[0] = new DiscordOverwriteBuilder(guild.EveryoneRole) { Denied = Permissions.AccessChannels };


                        Console.WriteLine($"{guild.Name}'s Player-Rooms... 404 NOT FOUND!");
                        var room = await guild.CreateChannelCategoryAsync("Player-Rooms", discordOverwrite).ConfigureAwait(false);
                        Console.WriteLine($"Generating {guild.Name}'s Player-Rooms...");

                        yuzuGuild.RoomId = room.Id;
                    }
                    if (!hasFloorsCategory)
                    {
                        DiscordOverwriteBuilder[] discordOverwrite = new DiscordOverwriteBuilder[1];
                        discordOverwrite[0] = new DiscordOverwriteBuilder(guild.EveryoneRole) { Denied = Permissions.AccessChannels };

                        Console.WriteLine($"{guild.Name}'s Floors... 404 NOT FOUND!");
                        var floor = await guild.CreateChannelCategoryAsync("Floors", discordOverwrite).ConfigureAwait(false);
                        Console.WriteLine($"Generating {guild.Name}'s Floors...");

                        yuzuGuild.FloorId = floor.Id;
                    }
                }
                else
                {
                    Console.WriteLine($"{guild.Name}'s Categories... OK.");
                }

                // Check if already exists
                bool hasPlayerRole = yuzuGuild.RoleId != 0;

                // if it does skip
                if (!hasPlayerRole)
                {
                    foreach (KeyValuePair<ulong, DiscordRole> roles in guild.Roles)
                    {

                        if (roles.Value.Name == "Player")
                        {
                            Console.WriteLine($"{guild.Name}'s Player Role... Found!");
                            hasPlayerRole = true;
                            yuzuGuild.RoleId = roles.Value.Id;
                            break;
                        }
                    }

                    // Add new role
                    if (!hasPlayerRole)
                    {
                        Console.WriteLine($"{guild.Name}'s Player Role... 404 NOT FOUND!");
                        var role = await guild.CreateRoleAsync("Player").ConfigureAwait(false);
                        Console.WriteLine($"Generating {guild.Name}'s Player Role...");

                        yuzuGuild.RoleId = role.Id;


                    }
                }
                else
                {

                    Console.WriteLine($"{guild.Name}'s Roles... OK.");

                    // Download files from resouces channel
                    try
                    {
                        if (resourcesChannel != 0)
                        {
                            DiscordChannel resources = await BaseClient.GetChannelAsync(resourcesChannel).ConfigureAwait(false);
                            Console.WriteLine($"Checking Resources... Found! Extracting data");

                            foreach (DiscordMessage msg in await resources.GetMessagesAsync().ConfigureAwait(false))
                            {

                                // IF already extracted skip
                                if (!yuzuGuild.Resources.Contains(msg.Id))
                                {
                                    if (msg.Attachments.Count != 0)
                                    {
                                        DiscordAttachment discordAttachment = msg.Attachments[0];

                                        using WebClient client = new WebClient();
                                        await client.DownloadFileTaskAsync(new Uri(discordAttachment.Url), $"{discordAttachment.FileName}").ConfigureAwait(false);

                                        if (discordAttachment.MediaType.Equals("application/zip", StringComparison.OrdinalIgnoreCase))
                                            ZipFile.ExtractToDirectory(discordAttachment.FileName, $"{Directory.GetCurrentDirectory()}/{msg.Content}", true);
                                        File.Delete(discordAttachment.FileName);

                                        yuzuGuild.Resources.Add(msg.Id);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Checking Resources... 404 NOT FOUND! Skipping...");
                        Console.WriteLine(ex);
                    }

                }

                Console.WriteLine($"Generating {guild.Name} data... Done.");
                GuildManager.WriteGuildData(yuzuGuild);


            }
            catch (Exception ex)
            {
                Console.WriteLine("Guild List could not be retrieved");
                Console.WriteLine(ex);
            }
        }

        public static void ReloadItems()
        {
            ItemManager = new ItemManager();
        }

        private static Task Discord_ApplicationCommandUpdated(DiscordClient sender, ApplicationCommandEventArgs e)
        {
            BaseClient.Logger.LogInformation($"Shard {sender.ShardId} sent application command updated: {e.Command.Name}: {e.Command.Id} for {e.Command.ApplicationId}");
            return Task.CompletedTask;
        }
        private static Task Discord_ApplicationCommandDeleted(DiscordClient sender, ApplicationCommandEventArgs e)
        {
            BaseClient.Logger.LogInformation($"Shard {sender.ShardId} sent application command deleted: {e.Command.Name}: {e.Command.Id} for {e.Command.ApplicationId}");
            return Task.CompletedTask;
        }
        private static Task Discord_ApplicationCommandCreated(DiscordClient sender, ApplicationCommandEventArgs e)
        {
            BaseClient.Logger.LogInformation($"Shard {sender.ShardId} sent application command created: {e.Command.Name}: {e.Command.Id} for {e.Command.ApplicationId}");
            return Task.CompletedTask;
        }
    }
}