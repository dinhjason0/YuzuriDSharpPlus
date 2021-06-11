using DSharpPlus;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Yuzuri.Commons;

namespace Yuzuri.Managers
{
    public class PlayerManager
    {
        public PlayerManager() { }

        public void WritePlayerData(Player player)
        {
            using StreamWriter w = File.CreateText($"data/Players/{player.UserId}.json");
            JsonSerializer searializer = new JsonSerializer();
            searializer.Serialize(w, player);
            w.Close();
        }

        public Player ReadPlayerData(ulong id)
        {
            using StreamReader r = new StreamReader($"data/Players/{id}.json");
            string json = r.ReadToEnd();
            Player player = JsonConvert.DeserializeObject<Player>(json);
            r.Close();
            return player;
        }

        public async Task<DiscordChannel> CreatePlayerRoom(DiscordGuild guild, Player player)
        {

            YuzuGuild yuzuGuild = Bot.GuildManager.ReadGuildData(guild.Id);

            DiscordOverwriteBuilder[] discordOverwrite = new DiscordOverwriteBuilder[1];
            discordOverwrite[0] = new DiscordOverwriteBuilder(await guild.GetMemberAsync(player.UserId).ConfigureAwait(false)) { Allowed = Permissions.AccessChannels };

            return await guild.CreateTextChannelAsync($"{player.Name}s Room", guild.GetChannel(yuzuGuild.RoomId), $"{player.Name}'s Room", discordOverwrite);
        }

        public async Task RemovePlayerRoom(DiscordGuild guild, Player player)
        {
            try
            {
                await guild.GetChannel(player.RoomId).DeleteAsync().ConfigureAwait(false);
            }
            catch
            { }
            
        }
    }
}
