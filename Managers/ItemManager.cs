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

        public static List<Item> Items = new List<Item>();
        
        public Item GetItem(string name)
        {
            foreach (Item item in Items)
            {
                if (string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase)) return item;
            }

            return null;
        }
    }
}
