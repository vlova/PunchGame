using System;

namespace PunchGame.Server.Room.Core.Output
{
    public class PunchEvent : RoomBroadcastEvent
    {
        public Guid KillerId { get; set; }

        public Guid VictimId { get; set; }

        public int Damage { get; set; }
    }
}
