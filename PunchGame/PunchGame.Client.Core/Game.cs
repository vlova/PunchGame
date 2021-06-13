using PunchGame.Server.Room.Core.Input;
using PunchGame.Server.Room.Core.Output;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PunchGame.Client.Core
{
    public class Game
    {
        private readonly GameSession gameSession;
        private readonly IGameUi ui;
        private string userName;

        public Game(GameSession gameSession, IGameUi ui)
        {
            this.gameSession = gameSession;
            this.ui = ui;
        }

        public async Task Run()
        {
            var gameSessionTask = gameSession.Start();

            Console.WriteLine("What's your name? ");
            userName = Console.ReadLine();

            var controllerTask = Task.Run(() => ReadInput());
            var reactOnEventsTask = Task.Run(() => ReactOnEventsTask());
            gameSession.ExecuteCommand(new ConnectToRoomCommand { ClientVersion = 1, Name = userName });

            ui.Run();
            await Task.WhenAll(
                gameSessionTask,
                controllerTask,
                reactOnEventsTask
            );
        }

        private void ReadInput()
        {
            while (true)
            {
                var commandText = Console.ReadLine();
                var command = ParseCommand(commandText);

                ui.Render(gameSession.RoomState, new CommandSentEvent { });
                if (command == null)
                {
                    continue;
                }
                else if (command is RestartClientCommand)
                {
                    Restart();
                }
                else
                {
                    gameSession.ExecuteCommand(command);
                }
            }
        }

        private async Task ReactOnEventsTask()
        {
            while (true)
            {
                if (gameSession.Events.TryDequeue(out var gameEvent))
                {
                    if (gameEvent is AttemptToJoinRejectedEvent rejected)
                    {
                        HandleJoinReject(rejected);
                    }

                    ui.Render(gameSession.RoomState, gameEvent);
                }

                await Task.Delay(1);
            }
        }

        private void HandleJoinReject(AttemptToJoinRejectedEvent rejected)
        {
            if (rejected.Reason == AttemptToJoinRejectedEvent.RejectReason.VersionMismatch)
            {
                Console.WriteLine("Server version mismatch");
                Stop();
                return;
            }

            if (rejected.Reason == AttemptToJoinRejectedEvent.RejectReason.NameNotValid)
            {
                Console.WriteLine("Your name is not valid");
                Stop();
                return;
            }

            Restart();
        }

        private void Stop()
        {
            throw new NotImplementedException();
        }

        private void Restart()
        {
            throw new NotImplementedException();
        }

        private GameCommand ParseCommand(string commandText)
        {
            if (commandText.StartsWith("Punch ", StringComparison.InvariantCultureIgnoreCase))
            {
                var name = commandText.Substring("Punch ".Length);
                // TODO(perf) this can be optimized
                var victim = gameSession.RoomState.PlayerIdToPlayerMap.Values.SingleOrDefault(x => x.Name == name);

                if (victim == null)
                {
                    return null;
                }

                return new PunchCommand
                {
                    VictimId = victim.Id
                };
            }

            if (string.Equals(commandText, "Fight more", StringComparison.InvariantCultureIgnoreCase))
            {
                return new RestartClientCommand { };
            }

            return null;
        }
    }
}
