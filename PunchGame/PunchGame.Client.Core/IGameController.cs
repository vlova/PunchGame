using System.Threading;

namespace PunchGame.Client.Core
{
    public interface IGameController
    {
        void ReadInput(Game game, IGameUi ui, CancellationToken cancellationToken);

        string ReadUserName();
    }
}