using Microsoft.Extensions.Logging;
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
        private readonly ILogger logger;

        public RoomProcessor(
            GameCommandHandler commandHandler,
            GameEventReducer eventReducer,
            RoomConfig roomConfig,
            ILogger logger)
        {
            this.commandHandler = commandHandler;
            this.eventReducer = eventReducer;
            this.roomConfig = roomConfig;
            this.logger = logger;
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
                logger.LogError(
                    ex,
                    "Failed to process state {state} and commands {commands}",
                    SerializeForLog(state),
                    SerializeForLog(commands));

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
                var stateBefore = state.GetFullClone();
                foreach (var command in commandGroup)
                {
                    var events = commandHandler.Process(stateBefore, state, command).ToList();

                    logger.LogInformation("Processed command {command}", SerializeForLog(@command));

                    foreach (var @event in events)
                    {
                        eventReducer.Process(state, @event);
                        logger.LogInformation("Processed event {event}", SerializeForLog(@event));
                    }

                    allEvents.AddRange(events);
                }
            }

            return allEvents;
        }

        private static string SerializeForLog(object value)
        {
            return JsonConvert.SerializeObject(
                value,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                });
        }
    }
}
