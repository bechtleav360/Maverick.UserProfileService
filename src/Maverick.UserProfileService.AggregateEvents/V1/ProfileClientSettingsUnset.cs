using Maverick.UserProfileService.AggregateEvents.Common;

namespace Maverick.UserProfileService.AggregateEvents.V1
{
    /// <summary>
    ///     Defines an event emitted when the client settings with specified key of a profile has been deleted.
    /// </summary>
    public class ProfileClientSettingsUnset : IUserProfileServiceEvent
    {
        ///<inheridoc />
        public string EventId { get; set; }

        /// <summary>
        ///     Specifies the key of the client-settings to unset.
        /// </summary>
        public string Key { get; set; }

        ///<inheridoc />
        public EventMetaData MetaData { get; set; } = new EventMetaData();

        /// <summary>
        ///     The id of the profile those client settings has been unset.
        /// </summary>
        public string ProfileId { get; set; }

        ///<inheridoc />
        public string Type => nameof(ProfileClientSettingsUnset);
    }
}
