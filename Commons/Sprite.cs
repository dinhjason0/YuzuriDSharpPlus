using System;
using System.Collections.Generic;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Advanced;

namespace Yuzuri.Commons
{
    class Sprite
    {
        public string SpritePath { get; set; }
        public Rectangle SpaceCrop { get; set; }
        public int CoordX { get; set; }
        public int CoordY { get; set; }
        public Image CurrentSprite { get; set; }

        public Sprite(ulong userID, int coordX, int coordY)
        {
            SpritePath = "data\\Sprite_Resources\\PlayerSheet.png";
            //Hardcoded for now
            SpaceCrop = new Rectangle(35, 35, 35, 35);
            //Hardcoded :KEKW:
            CoordX = coordX;
            CoordY = coordY;
        }

        //Make a sprite differentiator between 135x135 boss sprites and 35x35 player & equipment sprites

        public void SetSpritePath(ulong userID)
        {
            
        }

        public void SetCurrentSprite (string spriteSheetPath)
        {
            //Bitmap source = Image.FromFile(spriteSheetPath) as Bitmap;
            //CurrentSprite = source.Clone(SpaceCrop, source.PixelFormat)
            //Well fuck me then. :shrug:
        }
    }
}
