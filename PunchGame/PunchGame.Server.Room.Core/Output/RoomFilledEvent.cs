using System;

namespace PunchGame.Server.Room.Core.Output
{
    public class RoomFilledEvent : InternalEvent
    {
        public Guid RoomId { get; set; }
    }
}
