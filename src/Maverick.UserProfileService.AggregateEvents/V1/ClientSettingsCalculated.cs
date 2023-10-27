using Maverick.UserProfileService.AggregateEvents.Common;

namespace Maverick.UserProfileService.AggregateEvents.V1
{
    /// <summary>
    ///     Defines an event emitted when the client settings have been recalculated.
    /// </summary>
    public class ClientSettingsCalculated : IUserProfileServiceEvent
    {
        /// <summary>
        ///     Reference to the client settings in sensitive data store.
        /// </summary>
        public SensitiveReference CalculatedSettingsReference { get; set; }

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
        public string Type => nameof(ClientSettingsCalculated);
    }
}
