using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Yuzuri.Managers;

namespace Yuzuri.Commons
{
    public class Sprite
    {
        public string SpriteName { get; set; }
        public List<int> SpriteCoords { get; set; }
        public Rectangle CroppedSize { get; set; }

        public Sprite(string name)
        {
            SpriteName = name;
            SpriteCoords = SpriteDestination();
            EditSpriteInfo();
        }

        public Sprite(List<int> spriteCoords)
        {
            SpriteCoords = spriteCoords;
            EditSpriteInfo();
        }

        public string GetSpriteName()
        {
            return SpriteName;
        }

        public List<int> GetSpriteCoords()
        {
            return SpriteCoords;
        }

        public int GetSpriteCoordX()
        {
            return SpriteCoords[0];
        }

        public int GetSpriteCoordY()
        {
            return SpriteCoords[1];
        }


        public void EditSpriteInfo()
        {
            using StreamReader r = new StreamReader($"data/Sprite_Resources/PlayerSheetAssistant.json");
            string json = r.ReadToEnd();
            string[] checkToSpace = json.Split("\n");
            r.Close();
            json = "";
            JsonManager jsonManager = new JsonManager();
            //SpriteName is checked if exists
            //If it does, find the sprite's name, grab the coordinates, and do the sprite-sheet writing from there-on
            //-SpriteName is checked to see if it is a player
            //-If it is a player, see if there is an 'available__loading' name in the list
            //--If there is, replace the 'available__loading' name in the list with the Sprite's name and write the Sprite's information into the sheet
            //--If there isn't, write the sprite's information on the line immediately after, and then write the file back to the sheet

            //If it isn't a player~~~~~
            if (jsonManager.SpriteExistCheck(SpriteName) != true)
            {
                int key;
                if (SpriteName.Length == 18 && int.TryParse(SpriteName, out key))
                {
                    if (jsonManager.SpriteExistCheck("available__loading"))
                    for (int i = 0; i < checkToSpace.Length; i++)
                    {
                        if (checkToSpace[i] == "available__loading")
                        {
                                checkToSpace[i] = $"{checkToSpace[i].Replace("available__loading")}";
                            foreach (string str in checkToSpace)
                            {
                                json += $"{str}\n";
                            }
                            break;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < checkToSpace.Length; i++)
                        {
                            if (i == checkToSpace.Length - 2)
                                json += $"{checkToSpace[i].Replace("\r", "")},\n\t\"{SpriteName}\": [ {SpriteCoords[0]} , {SpriteCoords[1]} ]\n";
                            else if (i == checkToSpace.Length - 1)
                                json += $"}}";
                            else
                                json += $"{checkToSpace[i]}\n";
                        }
                    }
                }
            }
            else
            {
                foreach (string str in checkToSpace)
                {
                    if (str.Equals($"}}") == false)
                        json += $"{str}\n";
                    else
                    {
                        json += $"\t\"{SpriteName}\": [ {SpriteCoords[0]}, {SpriteCoords[1]} ]\n}}";
                    }
                }
            }
            Console.WriteLine($"\n{json}\n");
            File.WriteAllText(@"data/Sprite_Resources/PlayerSheetAssistant.json", json);
        }

        //This exists specifically for empty PlayerSpriteSheets - catching the worst case scenario
        //public void AddSpriteInfo()
        //{
        //    using StreamReader r = new StreamReader($"data/Sprite_Resources/PlayerSheetAssistant.json");
        //    string json = r.ReadToEnd();
        //    r.Close();
        //    Console.WriteLine($"\n{json}\n");
        //    File.WriteAllText(@"data/Sprite_Resources/PlayerSheetAssistant.json", json);
        //}

        public List<int> SpriteDestination()
        {
            using (StreamReader reader = new StreamReader($"data/Sprite_Resources/PlayerSheetAssistant.json"))
            {
                Console.WriteLine($"This is the target being searched: >>>>{SpriteName}<<<<");
                string content = reader.ReadToEnd();
                reader.Close();
                //Console.WriteLine("The listed coordinates are as follows:\n" + content);
                var fullContent = (JObject)JsonConvert.DeserializeObject(content);
                //Console.WriteLine("Summoned new JObject from Player Sheet Assistant\n" + fullContent);
                List<int> coordinates = new List<int>();
                coordinates.Add((int)fullContent[SpriteName][0]);
                coordinates.Add((int)fullContent[SpriteName][1]);
                Console.WriteLine($"{SpriteName} @ Coordinates: [{coordinates[0]},{coordinates[1]}]\n\n");
                return coordinates;
            }
        }

        public List<int> SpriteDestination(string spriteName)
        {
            using (StreamReader reader = new StreamReader($"data/Sprite_Resources/PlayerSheetAssistant.json"))
            {
                Console.WriteLine($"This is the target being searched: >>>>{spriteName}<<<<");
                string content = reader.ReadToEnd();
                reader.Close();
                //Console.WriteLine("The listed coordinates are as follows:\n" + content);
                var fullContent = (JObject)JsonConvert.DeserializeObject(content);
                //Console.WriteLine("Summoned new JObject from Player Sheet Assistant\n" + fullContent);
                List<int> coordinates = new List<int>();
                coordinates.Add((int)fullContent[spriteName][0]);
                coordinates.Add((int)fullContent[spriteName][1]);
                Console.WriteLine($"{spriteName} @ Coordinates: [{coordinates[0]},{coordinates[1]}]\n\n");
                return coordinates;
            }
        }

        public Rectangle CropLocation()
        {
            //coords[0] = x, coords[1] = y
            int localX = 0;
            int localy = 0;
            if (SpriteCoords[0] > 0)
                localX = SpriteCoords[0] * 35;
            if (SpriteCoords[1] > 0)
                localy = SpriteCoords[1] * 35;
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

        public FileStream CompoundedMessage()
        {
            if (File.Exists($"data/Sprite_Resources/{SpriteName}temp.png"))
                File.Delete($"data/Sprite_Resources/{SpriteName}temp.png");

            using var fs = new FileStream($"data/Sprite_Resources/PlayerSheet.png", FileMode.Open, FileAccess.Read);
            using MemoryStream outStream = new MemoryStream();
            using var image = Image.Load(fs);
            using (var fstemp = new FileStream($"data/Sprite_Resources/{SpriteName}temp.png", FileMode.CreateNew, FileAccess.ReadWrite))
            {
                var pngEncoder = new PngEncoder();
                var clone = image.Clone(img => img
                .Crop(CropLocation()));
                clone.Save(outStream, pngEncoder);
                Console.WriteLine("Cropped Image");
                outStream.Position = 0;
                outStream.CopyTo(fstemp);
                Console.WriteLine("Generated Image");
                fstemp.Close();
                var fstemp2 = new FileStream($"data/Sprite_Resources/{SpriteName}temp.png", FileMode.Open, FileAccess.Read);
                Console.WriteLine($"Read {SpriteName}");
                return fstemp2;
            }
        }

        public void WriteToSrpiteSheet()
        {
            using var image = Image.Load<Rgba32>($"data/Sprite_Resources/PlayerSheet.png");
            using var fs = new FileStream($"data/Sprite_Resources/PlayerSheet.png", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            using Image<Rgba32> clone = image.Clone<Rgba32>(i => i.Crop(CropLocation()));
            {
                var pngEncoder = new PngEncoder();
                image.Mutate(o => o.DrawImage(clone, new Point(SpriteCoords[0] * 35, SpriteCoords[1] * 35), 1f));
                image.Save(fs, pngEncoder);
                fs.Close();
            }
        }

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
    }
}
