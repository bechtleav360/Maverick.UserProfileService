using System;
using System.Collections.Generic;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Common.Annotations;
using Maverick.UserProfileService.AggregateEvents.Common.Models;

namespace Maverick.UserProfileService.AggregateEvents.V1
{
    /// <summary>
    ///     A model used to wrap all properties required for creating a role.<br />
    ///     Be aware! The version of this event does not correlate with the UPS API version.
    /// </summary>
    [AggregateEventDetails]
    public class RoleCreated : IUserProfileServiceEvent
    {
        /// <summary>
        ///     Contains term to reject or denied rights.
        /// </summary>
        public IList<string> DeniedPermissions { set; get; } = new List<string>();

        /// <summary>
        ///     A statement describing the role.
        /// </summary>
        public string Description { set; get; }

        ///<inheridoc />
        public string EventId { get; set; }

        /// <summary>
        ///     A collection of ids that are used to identify the resource in an external source.
        /// </summary>
        public IList<ExternalIdentifier> ExternalIds { get; set; } = new List<ExternalIdentifier>();

        /// <summary>
        ///     Used to identify the role.
        /// </summary>
        public string Id { set; get; }

        /// <summary>
        ///     If true, the group is system-relevant, that means it will be treated as read-only.
        /// </summary>
        public bool IsSystem { set; get; } = false;

        ///<inheridoc />
        public EventMetaData MetaData { get; set; } = new EventMetaData();

        /// <summary>
        ///     Defines the name of the role.
        /// </summary>
        public string Name { set; get; }

        /// <summary>
        ///     Contains terms to authorize or grant rights.
        /// </summary>
        public IList<string> Permissions { set; get; } = new List<string>();

        /// <summary>
        ///     The source name where the entity was transferred from (i.e. API, active directory).
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        ///     Tags to assign to group.
        /// </summary>
        public TagAssignment[] Tags { set; get; } = Array.Empty<TagAssignment>();

        ///<inheridoc />
        public string Type => nameof(RoleCreated);
    }
}
