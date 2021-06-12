using System;

namespace PunchGame.Server.Room.Core.Output
{
    /// <summary>
    /// Event that should be sent to specific player
    /// </summary>
    public abstract class PersonalEvent : GameEvent
    {
        public Guid ConnectionId { get; set; }
    }
}
