using System;

namespace PunchGame.Server.Room.Core.Input
{
    public abstract class GameCommand
    {
        public DateTime TimeStamp { get; set; }

        public Guid ByConnectionId { get; set; }
    }
}
