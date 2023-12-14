using System;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Common.Annotations;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Common.Models;

namespace Maverick.UserProfileService.AggregateEvents.V1
{
    /// <summary>
    ///     Defines an event emitted when a child profile has been unassigned from a parent (i.e. function, group,...).<br />
    ///     Be aware! The version of this event does not correlate with the UPS API version.
    /// </summary>
    [AggregateEventDetails]
    public class WasUnassignedFrom : IUserProfileServiceEvent
    {
        /// <summary>
        ///     Condition when the assignment is valid.
        /// </summary>
        public RangeCondition[] Conditions { get; set; } = Array.Empty<RangeCondition>();

        ///<inheridoc />
        public string EventId { get; set; }

        ///<inheridoc />
        public EventMetaData MetaData { get; set; } = new EventMetaData();

        /// <summary>
        ///     The id of the parent from which the current profile has been unassigned.
        /// </summary>
        public string ParentId { get; set; }

        /// <summary>
        ///     The container type of the parent from which the current profile has been unassigned.
        /// </summary>
        public ContainerType ParentType { get; set; }

        /// <summary>
        ///     The id of the child profile that has been assigned to <see cref="ParentId" />.
        /// </summary>
        public string ProfileId { get; set; }

        ///<inheridoc />
        public string Type => nameof(WasUnassignedFrom);
    }
}
