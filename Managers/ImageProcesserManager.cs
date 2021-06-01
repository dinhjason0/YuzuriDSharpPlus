using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Yuzuri.Commons;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.IO;


namespace Yuzuri.Managers
{
    //The coordinate for the Head is 1x1
    //The coordinate for the Torso is 2x1
    public class ImageProcesserManager
    {
        //Dimensions for Sprite Sheet tiles

        //This class isn't exclusively "outfits", it includes stacking the torso
        public Image PlayerSpriteSheet()
        {
            Image baseSpriteSheet;
            try
            {
                baseSpriteSheet = Image.FromFile("data\\Sprite_Resources\\PlayerSheet.png");
                return baseSpriteSheet;
            }
            catch (Exception)
            {
                Console.WriteLine("\"Player Sprite Sheet\" not found!");
            }
            return null;
        }

        

        //public Sprite OutfitStacker()
        //{
        //    return null;
        //}
    }
}
