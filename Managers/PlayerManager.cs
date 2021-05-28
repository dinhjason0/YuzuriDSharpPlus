using Newtonsoft.Json;
using System.IO;
using Yuzuri.Commons;

namespace Yuzuri.Managers
{
    public class PlayerManager
    {
        private string FilePath { get; set; } 

        public PlayerManager(string filePath)
        {
            FilePath = filePath;
        }

        public void WritePlayerData(Player player)
        {
            using StreamWriter w = File.CreateText($"{FilePath}/{player.UserId}.json");
            JsonSerializer searializer = new JsonSerializer
            {
                Formatting = Formatting.Indented
            };
            searializer.Serialize(w, player);
            w.Close();
        }
    }
}
