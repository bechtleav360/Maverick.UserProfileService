using System;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Common.Models;

namespace Maverick.UserProfileService.AggregateEvents.V1
{
    /// <summary>
    ///     This event is emitted when a new group is created.<br />
    ///     Be aware! The version of this event does not correlate with the UPS API version.
    /// </summary>
    public class GroupCreated : IUserProfileServiceEvent
    {
        /// <summary>
        ///     The reference of sensitive data in sensitive data store.
        /// </summary>
        public SensitiveReference DataReference { get; set; }

        ///<inheridoc />
        public string EventId { get; set; }

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
        ///     The source name where the entity was transferred to (i.e. API, active directory).
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        ///     Tags to assign to group.
        /// </summary>
        public TagAssignment[] Tags { set; get; } = Array.Empty<TagAssignment>();

        ///<inheridoc />
        public string Type => nameof(GroupCreated);
    }
}
