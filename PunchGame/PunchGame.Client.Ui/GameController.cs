using PunchGame.Client.Core.IO;
using PunchGame.Server.Room.Core.Input;
using System;
using System.Linq;
using System.Threading;

namespace PunchGame.Client.Core
{
    public class GameController : IGameController
    {
        public string ReadUserName()
        {
            Console.WriteLine("What's your name? ");
            return Console.ReadLine();
        }

        public void ReadInput(Game game, IGameUi ui, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var commandText = Console.ReadLine();
                var command = ParseCommand(game, commandText);

                ui.Render(game.RoomState, new CommandSentEvent { });
                if (command == null)
                {
                    continue;
                }
                else if (command is RestartClientCommand)
                {
                    game.Restart();
                }
                else if (command is StopClientCommand)
                {
                    game.Stop();
                }
                else
                {
                    game.ExecuteCommand(command);
                }
            }
        }

        private GameCommand ParseCommand(Game game, string commandText)
        {
            if (commandText.StartsWith("Punch ", StringComparison.InvariantCultureIgnoreCase))
            {
                var name = commandText.Substring("Punch ".Length);
                // TODO(perf) this can be optimized
                var victim = game.RoomState.PlayerIdToPlayerMap.Values.SingleOrDefault(x => x.Name == name);

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

            if (string.Equals(commandText, "quit", StringComparison.InvariantCultureIgnoreCase))
            {
                return new StopClientCommand { };
            }

            if (string.Equals(commandText, "exit", StringComparison.InvariantCultureIgnoreCase))
            {
                return new StopClientCommand { };
            }

            return null;
        }
    }
}
