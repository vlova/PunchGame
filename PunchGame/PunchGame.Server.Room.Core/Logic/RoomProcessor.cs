using Newtonsoft.Json;
using PunchGame.Server.Room.Core.Configs;
using PunchGame.Server.Room.Core.Input;
using PunchGame.Server.Room.Core.Models;
using PunchGame.Server.Room.Core.Output;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PunchGame.Server.Room.Core.Logic
{
    public class RoomProcessor
    {
        private readonly GameCommandHandler commandHandler;
        private readonly GameEventReducer eventReducer;
        private readonly RoomConfig roomConfig;

        public RoomProcessor(
            GameCommandHandler commandHandler,
            GameEventReducer eventReducer,
            RoomConfig roomConfig)
        {
            this.commandHandler = commandHandler;
            this.eventReducer = eventReducer;
            this.roomConfig = roomConfig;
        }

        public RoomState MakeInitialState(Guid roomId)
        {
            return new RoomState
            {
                RoomId = roomId,
                GameState = GameState.NotStarted,
                ConnectionIdToPlayerMap = new Dictionary<Guid, PlayerState>(),
                PlayerIdToPlayerMap = new Dictionary<Guid, PlayerState>()
            };
        }

        public IEnumerable<GameEvent> Process(RoomState state, IEnumerable<GameCommand> commands)
        {
            try
            {
                return ProcessInternal(state, commands);
            }
            catch (Exception ex)
            {
                // TODO: logging

                return new List<GameEvent>
                {
                    new GameEndedEvent() { Reason = GameEndedEvent.EventReason.Crash },
                    new RoomDestroyedEvent()
                };
            }
        }

        private IEnumerable<GameEvent> ProcessInternal(RoomState state, IEnumerable<GameCommand> commands)
        {
            var allEvents = new List<GameEvent>();

            var groupedCommands = commands.GroupBy(g => g.Timestamp.Ticks / roomConfig.TimeQuant.Ticks);

            foreach (var commandGroup in groupedCommands)
            {
                var groupEvents = new List<GameEvent>();
                foreach (var command in commandGroup)
                {
                    var events = commandHandler.Process(state, command).ToList();
                    groupEvents.AddRange(events);
                    allEvents.AddRange(events);
                }

                foreach (var @event in groupEvents)
                {
                    eventReducer.Process(state, @event);
                }
            }

            return allEvents;
        }
    }
}
