using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Common.Annotations;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models;

namespace Maverick.UserProfileService.AggregateEvents.Resolved.V1
{
    /// <summary>
    ///     This event is emitted when a member as been added.<br />
    ///     Be aware! The version of this event does not correlate with the UPS API version.
    /// </summary>
    [AggregateEventDetails(true)]
    public class MemberAdded : IUserProfileServiceEvent
    {
        ///<inheridoc />
        public string EventId { get; set; }

        /// <summary>
        ///     The member that has been added.
        /// </summary>
        public Member Member { get; set; }

        ///<inheridoc />
        public EventMetaData MetaData { get; set; } = new EventMetaData();

        /// <summary>
        ///     The id of the current parent.
        /// </summary>
        public string ParentId { get; set; }

        /// <summary>
        ///     The container type of the current parent.
        /// </summary>
        public ContainerType ParentType { set; get; }

        ///<inheridoc />
        public string Type => nameof(MemberAdded);
    }
}
