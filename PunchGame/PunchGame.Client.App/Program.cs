using PunchGame.Server.Room.Core.Input;
using System;
using System.Threading.Tasks;

namespace PunchGame.Client.App
{
    class Program
    {
        async static Task Main(string[] args)
        {
            var gameSession = new TcpGameSession(new NetworkConfig { Hostname = "127.0.0.1", Port = 6000 });
            var sessionTask = gameSession.Start();
            gameSession.ExecuteCommand(new ConnectToRoomCommand { ClientVersion = 1, Name = "olo" });
            gameSession.ExecuteCommand(new PunchCommand { VictimId = Guid.NewGuid() });

            await sessionTask;
        }
    }
}
