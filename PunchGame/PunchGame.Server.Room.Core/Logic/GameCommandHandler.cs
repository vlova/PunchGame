using PunchGame.Server.Room.Core.Input;
using PunchGame.Server.Room.Core.Models;
using PunchGame.Server.Room.Core.Output;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PunchGame.Server.Room.Core.Logic
{
    public class GameCommandHandler
    {
        private readonly Dictionary<Type, ICommandHandler> handlerMap;

        public GameCommandHandler(IEnumerable<ICommandHandler> handlers)
        {
            this.handlerMap = HandlersToMap(handlers);
        }

        public IEnumerable<GameEvent> Process(RoomState state, GameCommand command)
        {
            var commandType = command.GetType();
            dynamic handler = handlerMap[commandType];
            var events = handler.Process(state, (dynamic)command);
            return events;
        }

        private Dictionary<Type, ICommandHandler> HandlersToMap(IEnumerable<ICommandHandler> validators)
        {
            var commandTypes = typeof(GameCommand).Assembly
                .GetTypes()
                .Where(t => typeof(GameCommand).IsAssignableFrom(t))
                .Where(t => !t.IsAbstract)
                .ToList();

            var commandsMap = commandTypes
                .ToDictionary(
                    elementType => elementType,
                    commandType => validators
                        .SingleOrDefault(v => typeof(ICommandHandler<>).MakeGenericType(commandType).IsAssignableFrom(v.GetType())));

            foreach (var kv in commandsMap)
            {
                if (kv.Value == null)
                {
                    throw new NotImplementedException($"Failed to find validator for type {kv.Key.ToString()}");
                }
            }

            return commandsMap;
        }
    }
}
