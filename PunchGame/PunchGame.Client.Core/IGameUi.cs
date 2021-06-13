using PunchGame.Server.Room.Core.Models;
using PunchGame.Server.Room.Core.Output;

namespace PunchGame.Client.Core
{
    public interface IGameUi
    {
        void Render(RoomState state, GameEvent newEvent);

        void Run();

        void Stop();
    }
}