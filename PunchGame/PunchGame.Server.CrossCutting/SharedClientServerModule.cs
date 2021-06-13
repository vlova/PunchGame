using PunchGame.Server.Room.Core.Logic;
using PunchGame.Server.Room.Core.Logic.Connection;
using PunchGame.Server.Room.Core.Logic.Game;
using PunchGame.Server.Room.Core.Logic.GeneralGameState;
using System.Collections.Generic;

namespace PunchGame.Server.CrossCutting
{
    public class SharedClientServerModule
    {
        public static GameEventReducer BuildGameEventReducer()
        {
            return new GameEventReducer(new List<IEventReducer> {
                new PlayerDisconnectedEventReducer(),
                new PlayerJoinedEventReducer (),
                new PunchEventReducer (),
                new GameStartedEventReducer ()
            });
        }
    }
}
