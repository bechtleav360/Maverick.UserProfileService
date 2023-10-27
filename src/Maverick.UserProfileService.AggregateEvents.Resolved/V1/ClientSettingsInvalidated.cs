using Maverick.UserProfileService.AggregateEvents.Common;

namespace Maverick.UserProfileService.AggregateEvents.Resolved.V1
{
    /// <summary>
    ///     Defines an event emitted when the client settings have been recalculated.
    ///     It states the all keys that are not part of the event should be deleted.
    /// </summary>
    public class ClientSettingsInvalidated : IUserProfileServiceEvent
    {
        ///<inheridoc />
        public string EventId { get; set; }

        /// <summary>
        ///     Specifies the keys of the client-settings that are active now.
        /// </summary>
        public string[] Keys { get; set; }

        ///<inheridoc />
        public EventMetaData MetaData { get; set; } = new EventMetaData();

        /// <summary>
        ///     The id of the profile those client settings has been set.
        /// </summary>
        public string ProfileId { get; set; }

        ///<inheridoc />
        public string Type => nameof(ClientSettingsInvalidated);
    }
}
