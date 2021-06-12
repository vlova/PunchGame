using PunchGame.Server.Room.Core.Models;
using PunchGame.Server.Room.Core.Output;

namespace PunchGame.Server.Room.Core.Logic.Game
{
    public class PunchEventReducer : IEventReducer<PunchEvent>
    {
        public void Process(RoomState state, PunchEvent @event)
        {
            state.PlayerIdToPlayerMap[@event.VictimId].Life -= @event.Damage;
            state.PlayerIdToPlayerMap[@event.KillerId].LastPunch = @event.Timestamp;
        }
    }
}
