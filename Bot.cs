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
//fuck you jason


namespace Yuzuri
{
    public class Bot
    {
        public static DiscordClient Client { get; private set; }
        
        public CommandsNextExtension Commands { get; private set; }
        public SlashCommandsExtension Slash { get; private set; }
        public InteractionCreateEventArgs Interaction { get; private set; }
        public ConfigJson Config { get; protected set; }
        public ServiceProvider Provider { get; protected set; }
        public PlayerManager PlayerManager { get; private set; }
        public GuildManager GuildManager { get; private set; }
        public ItemManager ItemManager { get; private set; }

        public async Task RunAsync()
        {
            DateTime dateTime = DateTime.Now;
            StartUpCheck();

            Config = RegisterConfig().Result;

            Console.WriteLine("Loading Assets...");
            //PlayerManager = new PlayerManager();
            //GuildManager = new GuildManager();
            //ItemManager = new ItemManager();
            Console.WriteLine("Assets Loaded!");

            try
            {
                Client = new DiscordClient(new DiscordConfiguration
                {
                    Token = Debug.Token,
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

            Client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromSeconds(20),

            });

            ServiceProvider services = new ServiceCollection()
                .AddSingleton<Random>()
                .AddSingleton<ItemManager>()
                .AddSingleton<PlayerManager>()
                .AddSingleton<GuildManager>()
                .AddSingleton<ImageProcesserManager>()
                .BuildServiceProvider();

            ItemManager = services.GetRequiredService<ItemManager>();
            GuildManager = services.GetRequiredService<GuildManager>();
            PlayerManager = services.GetRequiredService<PlayerManager>();

            Provider = services;

            try
            {
                Commands = Client.UseCommandsNext(new CommandsNextConfiguration
                {
                    StringPrefixes = new string[] { Config.Prefix },
                    Services = services
                });
                //Commands.RegisterCommands<PlayerCommands>();
                Commands.RegisterCommands<AdminCommands>();
                //Commands.RegisterCommands<ItemsCommand>();
                //Commands.RegisterCommands(Assembly.GetExecutingAssembly());
                
                Slash = Client.UseSlashCommands(new SlashCommandsConfiguration
                {
                    Services = services
                });
                
                Slash.RegisterCommands<ItemsCommand>();
                Slash.RegisterCommands<PlayerCommands>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
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
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync().ConfigureAwait(false);

            ConfigJson configJson = JsonConvert.DeserializeObject<ConfigJson>(json);

            Console.WriteLine("Loading Config... OK.");

            return configJson;
        }

        private void RegisterEvents()
        {
            /*
            Client.InteractionCreated += async (s, e) =>
            {
                await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().WithContent(e.Interaction.))
            };*/
            Client.Ready += OnClientReady;
            Client.GuildAvailable += GuildAvailable;   
        }

        private async Task GuildAvailable(DiscordClient sender, GuildCreateEventArgs e)
        {
            DateTime dateTime = DateTime.Now;
            Console.WriteLine("Performing Guild check...");

            await GuildManager.GuildCheck(e.Guild).ConfigureAwait(false);

            Console.WriteLine($"Discord Requirements check took {(DateTime.Now - dateTime).TotalSeconds} seconds");
        }

        private async Task OnClientReady(DiscordClient sender, ReadyEventArgs e)
        {

            await sender.UpdateStatusAsync(new DiscordActivity($"Starting up...", ActivityType.Playing));

            for (int i = 0; i < 10; i++)
            {
                await sender.UpdateStatusAsync(new DiscordActivity($"Starting up... {new string('⬛', i)}{new string('⬜', 10 - i)} {10 * i}%", ActivityType.Playing), UserStatus.Idle);
                await Task.Delay(TimeSpan.FromSeconds(30)).ConfigureAwait(false);
            }

            await sender.UpdateStatusAsync(new DiscordActivity($"Exploring floor {new Random().Next(101)}", ActivityType.Playing), UserStatus.Online).ConfigureAwait(false);
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
    }
}
