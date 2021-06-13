using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using PunchGame.Client.Core;
using PunchGame.Server.Room.Core.Input;
using PunchGame.Server.Room.Core.Output;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PunchGame.Client.Network
{
    public class TcpGameSession : INetworkGameSession
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
            tcpClient = new TcpClient(networkConfig.Hostname, networkConfig.Port);
            stream = tcpClient.GetStream();
            await Task.WhenAll(
                Task.Run(() => ListenEvents()),
                Task.Run(() => WriteCommands())
            );
        }

        public void Stop()
        {
            cts.Cancel();

            stream.Close();
            stream.Dispose();
            stream = null;

            tcpClient.Close();
            tcpClient.Dispose();
            tcpClient = null;
        }

        private async Task WriteCommands()
        {
            var streamWriter = new StreamWriter(stream);
            var jsonWriter = new JsonTextWriter(streamWriter);
            var serializer = new JsonSerializer();
            var seenToken = cts.Token;
            while (!seenToken.IsCancellationRequested)
            {
                if (commands.TryDequeue(out var command))
                {
                    serializer.Serialize(jsonWriter, new
                    {
                        commandType = command.GetType().Name,
                        data = command
                    });

                    try
                    {
                        jsonWriter.Flush();
                    }
                    catch (IOException _)
                    {
                        HandleDisconnect();
                    }
                }

                await Task.Delay(1);
            }
        }

        private async Task ListenEvents()
        {
            var reader = new StreamReader(stream, Encoding.UTF8);
            var jsonReader = new JsonTextReader(reader);
            var jsonSerialier = GetJsonSerialier();
            jsonReader.SupportMultipleContent = true;

            try
            {
                var seenToken = cts.Token;
                while (await jsonReader.ReadAsync(seenToken))
                {
                    var eventObject = jsonSerialier.Deserialize<JObject>(jsonReader);
                    var @event = DeserializeEvent(eventObject);
                    if (@event != null)
                    {
                        Events.Enqueue(@event);
                    }
                    await Task.Delay(1);
                }
            }
            catch (IOException _)
            {
                HandleDisconnect();
            }
        }

        private void HandleDisconnect()
        {
            this.Events.Enqueue(new ClientDisconnectedEvent());
        }

        private static GameEvent DeserializeEvent(JObject eventObject)
        {
            var eventType = eventObject["eventType"].ToString();
            if (eventType == "keepAlive")
            {
                return null;
            }

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
            commands.Enqueue(command);
        }
    }
}
