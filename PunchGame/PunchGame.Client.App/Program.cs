using Microsoft.Extensions.Configuration;
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
            var configBuilder = new ConfigurationBuilder()
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            var config = configBuilder.Build();

            var networkConfig = config.GetSection(nameof(ClientNetworkConfig)).Get<ClientNetworkConfig>();

            var gameSession = new GameSession(
                () => new TcpGameSession(networkConfig),
                new ClientGameEventReducer(SharedClientServerModule.BuildGameEventReducer()));

            var game = new Game(gameSession, new GameUi(new GameUiEventRenderer()), new GameController());

            var gameTask = game.Run();

            await gameTask;
        }
    }
}
