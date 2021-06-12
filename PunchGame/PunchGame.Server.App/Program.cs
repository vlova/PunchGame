using PunchGame.Server.CrossCutting;
using PunchGame.Server.Room.Core.Configs;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace PunchGame.Server.App
{
    class Program
    {
        async static Task Main(string[] args)
        {
            var roomConfig = GetRoomConfig();
            var roomProcessor = ServerModule.BuildRoomProcessor(roomConfig, new PlayerIdGenerator(), new RandomProvider());
            var gameServer = new GameServer(roomProcessor, roomConfig);
            var server = new TcpGameServer(IPAddress.Loopback, 6000, gameServer);
            await server.Start();
        }

        private static RoomConfig GetRoomConfig()
        {
            return new RoomConfig
            {
                Player = new PlayerConfig
                {
                    InitialLifeAmount = 100,
                    Punch = new PunchConfig
                    {
                        Damage = 5,
                        CriticalChance = 0.5m,
                        CriticalDamage = 50,
                        MinimalTimeDiff = TimeSpan.FromSeconds(1)
                    }
                },
                TimeQuant = TimeSpan.FromSeconds(0.1),
                ClientVersion = 1,
                MaxPlayers = 2
            };
        }
    }
}
