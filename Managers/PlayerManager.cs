using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yuzuri.Commons;

namespace Yuzuri.Managers
{
    public class PlayerManager
    {
        public GuildManager GuildManager { get; private set; }

        public PlayerManager(IServiceProvider provider) 
        {
            GuildManager = provider.GetRequiredService<GuildManager>();
        }

        public static void WritePlayerData(Player player)
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

            YuzuGuild yuzuGuild = GuildManager.ReadGuildData(guild.Id);

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

        public static bool PlayerRoleCheck(DiscordGuild guild, DiscordMember member, out DiscordRole playerRole)
        {
            YuzuGuild yuzuGuild = GuildManager.ReadGuildData(guild.Id);
            playerRole = guild.GetRole(yuzuGuild.RoleId);

            return member.Roles.Contains(playerRole);
        }

        public static bool PlayerRoleCheck(DiscordGuild guild, DiscordMember member)
        {
            return PlayerRoleCheck(guild, member, out _);
        }
    }
}
