using PunchGame.Server.Room.Core.Models;
using PunchGame.Server.Room.Core.Output;
using System.Collections.Generic;

namespace PunchGame.Server.Room.Core.Logic
{
    public interface ICommandHandler
    {
    }

    public interface ICommandHandler<T> : ICommandHandler
    {
        IEnumerable<GameEvent> Process(RoomState state, T command);
    }
}
