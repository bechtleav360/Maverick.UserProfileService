using Maverick.UserProfileService.AggregateEvents.Common;

namespace Maverick.UserProfileService.AggregateEvents.Resolved.V1
{
    /// <summary>
    ///     Defines an event emitted when the client settings have been recalculated.
    /// </summary>
    public class ClientSettingsCalculated : IUserProfileServiceEvent
    {
        /// <summary>
        ///     Contains settings calculated by hierarchy/inheritance (normally JSON document).
        /// </summary>
        public string CalculatedSettings { get; set; }

        ///<inheridoc />
        public string EventId { get; set; }

        /// <summary>
        ///     Indicates whether these settings were set directly on the profile with id <see cref="ProfileId" /> or
        ///     whether they were inherited from another profile.
        /// </summary>
        public bool IsInherited { get; set; }

        /// <summary>
        ///     Specifies the key of the client-settings to set.
        /// </summary>
        public string Key { get; set; }

        ///<inheridoc />
        public EventMetaData MetaData { get; set; } = new EventMetaData();

        /// <summary>
        ///     The id of the profile those client settings has been set.
        /// </summary>
        public string ProfileId { get; set; }

        ///<inheridoc />
        public string Type => nameof(ClientSettingsCalculated);
    }
}
