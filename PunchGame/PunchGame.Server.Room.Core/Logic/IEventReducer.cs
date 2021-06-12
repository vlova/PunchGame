using PunchGame.Server.Room.Core.Models;

namespace PunchGame.Server.Room.Core.Logic
{
    public interface IEventReducer
    {
    }

    public interface IEventReducer<T> : IEventReducer
    {
        void Process(RoomState state, T @event);
    }
}
