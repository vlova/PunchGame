using PunchGame.Server.Room.Core.Configs;
using PunchGame.Server.Room.Core.Input;
using PunchGame.Server.Room.Core.Models;
using PunchGame.Server.Room.Core.Output;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PunchGame.Server.Room.Core.Logic.Connection
{
    public class ConnectToRoomCommandHandler : ICommandHandler<ConnectToRoomCommand>
    {
        private readonly IPlayerIdGenerator playerIdGenerator;
        private readonly RoomConfig config;

        public ConnectToRoomCommandHandler(IPlayerIdGenerator playerIdGenerator, RoomConfig config)
        {
            this.playerIdGenerator = playerIdGenerator;
            this.config = config;
        }

        public IEnumerable<GameEvent> Process(RoomState state, ConnectToRoomCommand command)
        {
            if (command.ClientVersion != config.ClientVersion)
            {
                yield return new AttemptToJoinRejectedEvent
                {
                    ConnectionId = command.ByConnectionId,
                    Reason = AttemptToJoinRejectedEvent.RejectReason.VersionMismatch,
                    Timestamp = command.Timestamp
                };

                yield break;
            }

            var isNameGood = IsNameValid(command);
            if (!isNameGood)
            {
                yield return new AttemptToJoinRejectedEvent
                {
                    ConnectionId = command.ByConnectionId,
                    Reason = AttemptToJoinRejectedEvent.RejectReason.NameNotValid,
                    Timestamp = command.Timestamp
                };

                yield break;
            }

            var isNameUnique = IsNameUnique(state, command);
            if (!isNameUnique)
            {
                yield return new AttemptToJoinRejectedEvent
                {
                    ConnectionId = command.ByConnectionId,
                    Reason = AttemptToJoinRejectedEvent.RejectReason.NameNotUnique,
                    Timestamp = command.Timestamp
                };

                yield break;
            }

            if (state.PlayerIdToPlayerMap.Count >= config.MaxPlayers)
            {
                yield return new AttemptToJoinRejectedEvent
                {
                    ConnectionId = command.ByConnectionId,
                    Reason = AttemptToJoinRejectedEvent.RejectReason.RoomIsFilled,
                    Timestamp = command.Timestamp
                };

                yield break;
            }

            if (state.GameState == GameState.Completed)
            {
                yield return new AttemptToJoinRejectedEvent
                {
                    ConnectionId = command.ByConnectionId,
                    Reason = AttemptToJoinRejectedEvent.RejectReason.GameCompleted,
                    Timestamp = command.Timestamp
                };

                yield break;
            }

            var playerId = playerIdGenerator.NextPlayerId(state);

            yield return new AttemptToJoinSuccessfulEvent
            {
                ConnectionId = command.ByConnectionId,
                JoinedAsPlayerId = playerId,
                Timestamp = command.Timestamp,
                Players = state.PlayerIdToPlayerMap.Values.Select(Map).ToList()
            };

            yield return new PlayerJoinedEvent
            {
                Timestamp = command.Timestamp,
                ConnectionId = command.ByConnectionId,
                PlayerId = playerId,
                LifeAmount = config.Player.InitialLifeAmount,
                Name = command.Name
            };

            if (state.PlayerIdToPlayerMap.Count == 1)
            {
                yield return new GameStartedEvent
                {
                    Timestamp = command.Timestamp
                };
            }

            if ((state.PlayerIdToPlayerMap.Count + 1) == config.MaxPlayers)
            {
                yield return new RoomFilledEvent
                {
                    RoomId = state.RoomId,
                    Timestamp = command.Timestamp
                };
            }
        }

        private bool IsNameValid(ConnectToRoomCommand command)
        {
            return 3 <= command.Name.Length
                && command.Name.Length <= 8
                && command.Name.All(c => char.IsDigit(c) || IsLatinChar(c));
        }

        private bool IsLatinChar(char c)
        {
            return ('a' < c && c < 'z')
                || ('A' < c && c < 'Z');
        }

        private bool IsNameUnique(RoomState state, ConnectToRoomCommand command)
        {
            // TODO(perf) this can be optimized
            return !state.PlayerIdToPlayerMap.Values
                .Select(x => x.Name)
                .Any(name => string.Equals(name, command.Name, StringComparison.InvariantCulture));
        }

        private AttemptToJoinSuccessfulEvent.ShortPlayerInfo Map(PlayerState arg)
        {
            return new AttemptToJoinSuccessfulEvent.ShortPlayerInfo
            {
                IsConnected = arg.IsConnected,
                Life = arg.Life,
                Name = arg.Name,
                PlayerId = arg.Id
            };
        }
    }
}
