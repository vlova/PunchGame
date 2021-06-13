using PunchGame.Server.CrossCutting;
using System.Threading.Tasks;

namespace PunchGame.Server.App
{
    class Program
    {
        async static Task Main(string[] args)
        {
            var server = ServerModule.BuildTcpGameServer();
            await server.Start();
        }
    }
}
