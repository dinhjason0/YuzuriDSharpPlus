using System;
using System.Collections.Generic;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Advanced;
using Yuzuri.Managers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Linq;

namespace Yuzuri.Commons
{
    //This entire management is to find png, recieve by calling SpriteSheet
    class Sprite
    {
        public Rectangle SpaceCrop { get; set; }
        public string SpriteName { get; set; }
        public int[] Coordinate { get; set; }

        public Sprite()
        { }

        public Sprite(string spriteName)
        {
            SpriteName = spriteName;
            Coordinate = JsonDecoder(SpriteName);
        }

        public Sprite(string spriteName, Rectangle rectangle, int[] coordinate)
        {
            SpriteName = spriteName;
            Coordinate = coordinate;
            SpaceCrop = rectangle;
        }
        //Make a sprite differentiator between 135x135 boss sprites and 35x35 player & equipment sprites

        //public string GetSpriteName()
        //{

        //}

        //public Rectangle GetRectangle()
        //{

        //    ImageProcesserManager.CropLocation();
        //}



        public void SetCurrentSprite(string spriteSheetPath)
        {
            //Bitmap source = Image.FromFile(spriteSheetPath) as Bitmap;
            //CurrentSprite = source.Clone(SpaceCrop, source.PixelFormat)
            //Well fuck me then. :shrug:
        }


        //Finds the sprite's coordinates in .json
        public int[] JsonDecoder(string target)
        {
            using (System.IO.StreamReader reader = new System.IO.StreamReader($"data/Sprite_Resources/PlayerSheetAssistant.json"))
            using (JsonTextReader fileContent = new JsonTextReader(reader))
            {
                JObject o2 = (JObject)JToken.ReadFrom(fileContent);
                Console.WriteLine("Found Json File Content");

                string stringedJObject = JsonConvert.ToString(o2);
                Console.WriteLine("Stringed Object\n" + stringedJObject);

                var dataJObject = (JObject)JsonConvert.DeserializeObject(stringedJObject);
                Console.WriteLine(dataJObject);
                //dynamic results = JsonConvert.DeserializeObject<dynamic>(stringedJObject);
                JArray jsonObject = JArray.Parse(stringedJObject);


                int[] sendingCoordinates = jsonObject[0][target].Values<int>().ToArray();
                return sendingCoordinates;

                //catch
                //{
                //    if (target.Length == 18)
                //    {
                //        return null;
                //    }
                //    return null;
                //}
            }
        }
    }
}
