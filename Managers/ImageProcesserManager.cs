using System;
using System.Collections.Generic;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Advanced;
using Yuzuri.Commons;
using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using SixLabors.ImageSharp.PixelFormats;

namespace Yuzuri.Managers
{
    //The coordinate for the Head is 1x1
    //The coordinate for the Torso is 2x1
    public class ImageProcesserManager
    {
        //Dimensions for Sprite Sheet tiles

        public void RemovePlayerSpriteInfo(Player player)
        {
            using StreamReader r = new StreamReader($"data/Sprite_Resources/PlayerSheetAssistant.json");
            string json = r.ReadToEnd();
            r.Close();
            StringBuilder jsonBuild = new StringBuilder(r.ReadToEnd());
            jsonBuild.Replace(player.UserId.ToString(), "available__loading");
            json = jsonBuild.ToString();
            Console.WriteLine($"\n{json}\n");
            File.WriteAllText(@"data/Sprite_Resources/PlayerSheetAssistant.json", json);
        }

        public void AddPlayerSpriteInfo(string player)
        {
            using StreamReader r = new StreamReader($"data/Sprite_Resources/PlayerSheetAssistant.json");
            string json = r.ReadToEnd();
            r.Close();
            string[] checkToSpace = json.Split("\n");
            for (int i = 0; i < checkToSpace.Length; i++)
            {
                if (checkToSpace[i] == "available__loading")
                {
                    checkToSpace[i] = player;
                    json = "";
                    foreach (string str in checkToSpace)
                    {
                        json += $"{str}\n";
                    }
                    break;
                }
            }
            Console.WriteLine($"\n{json}\n");
            File.WriteAllText(@"data/Sprite_Resources/PlayerSheetAssistant.json", json);
        }
        
        public void AddPlayerSpriteInfo(string player, List<int> playerCoords)
        {
            using StreamReader r = new StreamReader($"data/Sprite_Resources/PlayerSheetAssistant.json");
            string json = r.ReadToEnd();
            r.Close();
            string[] checkToSpace = json.Split("\n");
            json = "";
            for (int i = 0; i < checkToSpace.Length; i++)
            {
                if (i == checkToSpace.Length - 2)
                    json += $"{checkToSpace[i].Replace("\r", "")},\n\t\"{player}\": [ {playerCoords[0]} , {playerCoords[1]} ]\n";
                else if (i == checkToSpace.Length - 1)
                    json += $"}}";
                else
                    json += $"{checkToSpace[i]}\n";
            }
            Console.WriteLine($"\n{json}\n");
            File.WriteAllText(@"data/Sprite_Resources/PlayerSheetAssistant.json", json);
        }

        public FileStream CompoundedMessage (string target)
        {
            if (File.Exists($"data/Sprite_Resources/{target}temp.png"))
                File.Delete($"data/Sprite_Resources/{target}temp.png");

            using var fs = new FileStream($"data/Sprite_Resources/PlayerSheet.png", FileMode.Open, FileAccess.Read);
            using MemoryStream outStream = new MemoryStream();
            using var image = Image.Load(fs);
            {
                var pngEncoder = new PngEncoder();
                var clone = image.Clone(img => img
                .Crop(CropLocation(target)));
                clone.Save(outStream, pngEncoder);
                Console.WriteLine("Cropped Image");

                using (var fstemp = new FileStream($"data/Sprite_Resources/{target}temp.png", FileMode.CreateNew, FileAccess.ReadWrite))
                {
                    outStream.Position = 0;
                    outStream.CopyTo(fstemp);
                    Console.WriteLine("Generated Image");

                    while (File.Exists($"data/Sprite_Resources/PlayerSheet2.png"))
                    {
                        Console.WriteLine("Found PlayerSheet2");
                        fstemp.Close();
                        var fstemp2 = new FileStream($"data/Sprite_Resources/{target}temp.png", FileMode.Open, FileAccess.Read);
                        Console.WriteLine($"Read {target}");

                        return fstemp2;
                    }
                }
                return null;
            }
        }

