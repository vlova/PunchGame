using System;

namespace PunchGame.Server.Room.Core.Output
{
    public class PlayerDiedEvent : RoomBroadcastEvent
    {
        public Guid KillerId { get; set; }

        public Guid VictimId { get; set; }
    }
}
