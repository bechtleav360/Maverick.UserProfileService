using System;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Common.Annotations;
using Maverick.UserProfileService.AggregateEvents.Common.Models;

namespace Maverick.UserProfileService.AggregateEvents.V1
{
    /// <summary>
    ///     Defines an event emitted when a child profile has been added to a target (i.e. function, group, ...).<br />
    ///     Be aware! The version of this event does not correlate with the UPS API version.
    /// </summary>
    [AggregateEventDetails]
    public class WasAssignedTo : IUserProfileServiceEvent
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
        ///     The id of the child profile that has been assigned to <see cref="Target" />.
        /// </summary>
        public string ProfileId { get; set; }

        /// <summary>
        ///     The reference to the entry of the target object in the sensitive data store.
        /// </summary>
        public SensitiveReference Target { get; set; }

        ///<inheridoc />
        public string Type => nameof(WasAssignedTo);
    }
}
