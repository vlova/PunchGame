using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using PunchGame.Server.Room.Core.Configs;
using PunchGame.Server.Room.Core.Input;
using PunchGame.Server.Room.Core.Logic;
using PunchGame.Server.Room.Core.Output;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace PunchGame.Server.App
{
    public class GameServer
    {
        private readonly RoomProcessor roomProcessor;
        private readonly RoomConfig roomConfig;
        private readonly ILogger logger;

        private object roomMakerLocker = new object();
        private ConcurrentDictionary<Guid, GameClient> Clients = new ConcurrentDictionary<Guid, GameClient>();
        private ConcurrentDictionary<Guid, RoomServer> FreeRooms = new ConcurrentDictionary<Guid, RoomServer>();
        private ConcurrentDictionary<Guid, RoomServer> UnavailableRooms = new ConcurrentDictionary<Guid, RoomServer>();

        public GameServer(RoomProcessor roomProcessor, RoomConfig roomConfig, ILogger logger)
        {
            this.roomProcessor = roomProcessor;
            this.roomConfig = roomConfig;
            this.logger = logger;
        }

        public async Task RunClient(GameClient client)
        {
            Clients.TryAdd(client.ConnectionId, client);
            AssociateWithFreeRoom(client);
            await RunRegisteredClient(client);
        }

        private async Task RunRegisteredClient(GameClient client)
        {
            while (!client.CancellationToken.IsCancellationRequested)
            {
                if (client.Inputs.TryDequeue(out var input))
                {
                    client.AssociatedRoom?.ProcessClientInput(client, input);
                    if (input["commandType"].ToString() == nameof(DisconnectCommand))
                    {
                        Clients.TryRemove(client.ConnectionId, out var _);
                        client.Dispose();
                    }
                }

                await Task.Delay(1);
            }
        }

        public void UnregisterClient(GameClient client)
        {
            client.AssociatedRoom?.UnregisterClient(client);
        }

        private void AssociateWithFreeRoom(GameClient client)
        {
            var room = GetOrMakeFreeRoom();
            room.RegisterClient(client);
            client.AssociatedRoom = room;
            logger.LogInformation($"Client {client.ConnectionId} associated with room ${room.RoomId}");
        }

        private RoomServer GetOrMakeFreeRoom()
        {
            // Please, note that it's highly possible that
            //   Free room will be not free after joining.
            //      and we don't care about this, because it's hard to solve. 
            //      client just will get reject when attempting to connect to room
            return GetFreeRoom() ?? MakeNewFreeRoom();
        }

        private RoomServer GetFreeRoom()
        {
            var rooms = FreeRooms.Values.ToList();
            if (rooms.Count != 0)
            {
                var randomRoomIndex = new Random().Next(0, rooms.Count);
                return rooms[randomRoomIndex];
            }

            return null;
        }

        private RoomServer MakeNewFreeRoom()
        {
            lock (roomMakerLocker)
            {
                return MakeNewFreeRoomInternal();
            }
        }

        private RoomServer MakeNewFreeRoomInternal()
        {
            var room = new RoomServer(
                this.roomProcessor,
                this,
                this.roomConfig,
                this.logger
            );

            logger.LogInformation($"New room created ${room.RoomId}");

            Task.Run(async () =>
            {
                try
                {
                    await room.Start();
                }
                catch (Exception ex)
                {
                    Debugger.Break();
                }
            });

            this.FreeRooms.TryAdd(room.RoomId, room);
            return room;
        }

        internal void ProcessEvent(GameEvent @event)
        {
            if (@event is RoomFilledEvent filledEvent)
            {
                HandleRoomFilled(filledEvent);
            }

            if (@event is RoomDestroyedEvent destroyedEvent)
            {
                HandleRoomDestroyed(destroyedEvent);
            }
        }

        private void HandleRoomFilled(RoomFilledEvent filledEvent)
        {
            if (FreeRooms.TryRemove(filledEvent.RoomId, out var room))
            {
                logger.LogInformation($"Room ${room.RoomId} is filled");
                UnavailableRooms.TryAdd(room.RoomId, room);
            }
        }

        private void HandleRoomDestroyed(RoomDestroyedEvent destroyedEvent)
        {
            logger.LogInformation($"Destoying room ${destroyedEvent.RoomId}");
            if (FreeRooms.TryRemove(destroyedEvent.RoomId, out var freeRoom))
            {
                freeRoom.Stop();

                foreach (var client in freeRoom.Clients)
                {
                    AssociateWithFreeRoom(client);
                }
            }

            if (UnavailableRooms.TryRemove(destroyedEvent.RoomId, out var unavailableRoom))
            {
                unavailableRoom.Stop();

                foreach (var client in unavailableRoom.Clients)
                {
                    AssociateWithFreeRoom(client);
                }
            }
            logger.LogInformation($"Room ${destroyedEvent.RoomId} is destroyed");
        }
    }
}
