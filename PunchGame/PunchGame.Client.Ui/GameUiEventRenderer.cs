using PunchGame.Server.Room.Core.Models;
using PunchGame.Server.Room.Core.Output;

namespace PunchGame.Client.Ui
{
    public class GameUiEventRenderer
    {
        public string RenderEvent(RoomState roomState, GameEvent @event)
        {
            return RenderEventInternal(roomState, (dynamic)@event);
        }

        private string RenderEventInternal(RoomState roomState, GameEvent @event)
        {
            return null;
        }

        private string RenderEventInternal(RoomState roomState, PunchEvent @event)
        {
            var killer = roomState.PlayerIdToPlayerMap[@event.KillerId].Name;
            var victim = roomState.PlayerIdToPlayerMap[@event.VictimId].Name;
            return killer + " punched "
                + victim + " -"
                + @event.Damage;
        }

        private string RenderEventInternal(RoomState roomState, PlayerDiedEvent @event)
        {
            var killer = roomState.PlayerIdToPlayerMap[@event.KillerId].Name;
            var victim = roomState.PlayerIdToPlayerMap[@event.VictimId].Name;
            return killer + " killed " + victim;
        }

        private string RenderEventInternal(RoomState roomState, PlayerDisconnectedEvent @event)
        {
            var player = roomState.PlayerIdToPlayerMap[@event.PlayerId].Name;
            return player + " disconnected";
        }

        private string RenderEventInternal(RoomState roomState, PlayerJoinedEvent @event)
        {
            var player = roomState.PlayerIdToPlayerMap[@event.PlayerId].Name;
            return player + " joined";
        }

        private string RenderEventInternal(RoomState roomState, GameEndedEvent @event)
        {
            if (@event.Reason == GameEndedEvent.EventReason.Crash)
            {
                return "server crashed";
            }

            var player = roomState.PlayerIdToPlayerMap[@event.WinnerId.Value].Name;
            return player + " is winner";
        }

        private string RenderEventInternal(RoomState roomState, AttemptToJoinRejectedEvent @event)
        {
            return "room rejected you " + @event.Reason.ToString();
        }
    }
}
