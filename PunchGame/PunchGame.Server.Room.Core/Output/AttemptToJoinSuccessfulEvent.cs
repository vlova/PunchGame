using System;
using System.Collections.Generic;

namespace PunchGame.Server.Room.Core.Output
{
    public class AttemptToJoinSuccessfulEvent : PersonalEvent
    {
        public Guid JoinedAsPlayerId { get; set; }

        public List<ShortPlayerInfo> Players { get; set; }

        public class ShortPlayerInfo
        {
            public Guid UserId { get; set; }

            public string Name { get; set; }

            public int Life { get; set; }

            public bool IsConnected { get; set; }
        }
    }
}
