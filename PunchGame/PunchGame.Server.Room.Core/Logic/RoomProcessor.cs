using PunchGame.Server.Room.Core.Configs;
using PunchGame.Server.Room.Core.Input;
using PunchGame.Server.Room.Core.Models;
using PunchGame.Server.Room.Core.Output;
using System;
using System.Collections.Generic;

namespace PunchGame.Server.Room.Core.Logic
{
    public class RoomProcessor
    {
        private readonly IRandomProvider randomProvider;
        private readonly RoomConfig roomConfig;

        public RoomProcessor(IRandomProvider randomProvider, RoomConfig roomConfig)
        {
            this.randomProvider = randomProvider ?? throw new ArgumentNullException(nameof(randomProvider));
            this.roomConfig = roomConfig ?? throw new ArgumentNullException(nameof(roomConfig));
        }

        public RoomState MakeInitialState()
        {
            return new RoomState
            {
                RoomId = Guid.NewGuid(),
                GameState = GameState.NotStarted,
                ConnectionIdToPlayerMap = new Dictionary<Guid, PlayerState>(),
                PlayerIdToPlayerMap = new Dictionary<Guid, PlayerState>()
            };
        }

        public (RoomState newState, IEnumerable<GameEvent> events) Process(RoomState roomState, IEnumerable<GameCommand> commands)
        {
            throw new NotImplementedException();
        }
    }
}
