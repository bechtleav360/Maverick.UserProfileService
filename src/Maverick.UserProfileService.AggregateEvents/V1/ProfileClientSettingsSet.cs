using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Common.Annotations;

namespace Maverick.UserProfileService.AggregateEvents.V1
{
    /// <summary>
    ///     Defines an event emitted when client settings are set for profiles (user,group or organization).
    ///     Be aware! The version of this event does not correlate with the UPS API version.
    /// </summary>
    [AggregateEventDetails]
    public class ProfileClientSettingsSet : IUserProfileServiceEvent
    {
        ///<inheridoc />
        public string EventId { get; set; }

        /// <summary>
        ///     Specifies the key of the client-settings to set.
        /// </summary>
        public string Key { get; set; }

        ///<inheridoc />
        public EventMetaData MetaData { get; set; } = new EventMetaData();

        /// <summary>
        ///     Reference to profile including client setting.
        /// </summary>
        public SensitiveReference Profile { get; set; }

        ///<inheridoc />
        public string Type => nameof(ProfileClientSettingsSet);
    }
}
