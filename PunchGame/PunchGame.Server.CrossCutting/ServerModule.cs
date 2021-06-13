using Microsoft.Extensions.Logging;
using PunchGame.Server.App;
using PunchGame.Server.Room.Core.Configs;
using PunchGame.Server.Room.Core.Logic;
using PunchGame.Server.Room.Core.Logic.Connection;
using PunchGame.Server.Room.Core.Logic.Game;
using System;
using System.Collections.Generic;
using System.Net;

namespace PunchGame.Server.CrossCutting
{
    // TODO: should be via DI-container
    public class ServerModule
    {
        public static RoomProcessor BuildRoomProcessor(
            RoomConfig config,
            IPlayerIdGenerator playerIdGenerator,
            IRandomProvider randomProvider)
        {
            var gameCommandHandler = new GameCommandHandler(new List<ICommandHandler>
            {
                new ConnectToRoomCommandHandler(playerIdGenerator, config) ,
                new DisconnectCommandHandler(),
                new PunchCommandHandler(config, randomProvider),
            });

            var gameEventReducer = SharedClientServerModule.BuildGameEventReducer();

            return new RoomProcessor(gameCommandHandler, gameEventReducer, config, MakeLogger<RoomProcessor>());
        }

        public static TcpGameServer BuildTcpGameServer()
        {
            var roomConfig = GetRoomConfig();
            var roomProcessor = BuildRoomProcessor(roomConfig, new PlayerIdGenerator(), new RandomProvider());
            var gameServer = new GameServer(roomProcessor, roomConfig, MakeLogger<GameServer>());
            var server = new TcpGameServer(IPAddress.Loopback, 6000, gameServer, MakeLogger<TcpGameServer>());
            return server;
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

        private static ILogger MakeLogger<T>()
        {
            return LoggerFactory
                .Create(builder => builder.AddConsole())
                .CreateLogger<T>();
        }
    }
}
