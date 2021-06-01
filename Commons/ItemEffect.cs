using System;
using System.Collections.Generic;
using System.Text;

namespace Yuzuri.Commons
{
    public enum ItemEffect
    {
        /// <summary>
        /// No effect
        /// </summary>
        None = 0,

        /// <summary>
        /// Player deals dmg
        /// </summary>
        OnDmg = 1,

        /// <summary>
        /// Player becomes unconscious
        /// </summary>
        OnKnockout = 2, 

        /// <summary>
        /// Player takes dmg
        /// </summary>
        OnDmged = 3,

        /// <summary>
        /// Player dies
        /// </summary>
        OnDeath = 4,
    }
}
