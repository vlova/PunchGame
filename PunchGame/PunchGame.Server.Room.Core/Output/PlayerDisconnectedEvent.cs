﻿using System;

namespace PunchGame.Server.Room.Core.Output
{
    public class PlayerDisconnectedEvent : RoomBroadcastEvent
    {
        public Guid PlayerId { get; set; }
    }
}
