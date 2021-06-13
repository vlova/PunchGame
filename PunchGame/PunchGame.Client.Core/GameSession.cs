using Newtonsoft.Json;
using PunchGame.Server.Room.Core.Input;
using PunchGame.Server.Room.Core.Models;
using PunchGame.Server.Room.Core.Output;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace PunchGame.Client.Core
{
    public class GameSession
    {
        private readonly Func<INetworkGameSession> networkSessionFactory;
        private readonly ClientGameEventReducer gameEventReducer;

        private INetworkGameSession networkSession;
        private CancellationTokenSource cts;
        public RoomState RoomState { get; private set; }
        public ConcurrentQueue<GameEvent> Events { get; private set; } = new ConcurrentQueue<GameEvent>();

        public GameSession(Func<INetworkGameSession> networkSessionFactory, ClientGameEventReducer gameEventReducer)
        {
            this.networkSessionFactory = networkSessionFactory;
            this.gameEventReducer = gameEventReducer;
        }

        public async Task Start()
        {
            if (cts != null)
            {
                throw new InvalidOperationException("Game session already started");
            }

            cts = new CancellationTokenSource();
            RoomState = new RoomState();
            networkSession = networkSessionFactory();

            await Task.WhenAll(
                networkSession.Start(),
                Task.Run(() => ListenEvents())
            );
        }

        private async Task ListenEvents()
        {
            while (!cts.Token.IsCancellationRequested)
            {
                if (networkSession.Events.TryDequeue(out var @event))
                {
                    try
                    {
                        gameEventReducer.Process(RoomState, @event);
                    }
                    catch (Exception ex)
                    {
                        Debugger.Break();
                    }

                    Events.Enqueue(@event);
                }
                await Task.Delay(1);
            }
        }

        public void Stop()
        {
            cts.Cancel();
            cts = null;
            networkSession.Stop();
        }

        internal void ExecuteCommand(GameCommand command)
        {
            networkSession.ExecuteCommand(command);
        }
    }
}
