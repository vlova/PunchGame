using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PunchGame.Server.Room.Core.Configs;
using PunchGame.Server.Room.Core.Input;
using PunchGame.Server.Room.Core.Logic;
using PunchGame.Server.Room.Core.Models;
using PunchGame.Server.Room.Core.Output;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PunchGame.Server.App
{
    public class RoomServer
    {
        private readonly RoomProcessor roomProcessor;
        private readonly GameServer gameServer;
        private readonly RoomConfig roomConfig;

        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        private readonly ConcurrentQueue<GameCommand> commandsQueue
            = new ConcurrentQueue<GameCommand>();

        private readonly ConcurrentDictionary<Guid, GameClient> connectionIdToClientMap
            = new ConcurrentDictionary<Guid, GameClient>();

        private RoomState state;

        public Guid RoomId { get; private set; } = Guid.NewGuid();

        public IEnumerable<GameClient> Clients => this.connectionIdToClientMap.Values;

        public RoomServer(RoomProcessor roomProcessor, GameServer gameServer, RoomConfig roomConfig)
        {
            this.roomProcessor = roomProcessor;
            this.gameServer = gameServer;
            this.roomConfig = roomConfig;
        }

        public async Task Start()
        {
            if (this.state != null)
            {
                throw new InvalidOperationException("Can't call Start() on same roomServer twice");
            }

            this.state = roomProcessor.MakeInitialState(this.RoomId);
            while (!cts.Token.IsCancellationRequested)
            {
                await Task.Delay(1);
                var tickCommands = GetCommandsToExecute();

                if (!tickCommands.Any())
                {
                    continue;
                }


                // TODO: remove console
                Console.WriteLine(JsonConvert.SerializeObject(new { tickCommands }));
                var events = this.roomProcessor.Process(this.state, tickCommands);

                // TODO: remove console
                Console.WriteLine(JsonConvert.SerializeObject(new { events }));
                foreach (var @event in events)
                {
                    ProcessEvent(@event);
                }
            }
        }

        public void Stop()
        {
            this.cts.Cancel();
        }

        public void ProcessClientInput(GameClient client, JObject input)
        {
            this.commandsQueue.Enqueue(MapToGameCommand(client, input));
        }

        private static GameCommand MapToGameCommand(GameClient client, JObject input)
        {
            var commandType = input["commandType"].Value<string>();
            // TODO: consider caching
            var csharpType = typeof(ConnectToRoomCommand).Assembly.ExportedTypes
                .SingleOrDefault(x => x.Name == commandType);
            var commandData = input["data"].ToString();
            var command = JsonConvert.DeserializeObject(commandData, csharpType) as GameCommand;
            command.ByConnectionId = client.ConnectionId;
            command.Timestamp = DateTime.UtcNow;
            return command;
        }

        public void RegisterClient(GameClient client)
        {
            this.connectionIdToClientMap.TryAdd(client.ConnectionId, client);
        }

        public void UnregisterClient(GameClient client)
        {
            this.commandsQueue.Enqueue(new DisconnectCommand { ByConnectionId = client.ConnectionId, Timestamp = DateTime.UtcNow });
            this.connectionIdToClientMap.TryRemove(client.ConnectionId, out var _);
        }

        private List<GameCommand> GetCommandsToExecute()
        {
            var tickCommands = new List<GameCommand>();
            var currentTick = (DateTime.UtcNow.Ticks / roomConfig.TimeQuant.Ticks);
            while (commandsQueue.TryPeek(out var command))
            {
                var commandTick = command.Timestamp.Ticks / roomConfig.TimeQuant.Ticks;
                var isTickFinished = currentTick > commandTick;
                if (!isTickFinished)
                {
                    break;
                }

                tickCommands.Add(command);
                commandsQueue.TryDequeue(out var _);
            }

            return tickCommands;
        }

        private void ProcessEvent(GameEvent @event)
        {
            if (@event is InternalEvent)
            {
                this.gameServer.ProcessEvent(@event);
            }

            if (@event is RoomBroadcastEvent)
            {
                foreach (var client in this.connectionIdToClientMap.Values)
                {
                    client.Outputs.Enqueue(SerializeEvent(@event));
                }
            }

            if (@event is PersonalEvent personalEvent)
            {
                if (this.connectionIdToClientMap.TryGetValue(personalEvent.ConnectionId, out var client))
                {
                    client.Outputs.Enqueue(SerializeEvent(@event));
                }
            }
        }

        private JObject SerializeEvent(GameEvent @event)
        {
            return JObject.FromObject(new
            {
                eventType = @event.GetType().Name,
                data = @event
            });
        }
    }
}
