namespace PunchGame.Server.Room.Core.Output
{
    /// <summary>
    /// Event that should be sent to other systems (like GameScheduler)
    /// </summary>
    public abstract class InternalEvent : GameEvent
    {
    }
}
