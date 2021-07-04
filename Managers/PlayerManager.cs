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
        public ItemManager ItemManager { get; private set; }

        public PlayerManager(IServiceProvider provider) 
        {
            GuildManager = provider.GetRequiredService<GuildManager>();
            ItemManager = provider.GetRequiredService<ItemManager>();
        }

        /// <summary>
        /// Creates a new player object
        /// </summary>
        /// <param name="id">Snowflake Id of user</param>
        /// <param name="name">User's adventure name</param>
        /// <returns></returns>
        public Player NewPlayer(ulong id, string name)
        {
            Player player = new Player(id, name);

            player.GiveItem(ItemManager.GetItem("Potion"));
            player.GiveItem(ItemManager.GetItem("Copper Sword"));

            player.EquipItem(Player.EquippedSlots.Helmet, ItemManager.GetItem("Leather Helmet"));
            player.EquipItem(Player.EquippedSlots.Chest, ItemManager.GetItem("Leather Tunic"));
            player.EquipItem(Player.EquippedSlots.Arms, ItemManager.GetItem("Leather Wrist Guards"));
            player.EquipItem(Player.EquippedSlots.Legs, ItemManager.GetItem("Leather Pants"));
            player.EquipItem(Player.EquippedSlots.Shoes, ItemManager.GetItem("Leather Boots"));
            player.EquipItem(Player.EquippedSlots.Ring, ItemManager.GetItem("Copper Ring"));
            player.EquipItem(Player.EquippedSlots.MainHand, ItemManager.GetItem("Copper Sword"));

            return player;
        }

        /// <summary>
        /// Saves player data
        /// </summary>
        /// <param name="player">Player data to save</param>
        public static void WritePlayerData(Player player)
        {
            using StreamWriter w = File.CreateText($"data/Players/{player.UserId}.json");
            JsonSerializer searializer = new JsonSerializer();
            searializer.Serialize(w, player);
            w.Close();
        }

        /// <summary>
        /// Load a players data
        /// </summary>
        /// <param name="id">Snowflake id of players data to load</param>
        /// <returns></returns>
        public Player ReadPlayerData(ulong id)
        {
            using StreamReader r = new StreamReader($"data/Players/{id}.json");
            string json = r.ReadToEnd();
            Player player = JsonConvert.DeserializeObject<Player>(json);
            r.Close();
            return player;
        }

        /// <summary>
        /// Creates player's personal room
        /// </summary>
        /// <param name="guild">Guild for the room to be made in</param>
        /// <param name="player">Player for the room to be made for</param>
        /// <returns></returns>
        public async Task<DiscordChannel> CreatePlayerRoom(DiscordGuild guild, Player player)
        {

            YuzuGuild yuzuGuild = GuildManager.ReadGuildData(guild.Id);

            DiscordOverwriteBuilder[] discordOverwrite = new DiscordOverwriteBuilder[1];
            discordOverwrite[0] = new DiscordOverwriteBuilder(await guild.GetMemberAsync(player.UserId).ConfigureAwait(false)) { Allowed = Permissions.AccessChannels };

            return await guild.CreateTextChannelAsync($"{player.Name}s Room", guild.GetChannel(yuzuGuild.RoomId), $"{player.Name}'s Room", discordOverwrite);
        }

        /// <summary>
        /// Remove a players room
        /// </summary>
        /// <param name="guild">Guild for the room to be removed from</param>
        /// <param name="player">Player for the players room to be removed</param>
        /// <returns></returns>
        public async Task RemovePlayerRoom(DiscordGuild guild, Player player)
        {
            try
            {
                await guild.GetChannel(player.RoomId).DeleteAsync().ConfigureAwait(false);
            }
            catch
            { }
            
        }

        /// <summary>
        /// Check if the user has the player role. Returns playerrole if they do
        /// </summary>
        /// <param name="guild">Guild for the player role check</param>
        /// <param name="member">Member to check player role</param>
        /// <param name="playerRole">Returns player role if found</param>
        /// <returns>Returns true if the player has the guilds player role</returns>
        public static bool PlayerRoleCheck(DiscordGuild guild, DiscordMember member, out DiscordRole playerRole)
        {
            YuzuGuild yuzuGuild = GuildManager.ReadGuildData(guild.Id);
            playerRole = guild.GetRole(yuzuGuild.RoleId);

            return member.Roles.Contains(playerRole);
        }

        /// <summary>
        /// Check if the user has the player role
        /// </summary>
        /// <param name="guild">Guild for the player role check</param>
        /// <param name="member">Member to check player role</param>
        /// <returns>Returns true if the player has the guilds player role</returns>
        public static bool PlayerRoleCheck(DiscordGuild guild, DiscordMember member)
        {
            return PlayerRoleCheck(guild, member, out _);
        }

        
    }
}
