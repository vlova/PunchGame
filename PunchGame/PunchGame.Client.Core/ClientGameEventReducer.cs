using PunchGame.Server.Room.Core.Logic;
using PunchGame.Server.Room.Core.Models;
using PunchGame.Server.Room.Core.Output;

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
            // TODO: this should init state
        }
    }
}
