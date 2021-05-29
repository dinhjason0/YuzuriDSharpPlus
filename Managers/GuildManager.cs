using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Yuzuri.Commons;

namespace Yuzuri.Managers
{
    public class GuildManager
    {
        public GuildManager() { }

        public void WriteGuildData(YuzuGuild guild)
        {
            using StreamWriter w = File.CreateText($"data/Guilds/{guild.GuildId}.json");
            JsonSerializer searializer = new JsonSerializer();
            searializer.Serialize(w, guild);
            w.Close();
        }

        public YuzuGuild ReadGuildData(ulong id)
        {
            using StreamReader r = new StreamReader($"data/Guilds/{id}.json");
            string json = r.ReadToEnd();
            YuzuGuild guild = JsonConvert.DeserializeObject<YuzuGuild>(json);
            r.Close();

            return guild;
        }
    }
}
