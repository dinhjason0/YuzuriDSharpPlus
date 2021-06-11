using System.Collections.Generic;
using Yuzuri.Managers;

namespace Yuzuri.Commons
{
    public class Sprite
    {
        public string SpriteName { get; set; }
        public List<int> SpriteCoords { get; set; }

        public Sprite(string name)
        {
            SpriteName = name;
            ImageProcesserManager imageProcesser = new ImageProcesserManager();
            SpriteCoords = imageProcesser.SpriteDestination(name);
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
    }
}
