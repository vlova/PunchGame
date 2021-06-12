namespace PunchGame.Server.Room.Core.Input
{
    public class ConnectToRoomCommand : GameCommand
    {
        // TODO: consider using semantic versioning
        public int ClientVersion { get; set; }

        public string Name { get; set; }
    }
}
