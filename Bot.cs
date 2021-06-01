using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using System;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Yuzuri.Managers;
using Yuzuri.Commands;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Entities;
using Yuzuri.Commons;


namespace Yuzuri
{
    public class Bot
    {
        public DiscordClient Client { get; private set; }
        public CommandsNextExtension Commands { get; private set; }
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
            ItemManager = new ItemManager();
            Console.WriteLine("Assets Loaded!");

            try
            {
                var discordConfig = new DiscordConfiguration
                {
                    Token = Debug.Token,
                    TokenType = TokenType.Bot,
                    AutoReconnect = true,
                    //MinimumLogLevel = LogLevel.Debug
                };

                Client = new DiscordClient(discordConfig);
            }
            catch
            {
                Console.Write("Choke @ discordConfig");
            }

            Client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromSeconds(20),

            });


            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { Config.Prefix }

            };
            try
            {
                Commands = Client.UseCommandsNext(commandsConfig);
                Commands.RegisterCommands<PlayerCommands>();
                Commands.RegisterCommands<AdminCommands>();
                Commands.RegisterCommands<ItemsCommand>();
            }
            catch
            {
                Console.WriteLine("Choke at Commands Client");
            }

            RegisterEvents();

            Console.WriteLine($"Yuzuki now Online! Startup took {(DateTime.Now - dateTime).TotalSeconds} seconds");

            await Client.ConnectAsync().ConfigureAwait(false);

            await Task.Delay(-1);

        }

        private async Task<ConfigJson> RegisterConfig()
        {
            var json = string.Empty;
            using (var fs = File.OpenRead("data/config.json"))
            using (StreamReader sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync().ConfigureAwait(false);

            ConfigJson configJson = JsonConvert.DeserializeObject<ConfigJson>(json);

            Console.WriteLine("Loading Config... OK.");

            return configJson;
        }

        private void RegisterEvents()
        {
            Client.Ready += OnClientReady;
            Client.GuildAvailable += GuildAvailable;
        }

        private async Task GuildAvailable(DiscordClient sender, GuildCreateEventArgs e)
        {
            await GuildCheck(Client).ConfigureAwait(false);
        }

        private async Task OnClientReady(DiscordClient sender, ReadyEventArgs e)
        {
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
                ConfigJson configJson = new ConfigJson()
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

        private async Task GuildCheck(DiscordClient client)
        {
            DateTime dateTime = DateTime.Now;
            Console.WriteLine("Performing Guild check...");

            foreach (KeyValuePair<ulong, DiscordGuild> guild in client.Guilds)
            {
                YuzuGuild yuzuGuild;

                if (!File.Exists($"data/Guilds/{guild.Value.Id}.json"))
                {
                    Console.WriteLine($"Checking {guild.Value.Name} data... 404 NOT FOUND!");
                    yuzuGuild = new YuzuGuild(guild.Value.Id);
                }
                else
                {
                    Console.WriteLine($"Checking {guild.Value.Name} data... OK.");
                    yuzuGuild = GuildManager.ReadGuildData(guild.Value.Id);
                }

                // Check if already exists
                bool hasRoomCategory = yuzuGuild.RoomId != 0;
                bool hasFloorsCategory = yuzuGuild.FloorId != 0;

                // If both do skip
                if (!hasRoomCategory && !hasFloorsCategory)
                {
                    foreach (KeyValuePair<ulong, DiscordChannel> channel in guild.Value.Channels)
                    {
                        if (channel.Value.IsCategory)
                        {
                            // Skip if one already exists
                            switch (channel.Value.Name)
                            {
                                case "Player-Rooms":
                                    if (hasRoomCategory) break;

                                    Console.WriteLine($"{guild.Value.Name}'s Player-Rooms... Found!");
                                    hasRoomCategory = true;
                                    yuzuGuild.RoomId = channel.Value.Id;
                                    break;
                                case "Floors":
                                    if (hasFloorsCategory) break;

                                    Console.WriteLine($"{guild.Value.Name}'s Floors... Found!");
                                    hasFloorsCategory = true;
                                    yuzuGuild.FloorId = channel.Value.Id;
                                    break;
                            }

                        }
                    }

                    // Add new categories
                    if (!hasRoomCategory)
                    {
                        DiscordOverwriteBuilder[] discordOverwrite = new DiscordOverwriteBuilder[1];
                        discordOverwrite[0] = new DiscordOverwriteBuilder(guild.Value.EveryoneRole) { Denied = Permissions.AccessChannels };


                        Console.WriteLine($"{guild.Value.Name}'s Player-Rooms... 404 NOT FOUND!");
                        var room = await guild.Value.CreateChannelCategoryAsync("Player-Rooms", discordOverwrite).ConfigureAwait(false);
                        Console.WriteLine($"Generating {guild.Value.Name}'s Player-Rooms...");

                        yuzuGuild.RoomId = room.Id;
                    }
                    if (!hasFloorsCategory)
                    {
                        DiscordOverwriteBuilder[] discordOverwrite = new DiscordOverwriteBuilder[1];
                        discordOverwrite[0] = new DiscordOverwriteBuilder(guild.Value.EveryoneRole) { Denied = Permissions.AccessChannels };

                        Console.WriteLine($"{guild.Value.Name}'s Floors... 404 NOT FOUND!");
                        var floor = await guild.Value.CreateChannelCategoryAsync("Floors", discordOverwrite).ConfigureAwait(false);
                        Console.WriteLine($"Generating {guild.Value.Name}'s Floors...");

                        yuzuGuild.FloorId = floor.Id;
                    }
                }
                else
                {
                    Console.WriteLine($"{guild.Value.Name}'s Categories... OK.");
                }

                // Check if already exists
                bool hasPlayerRole = yuzuGuild.RoleId != 0;

                // if it does skip
                if (!hasPlayerRole)
                {
                    foreach (KeyValuePair<ulong, DiscordRole> roles in guild.Value.Roles)
                    {

                        if (roles.Value.Name == "Player")
                        {
                            Console.WriteLine($"{guild.Value.Name}'s Player Role... Found!");
                            hasPlayerRole = true;
                            yuzuGuild.RoleId = roles.Value.Id;
                            break;
                        }
                    }
                    
                    // Add new role
                    if (!hasPlayerRole)
                    {
                        Console.WriteLine($"{guild.Value.Name}'s Player Role... 404 NOT FOUND!");
                        var role = await guild.Value.CreateRoleAsync("Player").ConfigureAwait(false);
                        Console.WriteLine($"Generating {guild.Value.Name}'s Player Role...");

                        yuzuGuild.RoleId = role.Id;


                    }
                }
                else
                {
                    Console.WriteLine($"{guild.Value.Name}'s Roles... OK.");
                }

                Console.WriteLine($"Generating {guild.Value.Name} data... Done.");
                GuildManager.WriteGuildData(yuzuGuild);
            }

            Console.WriteLine($"Discord Requirements check took {(DateTime.Now - dateTime).TotalSeconds} seconds");

        }

        public static void ReloadItems()
        {
            ItemManager = new ItemManager();
        }
    }
}
