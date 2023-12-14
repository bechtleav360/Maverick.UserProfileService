using System.Collections.Generic;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Common.Annotations;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Common.Models;

namespace Maverick.UserProfileService.AggregateEvents.Resolved.V1
{
    /// <summary>
    ///     Defines the event emitted when a child has been
    ///     unassigned from the parent.
    /// </summary>
    [AggregateEventDetails(true)]
    public class MemberRemoved : IUserProfileServiceEvent
    {
        /// <summary>
        ///     A list of range-condition that should be deleted.
        /// </summary>
        public IList<RangeCondition> Conditions { get; set; }

        ///<inheridoc />
        public string EventId { get; set; }

        /// <summary>
        ///     The id of the member that has been unassigned.
        /// </summary>
        public string MemberId { get; set; }

        /// <summary>
        ///     The profile kind of the member that has been unassigned.
        /// </summary>
        public ProfileKind MemberKind { set; get; }

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
        public string Type => nameof(MemberRemoved);
    }
}
