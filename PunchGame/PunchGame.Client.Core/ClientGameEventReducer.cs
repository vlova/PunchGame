using PunchGame.Server.Room.Core.Logic;
using PunchGame.Server.Room.Core.Models;
using PunchGame.Server.Room.Core.Output;
using System.Linq;

namespace PunchGame.Client.Core
{
    public class ClientGameEventReducer
    {
        private readonly GameEventReducer sharedReducer;

        public ClientGameEventReducer(GameEventReducer sharedReducer)
        {
            this.sharedReducer = sharedReducer;
        }
        public void Process(RoomState state, GameEvent @event)
        {
            if (@event is AttemptToJoinSuccessfulEvent joinEvent)
            {
                HandleSuccessJoin(state, joinEvent);
            }

            sharedReducer.Process(state, @event);
        }

        private void HandleSuccessJoin(RoomState state, AttemptToJoinSuccessfulEvent joinEvent)
        {
            state.GameState = joinEvent.RoomState.GameState;
            state.RoomId = joinEvent.RoomState.RoomId;

            var players = joinEvent.RoomState.PlayerIdToPlayerMap.Values;
            state.ConnectionIdToPlayerMap = players.Where(x => x.ConnectionId.HasValue).ToDictionary(x => x.ConnectionId.Value, x => x);
            state.PlayerIdToPlayerMap = players.ToDictionary(x => x.Id, x => x);
        }
    }
}
