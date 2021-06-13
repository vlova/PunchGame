﻿using System;
using System.Collections.Generic;

namespace PunchGame.Server.Room.Core.Output
{
    public class AttemptToJoinSuccessfulEvent : PersonalEvent
    {
        public Guid JoinedAsPlayerId { get; set; }

        // TODO: this should expose actual roomstate
        public List<ShortPlayerInfo> Players { get; set; }

        public class ShortPlayerInfo
        {
            public Guid PlayerId { get; set; }

            public string Name { get; set; }

            public int Life { get; set; }

            public bool IsConnected { get; set; }
        }
    }
}
