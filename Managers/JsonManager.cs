using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Yuzuri.Commons;

namespace Yuzuri.Managers
{
    class JsonManager
    {
        //Dimensions for Sprite Sheet tiles
        /*
        public List<List<int>> SpriteDestinationList()
        {
            using (StreamReader reader = new StreamReader($"data/Sprite_Resources/PlayerSheetAssistant.json"))
            {
                List<List<int>> value = new List<List<int>>();
                string content = reader.ReadToEnd();
                reader.Close();
                Console.WriteLine("The listed coordinates are as follows:\n" + content + "\n\n");
                var contentListing = content.Split('\n');
                for (var i = 0; i < contentListing.Length; i++)
                {
                    var matches = Regex.Matches(contentListing[i], "\"([^}]+)\"");
                    foreach (Match match in matches)
                    {
                        contentListing[i] = match.Groups[1].Value;
                    }
                    Console.WriteLine(contentListing[i]);
                    if (contentListing[i] == "{\r" || contentListing[i] == "}" || contentListing[i] == "{" || contentListing[i] == "}\r" || contentListing[i] == "}\n")
                    { }
                    else
                        value.Add(SpriteDestination(contentListing[i]));
                }
                return value;
            }
        }*/

        public List<string> SpriteNames()
        {
            using (StreamReader reader = new StreamReader($"data/Sprite_Resources/PlayerSheetAssistant.json"))
            using (JsonTextReader fileContent = new JsonTextReader(reader))
            {
                List<string> value = new List<string>();
                string content = reader.ReadToEnd();
                reader.Close();
                Console.WriteLine("The listed coordinates are as follows:\n" + content + "\n\n");
                var contentListing = content.Split('\n');
                for (var i = 0; i < contentListing.Length; i++)
                {

                    var matches = Regex.Matches(contentListing[i], "\"([^}]+)\"");
                    foreach (Match match in matches)
                    {
                        contentListing[i] = match.Groups[1].Value;
                    }
                    Console.WriteLine(contentListing[i]);
                    if (contentListing[i] == "{\r" || contentListing[i] == "}")
                    { }
                    else
                        value.Add(contentListing[i]);
                }
                return value;
            }
        }

        public List<Sprite> spriteRetrieval()
        {
            List<Sprite> sprites = new List<Sprite>();
            List<string> spriteNames = SpriteNames();
            foreach (string str in spriteNames)
            {
                sprites.Add(new Sprite(str));
            }
            return sprites;
        }

        public bool SpriteExistCheck(string target)
        {
            List<string> listToCheck = SpriteNames();
            foreach (string str in listToCheck)
            {
                if (target == str)
                    return true;
            }
            return false;
        }

        public void organisePlayerSheetAssistant()
        {
            //Heads - Column 1
            //Torsos - Column 2
            //Tall Straight Legs - Column 3 
            //Short Straight Legs - Column 4 // Shoes - Column 8
            //Tall Curve Legs - Column 5 // Shoes - Column 9
            //Short Curve Legs - Column 6 // Shoes - Column 10
            //Misc.Shoes - Column 11 // Players w/ Snowflake - 12

            List<Sprite> sprites = spriteRetrieval();
            string toJson = "{\n";
            for (int i = 0; i <= 12; i++)
            {
                List<Sprite> organiseChunk = new List<Sprite>();
                foreach (Sprite spr in sprites)
                {
                    if (i == spr.GetSpriteCoordX())
                        organiseChunk.Add(spr);
                }
                for (int g = 0; g < organiseChunk.Count - 1; g++)
                {
                    if (organiseChunk[g].GetSpriteCoordY() > organiseChunk[g + 1].GetSpriteCoordY())
                    {
                        Sprite rep = organiseChunk[g];
                        organiseChunk[g] = organiseChunk[g + 1];
                        organiseChunk[g + 1] = rep;
                    }
                }
                foreach (Sprite spr in organiseChunk)
                {
                    toJson += $"\r\"{spr.GetSpriteName()}\": [ {spr.GetSpriteCoordX()}, {spr.GetSpriteCoordY()},\n]";
                }
            }
            toJson += "}";
            Console.WriteLine(toJson);
            File.WriteAllText(@"data/Sprite_Resources/PlayerSheetAssistant.json", toJson);
        }
    }
}
