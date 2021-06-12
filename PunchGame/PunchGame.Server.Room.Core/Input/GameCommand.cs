using System;

namespace PunchGame.Server.Room.Core.Input
{
    // TODO: maybe interface
    public abstract class GameCommand
    {
        public DateTime Timestamp { get; set; }

        public Guid ByConnectionId { get; set; }
    }
}