        public void WriteToSrpiteSheet(string target, List<int> coordinates)
        {
            using var image = Image.Load<Rgba32>($"data/Sprite_Resources/PlayerSheet.png");
            using var fs = new FileStream($"data/Sprite_Resources/PlayerSheet.png", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            {
                var pngEncoder = new PngEncoder();
                Image<Rgba32> clone = image.Clone(img => img
                .Crop(CropLocation(target)));
                Console.WriteLine("Cropped Image");
                int localX = 0;
                int localY = 0;
                if (coordinates[0] > 0)
                    localX = coordinates[0] * 35 - 34;
                if (coordinates[1] > 0)
                    localY = coordinates[1] * 35 - 34;
                image.Mutate(o => o.DrawImage(clone, new Point(localX, localY), 1f));
                image.Save(fs, pngEncoder);
                fs.Close();
            }
        }

        public Rectangle CropLocation(string target)
        {
            List<int> coordinates = SpriteDestination(target);
            //coords[0] = x, coords[1] = y
            int localX = 0;
            int localy = 0;
            if (coordinates[0] > 0)
                localX = coordinates[0] * 35;
            if (coordinates[1] > 0)
                localy = coordinates[1] * 35;
            var CropSpace = new Rectangle(localX, localy, 35, 35);
            return CropSpace;
        }

        public Rectangle CropLocation(int publicX, int publicY)
        {
            int localX = 0;
            int localy = 0;
            if (publicX > 0)
                localX = publicX * 35;
            if (publicY > 0)
                localy = publicY * 35;
            var CropSpace = new Rectangle(localX, localy, 35, 35);
            return CropSpace;
        }

        public List<int> SpriteDestination(string target)
        {
            using (StreamReader reader = new StreamReader($"data/Sprite_Resources/PlayerSheetAssistant.json"))
            using (JsonTextReader fileContent = new JsonTextReader(reader))
            {
                Console.WriteLine($"This is the target being searched: >>>>{target}<<<<");
                string content = reader.ReadToEnd();
                reader.Close();
                //Console.WriteLine("The listed coordinates are as follows:\n" + content);
                var fullContent = (JObject)JsonConvert.DeserializeObject(content);
                //Console.WriteLine("Summoned new JObject from Player Sheet Assistant\n" + fullContent);
                List<int> coordinates = new List<int>();
                coordinates.Add((int)fullContent[target][0]);
                coordinates.Add((int)fullContent[target][1]);
                Console.WriteLine($"{target} @ Coordinates: [{coordinates[0]},{coordinates[1]}]\n\n");
                return coordinates;
            }
        }

        public List<List<int>> SpriteDestinationList()
        {
            using (StreamReader reader = new StreamReader($"data/Sprite_Resources/PlayerSheetAssistant.json"))
            using (JsonTextReader fileContent = new JsonTextReader(reader))
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
        }

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

        public bool SpriteExistCheck(string target)
        {
            List<string> listToCheck = SpriteNames();
            foreach(string str in listToCheck)
            {
                if (target == str)
                    return true;
            }
            return false;
        }

        public void ResizePlayerSheetAssistant(List<int> borrowCoords)
        {
            using Image<Rgba32> imageSets = Image.Load<Rgba32>("data/Sprite_Resources/PlayerSheet.png");
            using var fs = new FileStream($"data/Sprite_Resources/PlayerSheet.png", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            borrowCoords[0] = (borrowCoords[0] + 1) * 35;
            borrowCoords[1] = (borrowCoords[1]+ 1) * 35;
            using Image<Rgba32> image = new Image<Rgba32>(borrowCoords[0], borrowCoords[1]);
            {
                var png = new PngEncoder();
                image.Mutate(o => o.DrawImage(imageSets, new Point(0, 0), 1f));
                image.Save(fs, png);
                fs.Close();
                              //using var image = new Image<Rgba32>(borrowCoords[0], borrowCoords[1]);
                              //{
                              //    Console.WriteLine("Loaded ResizePlayerSheetAssistant");
                              //    var png = new PngEncoder();
                              //    Console.WriteLine("Loaded Image in ResizePlayerSheetAssistant");

                    //    var drawOver = Image.Load<Rgba32>(fs);
                    //    image.Mutate(i => i.DrawImage(drawOver, 100));
                    //    Console.WriteLine($"Image Width: {image.Width}");
                    //    Console.WriteLine($"Image Height: {image.Height}");
                    //    image.Save(fs, png);
                    //    fs.Close();
                    //}
            }
        }
    }
}
