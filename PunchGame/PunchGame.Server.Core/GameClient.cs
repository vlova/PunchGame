using Newtonsoft.Json.Linq;
using PunchGame.Server.Room.Core.Input;
using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;

namespace PunchGame.Server.App
{
    public class GameClient
    {
        public TcpClient TcpClient { get; set; }

        public NetworkStream Stream { get; set; }

        public Guid ConnectionId { get; set; } = Guid.NewGuid();

        public ConcurrentQueue<JObject> Inputs { get; set; } = new ConcurrentQueue<JObject>();

        public ConcurrentQueue<JObject> Outputs { get; set; } = new ConcurrentQueue<JObject>();

        private CancellationTokenSource CTS { get; set; } = new CancellationTokenSource();

        public CancellationToken CancellationToken => CTS.Token;

        public RoomServer AssociatedRoom { get; set; } = null;

        public void OnDisconnect()
        {
            this.Inputs.Enqueue(JObject.FromObject(new
            {
                commandType = nameof(DisconnectCommand),
                data = new DisconnectCommand { ByConnectionId = this.ConnectionId, Timestamp = DateTime.UtcNow }
            }));
        }

        public void Dispose()
        {
            this.CTS.Cancel();

            if (this.Stream != null)
            {
                this.Stream.Close();
                this.Stream.Dispose();
                this.Stream = null;
            }

            if (this.TcpClient != null)
            {
                this.TcpClient.Close();
                this.TcpClient.Dispose();
                this.TcpClient = null;
            }
        }
    }
}
