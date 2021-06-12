using PunchGame.Server.Room.Core.Models;
using System;

namespace PunchGame.Server.Room.Core.Logic
{
    public interface IPlayerIdGenerator
    {
        Guid NextPlayerId(RoomState state);
    }
}
