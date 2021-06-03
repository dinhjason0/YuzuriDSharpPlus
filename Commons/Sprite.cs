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



        public void SetCurrentSprite (string spriteSheetPath)
        {
            //Bitmap source = Image.FromFile(spriteSheetPath) as Bitmap;
            //CurrentSprite = source.Clone(SpaceCrop, source.PixelFormat)
            //Well fuck me then. :shrug:
        }
    }

    public class SpriteSheetDecoder
    {
        public string TargetItem { get; set; }
        public int[] Coordinates { get; set; }
    }
}
