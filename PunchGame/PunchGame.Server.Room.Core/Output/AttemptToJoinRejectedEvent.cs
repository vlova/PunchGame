namespace PunchGame.Server.Room.Core.Output
{
    public class AttemptToJoinRejectedEvent : PersonalEvent
    {
        public RejectReason Reason { get; set; }

        public enum RejectReason
        {
            NameNotUnique,
            VersionMismatch, // (TODO) are you sure it should be here, not in scheduler?
            RoomIsFilled
        }
    }
}
