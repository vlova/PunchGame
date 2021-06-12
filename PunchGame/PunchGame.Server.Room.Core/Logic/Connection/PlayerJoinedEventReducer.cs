using PunchGame.Server.Room.Core.Models;
using PunchGame.Server.Room.Core.Output;

namespace PunchGame.Server.Room.Core.Logic.Connection
{
    public class PlayerJoinedEventReducer : IEventReducer<PlayerJoinedEvent>
    {
        public void Process(RoomState state, PlayerJoinedEvent @event)
        {
            var playerState = new PlayerState
            {
                ConnectionId = @event.ConnectionId,
                Id = @event.PlayerId,
                LastPunch = null,
                Life = @event.LifeAmount,
                Name = @event.Name
            };

            state.ConnectionIdToPlayerMap.Add(@event.ConnectionId, playerState);
            state.PlayerIdToPlayerMap.Add(@event.PlayerId, playerState);
        }
    }
}
