
using System;

namespace PunchGame.Server.Room.Core.Configs
{
    public class RoomConfig
    {
        public PlayerConfig Player { get; set; }

        /// <summary>
        /// Time quant is minimal amount of time.
        /// If two events has distance greater than timequant, they are considered as same-time events
        /// This makes funny experience - people can kill each one
        /// </summary>
        public TimeSpan TimeQuant { get; set; }

        public int ClientVersion { get; set; }

        public int MaxPlayers { get; set; }
    }
}
