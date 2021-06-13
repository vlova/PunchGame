using PunchGame.Server.Room.Core.Models;
using System;

namespace PunchGame.Server.Room.Core.Output
{
    public class AttemptToJoinSuccessfulEvent : PersonalEvent
    {
        public Guid JoinedAsPlayerId { get; set; }

        public RoomState RoomState { get; set; }
    }
}
