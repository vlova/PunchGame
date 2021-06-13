using Newtonsoft.Json;
using System;

namespace PunchGame.Server.Room.Core.Output
{
    public class PlayerJoinedEvent : RoomBroadcastEvent
    {
        public Guid PlayerId { get; set; }

        public string Name { get; set; }

        public int LifeAmount { get; set; }

        public Guid ConnectionId { get; set; }
    }
}
