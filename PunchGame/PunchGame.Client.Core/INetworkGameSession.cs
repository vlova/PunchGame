using PunchGame.Server.Room.Core.Input;
using PunchGame.Server.Room.Core.Output;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace PunchGame.Client.Core
{
    public interface INetworkGameSession
    {
        ConcurrentQueue<GameEvent> Events { get; }

        void ExecuteCommand(GameCommand command);
        Task Start();
        void Stop();
    }
}