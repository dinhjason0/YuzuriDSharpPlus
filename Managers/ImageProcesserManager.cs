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

        //public Sprite OutfitStacker()
        //{
        //    return null;
        //}
    }


}
