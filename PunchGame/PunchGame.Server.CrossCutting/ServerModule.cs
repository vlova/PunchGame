using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PunchGame.Server.App;
using PunchGame.Server.Room.Core.Configs;
using PunchGame.Server.Room.Core.Logic;
using PunchGame.Server.Room.Core.Logic.Connection;
using PunchGame.Server.Room.Core.Logic.Game;
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

        public static TcpGameServer BuildTcpGameServer(IConfigurationRoot configuration)
        {
            var roomConfig = configuration.GetSection(nameof(RoomConfig)).Get<RoomConfig>();
            var networkConfig = configuration.GetSection(nameof(ServerNetworkConfig)).Get<ServerNetworkConfig>();
            var roomProcessor = BuildRoomProcessor(roomConfig, new PlayerIdGenerator(), new RandomProvider());
            var gameServer = new GameServer(roomProcessor, roomConfig, MakeLogger<GameServer>());
            var server = new TcpGameServer(IPAddress.Parse(networkConfig.Ip), networkConfig.Port, gameServer, MakeLogger<TcpGameServer>());
            return server;
        }

        private static ILogger MakeLogger<T>()
        {
            return LoggerFactory
                .Create(builder => builder.AddConsole())
                .CreateLogger<T>();
        }
    }
}
