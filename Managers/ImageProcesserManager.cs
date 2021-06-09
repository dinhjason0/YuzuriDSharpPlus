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

        public Rectangle CropLocation(int coordX, int coordY)
        {
            int localX = 0;
            int localy = 0;
            if (coordX > 0)
                localX = coordX * 35;
            if (coordY > 0)
                localy = coordY * 35;
            var CropSpace = new Rectangle(localX, localy, 35, 35);
            return CropSpace;
        }

        public List<int> SpriteDestination(string target)
        {
            using (StreamReader reader = new StreamReader($"data/Sprite_Resources/PlayerSheetAssistant.json"))
            using (JsonTextReader fileContent = new JsonTextReader(reader))
            {
                string content = reader.ReadToEnd();
                reader.Close();
                Console.WriteLine("The listed coordinates are as follows:\n" + content);
                var fullContent = (JObject)JsonConvert.DeserializeObject(content);
                Console.WriteLine("Summoned new JObject from Player Sheet Assistant\n" + fullContent);
                List<int> coordinates = new List<int>();
                coordinates.Add((int)fullContent[target][0]);
                coordinates.Add((int)fullContent[target][1]);
                Console.WriteLine($"\n\n{target} @ Coordinates: [{coordinates[0]},{coordinates[1]}]");
                return coordinates;
            }
        }

        //public Sprite OutfitStacker()
        //{
        //    return null;
        //}
    }


}
