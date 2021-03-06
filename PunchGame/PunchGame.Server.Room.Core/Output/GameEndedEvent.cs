using System;

namespace PunchGame.Server.Room.Core.Output
{
    public class GameEndedEvent : RoomBroadcastEvent
    {
        public Guid? WinnerId { get; set; }

        public EventReason Reason { get; set; }

        public enum EventReason
        {
            Win,
            Crash
        }
    }
}
