using System;
using System.Collections.Generic;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Common.Models;

namespace Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models
{
    /// <summary>
    ///     Defines a base model of a function.<br />
    ///     Be aware! The version of this event does not correlate with the UPS API version.
    /// </summary>
    public class Function : IContainer
    {
        /// <inheritdoc />
        public ContainerType ContainerType => ContainerType.Function;

        /// <summary>
        ///     The time when the resource has been created.
        /// </summary>
        public DateTime CreatedAt { set; get; }

        /// <summary>
        ///     A collection of ids that are used to identify the resource in an external source.
        /// </summary>
        public IList<ExternalIdentifier> ExternalIds { get; set; }

        /// <summary>
        ///     A unique string to identify a function.
        /// </summary>
        public string Id { set; get; }

        /// <summary>
        ///     The base model of a organization.
        /// </summary>
        public Organization Organization { get; set; }

        /// <summary>
        ///     The Id of the organization <see cref="Organization" />.
        /// </summary>
        public string OrganizationId { get; set; }

        /// <summary>
        ///     Describes the role that is related to the function.
        /// </summary>
        public Role Role { set; get; }

        /// <summary>
        ///     The id of the role that is related to the function.
        ///     <br />
        ///     Is required to validate the assignment of the role during updating.
        /// </summary>
        public string RoleId { get; set; }

        /// <summary>
        ///     The source name where the entity was transferred to (i.e. API, active directory).
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        ///     The time stamp when the object has been synchronized the last time.
        /// </summary>
        public DateTime? SynchronizedAt { set; get; }

        /// <summary>
        ///     The time when the resource has been updated lastly.
        /// </summary>
        public DateTime UpdatedAt { set; get; }
    }
}
