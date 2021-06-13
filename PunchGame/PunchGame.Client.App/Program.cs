using PunchGame.Client.Core;
using PunchGame.Client.Network;
using PunchGame.Client.Ui;
using PunchGame.Server.CrossCutting;
using System.Threading.Tasks;

namespace PunchGame.Client.App
{
    class Program
    {
        async static Task Main(string[] args)
        {
            var gameSession = new GameSession(
                () => new TcpGameSession(new NetworkConfig { Hostname = "127.0.0.1", Port = 6000 }),
                new ClientGameEventReducer(SharedClientServerModule.BuildGameEventReducer()));

            var game = new Game(gameSession, new GameUi(new GameUiEventRenderer()));

            var gameTask = game.Run();

            await gameTask;
        }
    }
}
