using System;

namespace PunchGame.Server.Room.Core.Input
{
    public class PunchCommand : GameCommand
    {
        public Guid VictimId { get; set; }
    }
}
