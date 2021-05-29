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


        public async Task RunAsync()
        {
            StartUpCheck();

            Config = RegisterConfig().Result;

            PlayerManager = new PlayerManager();
            GuildManager = new GuildManager();

            try
            {
                var discordConfig = new DiscordConfiguration
                {
                    Token = Debug.Token2,
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
            }
            catch
            {
                Console.WriteLine("Choke at Commands Client");
            }

            RegisterEvents();

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
            //Client.Ready += OnClientReady;
            Client.GuildAvailable += GuildAvailable;
        }

        private async Task GuildAvailable(DiscordClient sender, GuildCreateEventArgs e)
        {
            await GuildCheck(Client).ConfigureAwait(false);
        }

        private void OnClientReady(DiscordClient sender, ReadyEventArgs e)
        {

        }

        private void StartUpCheck()
        {
            Console.WriteLine("Performing Startup checks...");

            Directory.CreateDirectory("data/");
            Directory.CreateDirectory("data/Players");
            Directory.CreateDirectory("data/Items");
            Directory.CreateDirectory("data/Floors");
            Directory.CreateDirectory("data/Monsters");
            Directory.CreateDirectory("data/Guilds");

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
            await Task.Delay(1000);
            Console.WriteLine("Performing Guild check...");

            foreach (KeyValuePair<ulong, DiscordGuild> guild in client.Guilds)
            {

                if (!File.Exists($"data/Guilds/{guild.Value.Id}.json"))
                {
                    Console.WriteLine($"Checking {guild.Value.Name} data... 404 NOT FOUND!");
                    YuzuGuild yuzuGuild = new YuzuGuild(guild.Value.Id);


                    bool hasRoomCategory = false;
                    bool hasFloorsCategory = false;

                    foreach (KeyValuePair<ulong, DiscordChannel> channel in guild.Value.Channels)
                    {

                        if (channel.Value.IsCategory)
                        {

                            switch (channel.Value.Name)
                            {
                                case "Player-Rooms":
                                    Console.WriteLine($"{guild.Value.Name}'s Player-Rooms... Found!");
                                    hasRoomCategory = true;
                                    yuzuGuild.RoomId = channel.Value.Id;
                                    break;
                                case "Floors":
                                    Console.WriteLine($"{guild.Value.Name}'s Floors... Found!");
                                    hasFloorsCategory = true;
                                    yuzuGuild.FloorId = channel.Value.Id;
                                    break;
                            }

                        }
                    }

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

                    bool hasPlayerRole = false;

                    Console.WriteLine("Performing Player Role Check...");



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

                    if (!hasPlayerRole)
                    {
                        Console.WriteLine($"{guild.Value.Name}'s Player Role... 404 NOT FOUND!");
                        var role = await guild.Value.CreateRoleAsync("Player").ConfigureAwait(false);
                        Console.WriteLine($"Generating {guild.Value.Name}'s Player Role...");

                        yuzuGuild.RoleId = role.Id;

                        
                    }

                    Console.WriteLine($"Generating {guild.Value.Name} data... Done.");
                    GuildManager.WriteGuildData(yuzuGuild);
                }
                else
                {
                    Console.WriteLine($"Checking {guild.Value.Name}... Found!");
                }




            }
        }



        internal struct NewStruct
        {
            public IServiceProvider service;
            public object Item2;

            public NewStruct(IServiceProvider service, object item2)
            {
                this.service = service;
                Item2 = item2;
            }

            public override bool Equals(object obj)
            {
                return obj is NewStruct other &&
                       EqualityComparer<IServiceProvider>.Default.Equals(service, other.service) &&
                       EqualityComparer<object>.Default.Equals(Item2, other.Item2);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(service, Item2);
            }

            public void Deconstruct(out IServiceProvider service, out object item2)
            {
                service = this.service;
                item2 = Item2;
            }

            public static implicit operator (IServiceProvider service, object)(NewStruct value)
            {
                return (value.service, value.Item2);
            }

            public static implicit operator NewStruct((IServiceProvider service, object) value)
            {
                return new NewStruct(value.service, value.Item2);
            }
        }
    }
}
