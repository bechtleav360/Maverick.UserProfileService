using System;
using System.Collections.Generic;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Common.Models;

namespace Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models
{
    /// <summary>
    ///     The base model of a organization.
    /// </summary>
    public class Organization : IContainer
    {
        /// <inheritdoc cref="IContainer" />
        public ContainerType ContainerType => ContainerType.Organization;

        /// <summary>
        ///     The time when the resource has been created.
        /// </summary>
        public DateTime CreatedAt { set; get; }

        /// <summary>
        ///     If true, the organization is system-relevant, that means it will be treated as read-only.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        ///     A collection of ids that are used to identify the resource in an external source.
        /// </summary>
        public IList<ExternalIdentifier> ExternalIds { get; set; }

        /// <summary>
        ///     Used to identify the resource.
        /// </summary>
        public string Id { set; get; }

        /// <summary>
        ///     A boolean value that is true if the resource should be deleted but it is not possible cause of underlying
        ///     dependencies.
        /// </summary>
        public bool IsMarkedForDeletion { set; get; }

        /// <summary>
        ///     If true the organization is an sub-organization.
        /// </summary>
        public bool IsSubOrganization { get; set; }

        /// <summary>
        ///     If true, the organization is system-relevant, that means it will be treated as read-only.
        /// </summary>
        public bool IsSystem { set; get; }

        /// <summary>
        ///     A profile kind is used to identify a profile. Either it is group or a user.
        /// </summary>
        public ProfileKind Kind => ProfileKind.Organization;

        /// <summary>
        ///     Defines the name of the resource.
        /// </summary>
        public string Name { set; get; }

        /// <summary>
        ///     The source name where the entity was transferred to (i.e. API, active directory).
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        ///     The time stamp when the object has been synchronized the last time.
        /// </summary>
        public DateTime? SynchronizedAt { set; get; }

        /// <summary>
        ///     The url of the source that contains detailed information about related tags.
        /// </summary>
        public string TagUrl { set; get; }

        /// <summary>
        ///     The time when the resource has been updated lastly.
        /// </summary>
        public DateTime UpdatedAt { set; get; }

        /// <summary>
        ///     The weight of a organization profile that can be used to sort a result set.
        /// </summary>
        public double Weight { set; get; } = 0;
    }
}
