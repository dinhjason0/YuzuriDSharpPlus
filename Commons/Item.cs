using System;
using System.Collections.Generic;
using System.Text;

namespace Yuzuri.Commons
{
    public class Item
    {
        public string Name { get; set; }
        public int HIT { get; set; }
        public int DEF { get; set; }

        public Item()
        { }

        public Item(String name, int hit, int def)
        {
            Name = name;
            HIT = hit;
            DEF = def;
        }

    }
}
