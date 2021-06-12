using PunchGame.Server.Room.Core.Input;
using PunchGame.Server.Room.Core.Models;
using PunchGame.Server.Room.Core.Output;
using System.Collections.Generic;

namespace PunchGame.Server.Room.Core.Logic.Connection
{
    public class DisconnectCommandHandler : ICommandHandler<DisconnectCommand>
    {
        public IEnumerable<GameEvent> Process(RoomState state, DisconnectCommand command)
        {
            yield return new PlayerDisconnectedEvent
            {
                PlayerId = state.ConnectionIdToPlayerMap[command.ByConnectionId].Id
            };
        }
    }
}
