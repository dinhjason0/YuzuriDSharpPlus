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

namespace Yuzuri
{
    public class Bot
    {
        public DiscordClient Client { get; private set; }
        public CommandsNextExtension Commands { get; private set; } 
        public InteractionCreateEventArgs Interaction { get; private set; }
        public ConfigJson Config { get; protected set; }
        public static PlayerManager PlayerManager { get; private set; }
        public async Task RunAsync()
        {
            Config = RegisterConfig().Result;

            PlayerManager = new PlayerManager(Config.PlayerFilePath);

            var discordConfig = new DiscordConfiguration
            {
                Token = Config.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
            };

            Client = new DiscordClient(discordConfig);

            

            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { Config.Prefix }                

            };

            Commands = Client.UseCommandsNext(commandsConfig);
            Commands.RegisterCommands<Players>();

            await Client.ConnectAsync();

            await Task.Delay(1);
        }

        private async Task<ConfigJson> RegisterConfig()
        {
            var json = string.Empty;
            using (var fs = File.OpenRead("data/config.json"))
            using (StreamReader sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync().ConfigureAwait(false);

            var configJson = JsonConvert.DeserializeObject<ConfigJson>(json);

            Console.WriteLine("Loaded Config file.");

            return configJson;
        }

        private void RegisterEvents()
        {
            Client.Ready += OnClientReady;
        }

        private Task OnClientReady(DiscordClient sender, ReadyEventArgs e)
        {
            return Task.CompletedTask;
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
