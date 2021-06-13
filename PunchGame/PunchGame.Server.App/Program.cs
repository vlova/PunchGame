using Microsoft.Extensions.Configuration;
using PunchGame.Server.CrossCutting;
using System.Threading.Tasks;

namespace PunchGame.Server.App
{
    class Program
    {
        async static Task Main(string[] args)
        {
            var configBuilder = new ConfigurationBuilder()
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            var server = ServerModule.BuildTcpGameServer(configBuilder.Build());
            await server.Start();
        }
    }
}
