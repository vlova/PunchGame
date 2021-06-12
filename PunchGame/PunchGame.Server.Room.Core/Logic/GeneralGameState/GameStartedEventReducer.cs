using PunchGame.Server.Room.Core.Models;
using PunchGame.Server.Room.Core.Output;

namespace PunchGame.Server.Room.Core.Logic.GeneralGameState
{
    public class GameStartedEventReducer : IEventReducer<GameStartedEvent>
    {
        public void Process(RoomState state, GameStartedEvent @event)
        {
            state.GameState = GameState.InProgress;
        }
    }
}
