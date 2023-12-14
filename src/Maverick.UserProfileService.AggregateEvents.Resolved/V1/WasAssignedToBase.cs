using System;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Common.Annotations;
using Maverick.UserProfileService.AggregateEvents.Common.Models;

namespace Maverick.UserProfileService.AggregateEvents.Resolved.V1
{
    /// <summary>
    ///     Defines the base class of events emitted when a child profile has been added to a target (i.e. function, group,)
    ///     <br />
    ///     Be aware! The version of this event does not correlate with the UPS API version.
    /// </summary>
    /// <typeparam name="TContainer">Type of the target object.</typeparam>
    [AggregateEventDetails(true)]
    public abstract class WasAssignedToBase<TContainer> : IUserProfileServiceEvent
        where TContainer : class
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
        ///     The target object as a new parent of <see cref="ProfileId" />.
        /// </summary>
        public TContainer Target { get; set; }

        ///<inheridoc />
        public string Type => GetType().Name;
    }
}
