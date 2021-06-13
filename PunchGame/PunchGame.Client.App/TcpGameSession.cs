using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using PunchGame.Server.Room.Core.Input;
using PunchGame.Server.Room.Core.Output;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PunchGame.Client.App
{
    // TODO: disconnect event
    class TcpGameSession : INetworkGameSession
    {
        private readonly NetworkConfig networkConfig;

        private readonly ConcurrentQueue<GameCommand> commands = new ConcurrentQueue<GameCommand>();
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        private TcpClient tcpClient;
        private NetworkStream stream;

        public ConcurrentQueue<GameEvent> Events { get; private set; } = new ConcurrentQueue<GameEvent>();

        public TcpGameSession(NetworkConfig networkConfig)
        {
            this.networkConfig = networkConfig;
        }

        public async Task Start()
        {
            this.tcpClient = new TcpClient(networkConfig.Hostname, networkConfig.Port);
            this.stream = tcpClient.GetStream();
            await Task.WhenAll(
                Task.Run(() => ListenEvents()),
                Task.Run(() => WriteCommands())
            );
        }

        public void Stop()
        {
            this.cts.Cancel();
            this.stream.Close();
            this.tcpClient.Close();
        }

        private async Task WriteCommands()
        {
            var streamWriter = new StreamWriter(stream);
            var jsonWriter = new JsonTextWriter(streamWriter);
            var serializer = new JsonSerializer();
            while (!cts.Token.IsCancellationRequested)
            {
                if (commands.TryDequeue(out var command))
                {
                    serializer.Serialize(jsonWriter, new
                    {
                        commandType = command.GetType().Name,
                        data = command
                    });
                    jsonWriter.Flush();
                }

                await Task.Yield();
            }
        }

        private async Task ListenEvents()
        {
            var reader = new StreamReader(this.stream, Encoding.UTF8);
            var jsonReader = new JsonTextReader(reader);
            var jsonSerialier = GetJsonSerialier();
            jsonReader.SupportMultipleContent = true;

            while (await jsonReader.ReadAsync(this.cts.Token))
            {
                var eventObject = jsonSerialier.Deserialize<JObject>(jsonReader);
                var @event = DeserializeEvent(eventObject);
                this.Events.Enqueue(@event);
                // TODO: remove this
                Console.WriteLine(JsonConvert.SerializeObject(@event));
                await Task.Yield();
            }
        }

        private static GameEvent DeserializeEvent(JObject eventObject)
        {
            var eventType = eventObject["eventType"].ToString();
            var csharpType = typeof(PlayerDiedEvent).Assembly.ExportedTypes
                .SingleOrDefault(x => x.Name == eventType);
            var eventData = eventObject["data"].ToString();
            var @event = JsonConvert.DeserializeObject(eventData, csharpType) as GameEvent;
            return @event;
        }

        private static JsonSerializer GetJsonSerialier()
        {
            return new JsonSerializer
            {
                Converters =
                {
                    new StringEnumConverter()
                }
            };
        }

        public void ExecuteCommand(GameCommand command)
        {
            this.commands.Enqueue(command);
        }
    }
}
