using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Yuzuri.Commons;

namespace Yuzuri.Managers
{
    public class ItemManager
    {
        public ItemManager() 
        {
            Console.WriteLine("Checking Items...");

            Items.Clear();

            LoadItems();
        }

        public List<Item> Items = new List<Item>();
        
        public Item GetItem(string name)
        {
            foreach (Item item in Items)
            {
                if (string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    if (item.ItemEffects.FindAll(i => i == ItemEffect.None).Count > 1)
                    {
                        item.ItemEffects.RemoveAll(i => i == ItemEffect.None);
                        item.ItemEffects.Add(ItemEffect.None);
                    }
                    return item;
                }
            }

            return null;
        }

        public void LoadItems()
        {
            foreach (string file in Directory.GetFiles("data/Items"))
            {
                using StreamReader r = new StreamReader(file);
                string json = r.ReadToEnd();
                Item item = JsonConvert.DeserializeObject<Item>(json);
                Items.Add(item);
                r.Close();
            }

            Console.WriteLine($"{Items.Count}(s) Items found!");
        }

        public void ReloadItems()
        {
            Items.Clear();
            LoadItems();
        }

        public void WriteItem(Item item)
        {
            using StreamWriter w = File.CreateText($"data/Items/{item.Name.Replace(" ", "")}.json");
            JsonSerializer searializer = new JsonSerializer();
            searializer.Serialize(w, item);
            w.Close();
        }

        internal void WriteNewItem(Item item, string originalName)
        {
            using StreamWriter w = File.CreateText($"data/Items/{item.Name.Replace(" ", "")}.json");
            JsonSerializer searializer = new JsonSerializer();
            searializer.Serialize(w, item);
            w.Close();

            if (File.Exists($"data/Items/{originalName.Replace(" ", "")}.json"))
                File.Delete($"data/Items/{originalName.Replace(" ", "")}.json");
        }
    }
}
