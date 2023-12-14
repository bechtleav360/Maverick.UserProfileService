using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Common.Annotations;

namespace Maverick.UserProfileService.AggregateEvents.V1
{
    /// <summary>
    ///     Defines the event emitted when a child of an object has been deleted.<br />
    ///     Be aware! The version of this event does not correlate with the UPS API version.
    /// </summary>
    [AggregateEventDetails]
    public class MemberDeleted : IUserProfileServiceEvent
    {
        /// <summary>
        ///     The id of the container whose member has been deleted.
        /// </summary>
        public string ContainerId { get; set; }

        ///<inheridoc />
        public string EventId { get; set; }

        /// <summary>
        ///     The id of the member that has been deleted.
        /// </summary>
        public string MemberId { get; set; }

        ///<inheridoc />
        public EventMetaData MetaData { get; set; } = new EventMetaData();

        ///<inheridoc />
        public string Type => nameof(MemberDeleted);
    }
}
