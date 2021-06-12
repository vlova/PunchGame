using PunchGame.Server.Room.Core.Logic;
using PunchGame.Server.Room.Core.Models;
using System;

namespace PunchGame.Server.App
{
    public class PlayerIdGenerator : IPlayerIdGenerator
    {
        public Guid NextPlayerId(RoomState state)
        {
            return Guid.NewGuid();
        }
    }
}
