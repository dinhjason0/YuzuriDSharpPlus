using System;
using System.Collections.Generic;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Memory;
using Yuzuri.Commons;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Text.RegularExpressions;

namespace Yuzuri.Managers
{
    //The coordinate for the Head is 1x1
    //The coordinate for the Torso is 2x1
    public class ImageProcesserManager
    {
        //Dimensions for Sprite Sheet tiles

        //This class isn't exclusively "outfits", it includes stacking the torso
        public Image PlayerSpriteSheet(FileStream fs)
        {
            try
            {
                using var image = Image.Load(fs);
                return image;
            }
            catch (Exception)
            {
                Console.WriteLine("ImageProcesserManager unable to load FileStream");
            }
            return null;
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

                    if (File.Exists($"data/Sprite_Resources/PlayerSheet2.png"))
                    {
                        Console.WriteLine("Found PlayerSheet2");
                        fstemp.Close();
                        var fstemp2 = new FileStream($"data/Sprite_Resources/{target}temp.png", FileMode.Open, FileAccess.Read);
                        Console.WriteLine("Read PlayerSheet2");

                        return fstemp2;
                    }
                }
                return null;
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
                Console.WriteLine($"\n\n{target} @ Coordinates: [{coordinates[0]},{coordinates[1]}]");
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
                    if (contentListing[i] == "{\r" || contentListing[i] == "}")
                    {}
                    else
                    value.Add(SpriteDestination(contentListing[i]));
                }
                return value;
            }
        }
    }

    public class SpriteCoordinate
    {
        public string SpriteName { get; set; }
        public List<int> SpriteCoords { get; set; }
        //public int AxisX { get; set; }
        //public int AxisY { get; set; }

        //public SpriteCoordinate()
        //{
        //    SpriteName = null;
        //    AxisX = 0;
        //    AxisY = 0;
        //}

        //public SpriteCoordinate(int publicX, int publicY)
        //{
        //    SpriteName = null;
        //    AxisX = publicX;
        //    AxisY = publicY;
        //}

        //public SpriteCoordinate(string spriteName, int publicX, int publicY)
        //{
        //    SpriteName = spriteName;
        //    AxisX = publicX;
        //    AxisY = publicY;
        //}
    }
}
