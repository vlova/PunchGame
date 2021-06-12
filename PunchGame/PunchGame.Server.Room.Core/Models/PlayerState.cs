using System;

namespace PunchGame.Server.Room.Core.Models
{
    public class PlayerState
    {
        public Guid UserId { get; set; }

        public Guid? ConnectionId { get; set; }

        public string Name { get; set; }

        public int Life { get; set; }

        public DateTime LastPunch { get; set; }

        public bool IsConnected => ConnectionId != null;

        public bool IsAlive => Life > 0;
    }
}
