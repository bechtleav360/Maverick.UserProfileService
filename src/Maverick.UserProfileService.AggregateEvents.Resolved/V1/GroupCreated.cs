using System;
using System.Collections.Generic;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Common.Annotations;
using Maverick.UserProfileService.AggregateEvents.Common.Models;

namespace Maverick.UserProfileService.AggregateEvents.Resolved.V1
{
    /// <summary>
    ///     This event is emitted when a new group is created.<br />
    ///     Be aware! The version of this event does not correlate with the UPS API version.
    /// </summary>
    [AggregateEventDetails(true)]
    public class GroupCreated : IUserProfileServiceEvent
    {
        /// <summary>
        ///     The time when the resource has been created.
        /// </summary>
        public DateTime CreatedAt { set; get; }

        /// <summary>
        ///     The name that is used for displaying.
        /// </summary>
        public string DisplayName { get; set; }

        ///<inheridoc />
        public string EventId { get; set; }

        /// <summary>
        ///     A collection of ids that are used to identify the resource in an external source.
        /// </summary>
        public IList<ExternalIdentifier> ExternalIds { get; set; } = new List<ExternalIdentifier>();

        /// <summary>
        ///     Used to identify the group.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///     If true, the group is system-relevant, that means it will be treated as read-only.
        /// </summary>
        public bool IsSystem { set; get; } = false;

        ///<inheridoc />
        public EventMetaData MetaData { get; set; } = new EventMetaData();

        /// <summary>
        ///     Defines the name of the resource.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     The source name where the entity was transferred to (i.e. API, active directory).
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        ///     The time stamp when the object has been synchronized the last time.
        /// </summary>
        public DateTime? SynchronizedAt { set; get; }

        /// <summary>
        ///     Tags to assign to group.
        /// </summary>
        public TagAssignment[] Tags { set; get; } = Array.Empty<TagAssignment>();

        ///<inheridoc />
        public string Type => nameof(GroupCreated);

        /// <summary>
        ///     The time when the resource has been updated lastly.
        /// </summary>
        public DateTime UpdatedAt { set; get; }

        /// <summary>
        ///     The weight of a group profile that can be used to sort a result set.
        /// </summary>
        public double Weight { set; get; } = 0;

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Type}{Environment.NewLine}"
                + $"Id: {Id}{Environment.NewLine}"
                + $"Name: {Name}{Environment.NewLine}"
                + MetaData;
        }
    }
}
