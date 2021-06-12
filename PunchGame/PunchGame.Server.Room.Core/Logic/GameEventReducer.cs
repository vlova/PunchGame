using PunchGame.Server.Room.Core.Models;
using PunchGame.Server.Room.Core.Output;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PunchGame.Server.Room.Core.Logic
{
    public class GameEventReducer
    {
        private readonly Dictionary<Type, IEnumerable<IEventReducer>> reducersMap;

        public GameEventReducer(IEnumerable<IEventReducer> reducers)
        {
            this.reducersMap = HandlersToMap(reducers);
        }

        public void Process(RoomState state, GameEvent @event)
        {
            var eventType = @event.GetType();
            if (!this.reducersMap.ContainsKey(eventType))
            {
                return;
            }

            var reducers = this.reducersMap[eventType];
            foreach (dynamic reducer in reducers)
            {
                reducer.Process(state, (dynamic)@event);
            }
        }

        private Dictionary<Type, IEnumerable<IEventReducer>> HandlersToMap(IEnumerable<IEventReducer> reducers)
        {
            var eventTypes = typeof(GameEvent).Assembly
                .GetTypes()
                .Where(t => typeof(GameEvent).IsAssignableFrom(t))
                .Where(t => !t.IsAbstract)
                .ToList();

            var reducersMap = eventTypes
                .ToDictionary(
                    elementType => elementType,
                    commandType => reducers
                        .Where(v => typeof(IEventReducer<>).MakeGenericType(commandType).IsAssignableFrom(v.GetType()))
                        .ToList()
                        .AsEnumerable());

            return reducersMap;
        }
    }
}
