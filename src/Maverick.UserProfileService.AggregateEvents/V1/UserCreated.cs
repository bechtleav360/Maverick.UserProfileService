using Maverick.UserProfileService.AggregateEvents.Common;

namespace Maverick.UserProfileService.AggregateEvents.V1
{
    /// <summary>
    ///     This event is emitted when a new user is created.<br />
    ///     Be aware! The version of this event does not correlate with the UPS API version.
    /// </summary>
    public class UserCreated : IUserProfileServiceEvent
    {
        /// <summary>
        ///     The reference of sensitive data in sensitive data store.
        /// </summary>
        public SensitiveReference DataReference { get; set; }

        ///<inheridoc />
        public string EventId { get; set; }

        /// <summary>
        ///     Used to identify the resource.
        /// </summary>
        public string Id { get; set; }

        ///<inheridoc />
        public EventMetaData MetaData { get; set; } = new EventMetaData();

        /// <summary>
        ///     The source name where the entity was transferred from (i.e. API, active directory).
        /// </summary>
        public string Source { get; set; }

        ///<inheridoc />
        public string Type => nameof(UserCreated);
    }
}
