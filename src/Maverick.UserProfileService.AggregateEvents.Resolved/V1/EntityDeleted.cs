using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Common.Annotations;

namespace Maverick.UserProfileService.AggregateEvents.Resolved.V1
{
    /// <summary>
    ///     Defines the event emitted when the entity itself has been deleted.<br />
    ///     Be aware! The version of this event does not correlate with the UPS API version.
    /// </summary>
    [AggregateEventDetails(true)]
    public class EntityDeleted : IUserProfileServiceEvent
    {
        ///<inheridoc />
        public string EventId { get; set; }

        /// <summary>
        ///     The id of the current entity that has been deleted.
        /// </summary>
        public string Id { get; set; }

        ///<inheridoc />
        public EventMetaData MetaData { get; set; } = new EventMetaData();

        ///<inheridoc />
        public string Type => nameof(EntityDeleted);
    }
}
