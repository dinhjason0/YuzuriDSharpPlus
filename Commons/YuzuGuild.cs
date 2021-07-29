using System;
using System.Collections.Generic;
using System.Text;

namespace Yuzuri.Commons
{
    public class YuzuGuild
    {
        public ulong GuildId { get; set; }
        public ulong RoomId { get; set; }
        public ulong FloorId { get; set; }
        public ulong RoleId { get; set; }
        public List<ulong> Resources { get; set; }

        public List<ulong> Floors { get; set; }

        public YuzuGuild(ulong guildId)
        {
            GuildId = guildId;
            Resources = new List<ulong>();
            Floors = new List<ulong>();
        }
    }
}
