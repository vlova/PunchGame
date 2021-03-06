namespace PunchGame.Server.Room.Core.Output
{
    public class AttemptToJoinRejectedEvent : PersonalEvent
    {
        public RejectReason Reason { get; set; }

        public enum RejectReason
        {
            VersionMismatch,
            NameNotUnique,
            NameNotValid,
            RoomIsFilled,
            GameCompleted
        }
    }
}
