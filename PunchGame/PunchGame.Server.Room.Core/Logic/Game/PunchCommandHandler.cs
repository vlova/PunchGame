using PunchGame.Server.Room.Core.Configs;
using PunchGame.Server.Room.Core.Input;
using PunchGame.Server.Room.Core.Models;
using PunchGame.Server.Room.Core.Output;
using System.Collections.Generic;
using System.Linq;

namespace PunchGame.Server.Room.Core.Logic.Game
{
    public class PunchCommandHandler : ICommandHandler<PunchCommand>
    {
        private readonly RoomConfig roomConfig;
        private readonly IRandomProvider randomProvider;

        public PunchCommandHandler(RoomConfig roomConfig, IRandomProvider randomProvider)
        {
            this.roomConfig = roomConfig;
            this.randomProvider = randomProvider;
        }

        public IEnumerable<GameEvent> Process(RoomState state, PunchCommand command)
        {
            var killer = state.ConnectionIdToPlayerMap[command.ByConnectionId];
            var punch = roomConfig.Player.Punch;
            var canPunch
                = killer.LastPunch == null
                || ((command.Timestamp - killer.LastPunch) > punch.MinimalTimeDiff);

            if (!canPunch)
            {
                yield break;
            }

            var isCritical = randomProvider.GetNextChance() > punch.CriticalChance;
            var damage = isCritical ? punch.CriticalDamage : punch.Damage;

            yield return new PunchEvent
            {
                Damage = damage,
                KillerId = killer.Id,
                Timestamp = command.Timestamp,
                VictimId = command.VictimId
            };

            var victim = state.PlayerIdToPlayerMap[command.VictimId];
            var willBeAlive = victim.Life > damage;
            if (willBeAlive)
            {
                yield break;
            }

            yield return new PlayerDiedEvent
            {
                KillerId = killer.Id,
                VictimId = command.VictimId,
                Timestamp = command.Timestamp
            };

            var hasOtherAlivePlayers = state.PlayerIdToPlayerMap.Values
                .Where(p => p.Id != killer.Id && p.Id != victim.Id)
                .Any(p => p.IsAlive);

            if (hasOtherAlivePlayers)
            {
                yield break;
            }

            yield return new GameEndedEvent
            {
                Timestamp = command.Timestamp,
                WinnerId = killer.Id,
            };

            yield return new RoomDestroyedEvent
            {
                Timestamp = command.Timestamp,
                RoomId = state.RoomId
            };
        }
    }
}
