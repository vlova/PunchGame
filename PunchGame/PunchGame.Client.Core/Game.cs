using PunchGame.Server.Room.Core.Input;
using PunchGame.Server.Room.Core.Models;
using PunchGame.Server.Room.Core.Output;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PunchGame.Client.Core
{
    public class Game
    {
        private readonly GameSession gameSession;
        private readonly IGameUi ui;
        private readonly IGameController controller;
        private string userName;
        private CancellationTokenSource cts;
        private Task actualWaitTask;


        public RoomState RoomState => gameSession.RoomState;

        public Game(GameSession gameSession, IGameUi ui, IGameController controller)
        {
            this.gameSession = gameSession;
            this.ui = ui;
            this.controller = controller;
        }

        public async Task Run()
        {
            this.actualWaitTask = RunInternal(() =>
            {
                this.userName = controller.ReadUserName();
            });

            await WaitForCompetion();
        }

        private async Task WaitForCompetion()
        {
            while (true)
            {
                var seenTask = this.actualWaitTask;
                await seenTask;
                if (seenTask == this.actualWaitTask)
                {
                    return;
                }
            }
        }

        private async Task RunInternal(Action afterConnection)
        {
            this.cts = new CancellationTokenSource();
            var gameSessionTask = gameSession.Start();

            afterConnection();

            var reactOnEventsTask = Task.Run(() => ReactOnEventsTask());
            gameSession.ExecuteCommand(new ConnectToRoomCommand { ClientVersion = 1, Name = userName });

            ui.Run();

            await Task.WhenAll(
                gameSessionTask,
                reactOnEventsTask
            );
        }

        public async void Restart()
        {
            this.actualWaitTask = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                await RunInternal(() => { });
            });
            this.Stop();
        }

        public void ExecuteCommand(GameCommand command)
        {
            this.gameSession.ExecuteCommand(command);
        }

        public void Stop()
        {
            this.cts.Cancel();
            this.ui.Stop();
            this.gameSession.Stop();
        }

        private async Task ReactOnEventsTask()
        {
            var token = cts.Token;
            while (!token.IsCancellationRequested)
            {
                if (gameSession.Events.TryDequeue(out var gameEvent))
                {
                    HandleEvent(gameEvent, token);
                }

                await Task.Delay(1);
            }
        }

        private void HandleEvent(GameEvent gameEvent, CancellationToken token)
        {
            HandleEventInternal((dynamic)gameEvent, token);

            ui.Render(gameSession.RoomState, gameEvent);
        }

        private void HandleEventInternal(GameEvent gameEvent, CancellationToken token)
        {
        }

        private void HandleEventInternal(AttemptToJoinSuccessfulEvent gameEvent, CancellationToken token)
        {
            Task.Run(() => controller.ReadInput(this, this.ui, token));
        }

        private void HandleEventInternal(AttemptToJoinRejectedEvent gameEvent, CancellationToken token)
        {
            if (gameEvent.Reason == AttemptToJoinRejectedEvent.RejectReason.VersionMismatch)
            {
                Console.WriteLine("Server version mismatch");
                Stop();
                return;
            }

            if (gameEvent.Reason == AttemptToJoinRejectedEvent.RejectReason.NameNotValid)
            {
                Console.WriteLine("Your name is not valid");
                Stop();
                return;
            }

            Restart();
        }

        private void HandleEventInternal(ClientDisconnectedEvent gameEvent, CancellationToken token)
        {
            Restart();
        }
    }
}
