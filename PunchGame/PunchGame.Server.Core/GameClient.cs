using Newtonsoft.Json.Linq;
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

        public void Kill()
        {
            this.CTS.Cancel();
            this.Stream.Close();
            this.TcpClient.Close();
        }
    }
}
