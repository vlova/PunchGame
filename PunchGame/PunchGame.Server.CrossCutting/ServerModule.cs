using PunchGame.Server.Room.Core.Configs;
using PunchGame.Server.Room.Core.Logic;
using PunchGame.Server.Room.Core.Logic.Connection;
using PunchGame.Server.Room.Core.Logic.Game;
using PunchGame.Server.Room.Core.Logic.GeneralGameState;
using System.Collections.Generic;

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

            var gameEventReducer = new GameEventReducer(new List<IEventReducer> {
                new PlayerDisconnectedEventReducer(),
                new PlayerJoinedEventReducer (),
                new PunchEventReducer (),
                new GameStartedEventReducer ()
            });

            return new RoomProcessor(gameCommandHandler, gameEventReducer, config);
        }
    }
}
