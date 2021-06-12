using PunchGame.Server.Room.Core.Models;
using PunchGame.Server.Room.Core.Output;

namespace PunchGame.Server.Room.Core.Logic.Connection
{
    public class PlayerDisconnectedEventReducer : IEventReducer<PlayerDisconnectedEvent>
    {
        public void Process(RoomState state, PlayerDisconnectedEvent @event)
        {
            state.PlayerIdToPlayerMap[@event.PlayerId].ConnectionId = null;
        }
    }
}
