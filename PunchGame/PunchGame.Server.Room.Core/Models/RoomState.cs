using System;
using System.Collections.Generic;

namespace PunchGame.Server.Room.Core.Models
{
    public class RoomState
    {
        public Guid RoomId { get; set; }

        public GameState GameState { get; set; }
            = GameState.NotStarted;

        public Dictionary<Guid, PlayerState> ConnectionIdToPlayerMap { get; set; }
            = new Dictionary<Guid, PlayerState>();

        public Dictionary<Guid, PlayerState> PlayerIdToPlayerMap { get; set; }
            = new Dictionary<Guid, PlayerState>();
    }
}
