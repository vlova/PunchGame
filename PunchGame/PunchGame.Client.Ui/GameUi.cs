using PunchGame.Client.Core;
using PunchGame.Server.Room.Core.Models;
using PunchGame.Server.Room.Core.Output;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace PunchGame.Client.Ui
{
    public class GameUi : IGameUi
    {
        private readonly ConcurrentQueue<(RoomState state, GameEvent newEvent)> wantedRenders
            = new ConcurrentQueue<(RoomState state, GameEvent newEvent)>();

        private readonly List<string> latestEvents = new List<string> { };

        private readonly GameUiEventRenderer eventRenderer;
        private CancellationTokenSource cts;
        private Thread thread;

        public GameUi(GameUiEventRenderer eventRenderer)
        {
            this.eventRenderer = eventRenderer;
        }

        public void Run()
        {
            this.latestEvents.Clear();
            this.cts = new CancellationTokenSource();
            this.thread = new Thread(RunInternal);
            thread.Start();
        }

        public void Stop()
        {
            cts.Cancel();
            thread = null;
        }

        private void RunInternal()
        {
            var seenToken = cts.Token;
            while (!seenToken.IsCancellationRequested)
            {
                RoomState lastState = null;
                while (!seenToken.IsCancellationRequested && wantedRenders.TryDequeue(out var render))
                {
                    var renderedEvent = eventRenderer.RenderEvent(render.state, render.newEvent);
                    if (renderedEvent != null)
                    {
                        latestEvents.Add(renderedEvent);
                    }

                    lastState = render.state;
                }

                if (lastState != null)
                {
                    RenderUi(lastState);
                }
            }
        }

        public void Render(RoomState state, GameEvent newEvent)
        {
            wantedRenders.Enqueue((state.GetFullClone(), newEvent));
        }

        private void RenderUi(RoomState state)
        {
            try
            {
                Console.Clear();
                Console.SetWindowSize(UiConstants.ConsoleWidth, UiConstants.ConsoleHeight);

                var ui = BuildUI(state);
                foreach (var line in ui.Take(ui.Count))
                {
                    Console.WriteLine(line);
                }

                Console.Write("> ");
            }
            catch (Exception ex)
            {
                Debugger.Break();
            }
        }

        private List<string> BuildUI(RoomState state)
        {
            var emptyLine = string.Join("", Enumerable.Repeat(' ', UiConstants.ConsoleWidth));
            var ui = Enumerable.Repeat(emptyLine, UiConstants.ConsoleHeight - 2).ToList();
            ui = MergeUi(ui, RenderPlayers(state));
            ui = MergeUi(ui, RenderEvents());
            return ui;
        }

        private List<string> MergeUi(List<string> ui1, List<string> ui2)
        {
            var newUi = new List<string>();
            for (var lineNo = 0; lineNo < Math.Max(ui1.Count, ui2.Count); lineNo++)
            {
                var ui1Line = lineNo < ui1.Count ? ui1[lineNo] : "";
                var ui2Line = lineNo < ui2.Count ? ui2[lineNo] : "";
                var newUiLine = string.Join("",
                    ui1Line.PadRight(UiConstants.ConsoleWidth, ' ').Zip(
                        ui2Line.PadRight(UiConstants.ConsoleWidth, ' '),
                        (left, b) => (a: left, b))
                    .Select(t => t.a == ' ' ? t.b : t.a));

                newUi.Add(newUiLine);
            }

            return newUi;
        }

        private List<string> RenderPlayers(RoomState state)
        {
            var ui = new List<string>();
            var players = state.PlayerIdToPlayerMap.Values.ToList().OrderByDescending(p => p.Life).Take(UiConstants.ConsoleHeight - 2).ToList();
            foreach (var player in players)
            {
                var connected = player.IsConnected ? "[+]" : "[-]";
                ui.Add($"{player.Name.PadRight(UiConstants.NameWidth, ' ')} {Math.Min(player.Life, 99)}♥ {connected}");
            }

            return ui;
        }

        private List<string> RenderEvents()
        {
            var eventsUi = (latestEvents as IEnumerable<string>)
                .Reverse()
                .Select(x => x.PadLeft(UiConstants.ConsoleWidth, ' '))
                .Take(UiConstants.ConsoleHeight - 2)
                .Reverse()
                .ToList();

            return eventsUi;
        }
    }
}
