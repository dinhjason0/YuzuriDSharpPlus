using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using System.IO;
using SixLabors.ImageSharp.PixelFormats;
using Yuzuri.Managers;

namespace Yuzuri.Managers
{
    //The coordinate for the Head is 1x1
    //The coordinate for the Torso is 2x1
    public class ImageProcesserManager
    {
        private JsonManager jsonManager = new JsonManager();

        public void ResizePlayerSheetAssistant(List<int> borrowCoords)
        {
            using Image<Rgba32> imageSets = Image.Load<Rgba32>("data/Sprite_Resources/PlayerSheet.png");
            using var fs = new FileStream($"data/Sprite_Resources/PlayerSheet.png", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            borrowCoords[0] = (borrowCoords[0] + 1) * 35;
            borrowCoords[1] = (borrowCoords[1] + 1) * 35;
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

