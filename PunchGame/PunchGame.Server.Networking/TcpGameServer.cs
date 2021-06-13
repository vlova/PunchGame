using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PunchGame.Server.App
{
    // TODO: fix impl. GameServer should depend on TcpGameServer
    public class TcpGameServer
    {
        private readonly IPAddress address;
        private readonly int port;
        private readonly GameServer server;
        private readonly ILogger logger;
        private TcpListener tcpListener;

        public TcpGameServer(IPAddress address, int port, GameServer server, ILogger logger)
        {
            this.address = address;
            this.port = port;
            this.server = server;
            this.logger = logger;
        }

        public async Task Start()
        {
            if (this.tcpListener != null)
            {
                throw new InvalidOperationException();
            }

            this.tcpListener = new TcpListener(address, port);
            this.tcpListener.Start();

            while (true)
            {
                var tcpClient = await this.tcpListener.AcceptTcpClientAsync();
                var gameClient = new GameClient { TcpClient = tcpClient, Stream = tcpClient.GetStream() };
                logger.LogInformation($"New client {gameClient.ConnectionId} connected");
                Run(() => ReadFromClient(gameClient), "reading from client");
                Run(() => WriteToClient(gameClient), "writing to client");
                Run(() => CheckClientAlive(gameClient), "checking client alive");
                Run(() => server.RunClient(gameClient), "processing cleint");
            }
        }

        private void Run(Func<Task> action, string operation)
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () =>
            {
                try
                {
                    await action();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Crash during {operation}");
                }
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private async Task ReadFromClient(GameClient gameClient)
        {
            var reader = new StreamReader(gameClient.Stream, Encoding.UTF8);
            var jsonReader = new JsonTextReader(reader);
            var jsonSerialier = GetJsonSerialier();
            jsonReader.SupportMultipleContent = true;

            try
            {
                while (await jsonReader.ReadAsync(gameClient.CancellationToken))
                {
                    var command = jsonSerialier.Deserialize<JObject>(jsonReader);
                    gameClient.Inputs.Enqueue(command);
                    await Task.Delay(1);
                }
            }
            catch (IOException)
            {
                logger.LogInformation($"Client {gameClient.ConnectionId} disconnected");
                gameClient.OnDisconnect();
            }
        }

        private async Task WriteToClient(GameClient gameClient)
        {
            var writer = new StreamWriter(gameClient.Stream, Encoding.UTF8);
            var jsonWriter = new JsonTextWriter(writer);
            var jsonSerialier = GetJsonSerialier();
            try
            {
                while (!gameClient.CancellationToken.IsCancellationRequested)
                {
                    if (gameClient.Outputs.TryDequeue(out var jObject))
                    {
                        jsonSerialier.Serialize(jsonWriter, jObject);
                        await jsonWriter.FlushAsync();
                    }

                    await Task.Delay(1);
                }
            }
            catch (IOException)
            {
                logger.LogInformation($"Client {gameClient.ConnectionId} disconnected");
                gameClient.OnDisconnect();
            }
        }

        private async Task CheckClientAlive(GameClient gameClient)
        {
            while (!gameClient.CancellationToken.IsCancellationRequested)
            {
                gameClient.Outputs.Enqueue(JObject.FromObject(new { eventType = "keepAlive" }));
                await Task.Delay(100);
            }
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

        public void Stop()
        {
            if (this.tcpListener == null)
            {
                throw new InvalidOperationException();
            }

            this.tcpListener.Stop();
            this.tcpListener = null;
            // TODO: kill all game clients
        }

        ~TcpGameServer()
        {
            if (this.tcpListener != null)
            {
                this.Stop();
            }
        }
    }
}
