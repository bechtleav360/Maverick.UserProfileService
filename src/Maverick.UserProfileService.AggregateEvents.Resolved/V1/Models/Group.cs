using System;
using System.Collections.Generic;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Common.Models;

namespace Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models
{
    /// <summary>
    ///     The base model of a group.
    /// </summary>
    public class Group : IContainer
    {
        /// <inheritdoc />
        public ContainerType ContainerType => ContainerType.Group;

        /// <summary>
        ///     The time when the resource has been created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        ///     The name that is used for displaying.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        ///     A collection of ids that are used to identify the resource in an external source.
        /// </summary>
        public IList<ExternalIdentifier> ExternalIds { get; set; }

        /// <summary>
        ///     Used to identify the resource.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///     A boolean value that is true if the resource should be deleted but it is not possible cause of underlying
        ///     dependencies.
        /// </summary>
        public bool IsMarkedForDeletion { set; get; }

        /// <summary>
        ///     If true, the group is system-relevant, that means it will be treated as read-only.
        /// </summary>
        public bool IsSystem { set; get; }

        /// <summary>
        ///     A profile kind is used to identify a profile. Either it is group or a user.
        /// </summary>
        public ProfileKind Kind => ProfileKind.Group;

        /// <summary>
        ///     Defines the name of the resource.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     The source name where the entity was transferred from (i.e. API, active directory).
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        ///     The time stamp when the object has been synchronized the last time.
        /// </summary>
        public DateTime? SynchronizedAt { set; get; }

        /// <summary>
        ///     The time when the resource has been updated lastly.
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        ///     The weight of a group profile that can be used to sort a result set.
        /// </summary>
        public double Weight { set; get; } = 0;
    }
}
