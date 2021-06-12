using Newtonsoft.Json;
using PunchGame.Server.Room.Core.Input;
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace PunchGame.Client.App
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.Sleep(100);
            var tcpClient = new TcpClient("127.0.0.1", 6000);
            Console.WriteLine("client connected");
            using (var stream = tcpClient.GetStream())
            {
                var streamWriter = new StreamWriter(stream);
                var jsonWriter = new JsonTextWriter(streamWriter);
                var serializer = new JsonSerializer();
                serializer.Serialize(jsonWriter, new
                {
                    commandType = "connectToRoom",
                    data = new ConnectToRoomCommand { ClientVersion = 1, Name = "olo" }
                });
                jsonWriter.Flush();
                Thread.Sleep(500);
                serializer.Serialize(jsonWriter, new
                {
                    commandType = "punch",
                    data = new PunchCommand { VictimId = Guid.NewGuid() }
                });
                jsonWriter.Flush();
            }
        }
    }
}
