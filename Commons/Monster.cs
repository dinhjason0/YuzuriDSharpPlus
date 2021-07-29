using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yuzuri.Commons
{
    public class Monster
    {
        public string Name { get; set; }
        /// <summary>
        /// Max HP stat
        /// </summary>
        public int MaxHP { get; set; }
        /// <summary>
        /// Health Stat
        /// </summary>
        public int HP { get; set; }
        /// <summary>
        /// Strength Stat
        /// </summary>
        public int STR { get; set; }
        /// <summary>
        /// Dexterity Stat
        /// </summary>
        public int DEX { get; set; }
        /// <summary>
        /// Speed Stat
        /// </summary>
        public int SPD { get; set; }
        /// <summary>
        /// Magic Power Stat
        /// </summary>
        public int MPE { get; set; }
        /// <summary>
        /// Hit chance Stat
        /// </summary>
        public int HIT { get; set; }
        /// <summary>
        /// Damage roll stat
        /// </summary>
        public int DHL { get; set; }

        public List<string> Pattern { get; set; }

        public string ImgUrl { get; set; }

    }
}
