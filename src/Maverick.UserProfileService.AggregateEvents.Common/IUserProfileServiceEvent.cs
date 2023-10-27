namespace Maverick.UserProfileService.AggregateEvents.Common
{
    /// <summary>
    ///     This interface represent a user profile service event.
    /// </summary>
    public interface IUserProfileServiceEvent
    {
        /// <summary>
        ///     An unique identifier of this event.
        /// </summary>
        string EventId { get; set; }

        /// <summary>
        ///     The metadata that belongs to the event.
        /// </summary>
        EventMetaData MetaData { get; set; }

        /// <summary>
        ///     A string value is used to identify the type of user profile service events.
        /// </summary>
        string Type { get; }
    }
}
