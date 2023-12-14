using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Common.Annotations;

namespace Maverick.UserProfileService.AggregateEvents.Resolved.V1
{
    /// <summary>
    ///     Defines an event emitted when client settings are set for profiles (user,group or organization).
    ///     Be aware! The version of this event does not correlate with the UPS API version.
    /// </summary>
    [AggregateEventDetails(true)]
    public class ProfileClientSettingsSet : IUserProfileServiceEvent
    {
        /// <summary>
        ///     Specifies the settings value to set (normally JSON document).
        /// </summary>
        public string ClientSettings { get; set; }

        ///<inheridoc />
        public string EventId { get; set; }

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
        public string Type => nameof(ProfileClientSettingsSet);
    }
}
