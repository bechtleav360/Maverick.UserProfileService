using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Maverick.UserProfileService.Models.Annotations;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;

namespace Maverick.UserProfileService.Models.BasicModels
{
    /// <summary>
    ///     Defines a base model of a function.
    /// </summary>
    public class FunctionBasic
    {
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
        ///     Defines the name of the resource.
        /// </summary>
        [Searchable]
        public string Name { set; get; }

        /// <summary>
        ///     The base model of a organization.
        /// </summary>
        public OrganizationBasic Organization { get; set; }

        /// <summary>
        ///     The Id of the organization <see cref="OrganizationBasic" />
        /// </summary>
        [Required]
        [NotEmptyOrWhitespace]
        public virtual string OrganizationId { get; set; }

        /// <summary>
        ///     Describes the role that is related to the function.
        /// </summary>
        public RoleBasic Role { set; get; }

        /// <summary>
        ///     The id of the role that is related to the function.
        ///     <br />
        ///         Is required to validate the assignment of the role during updating.
        /// </summary>
        [Required]
        [NotEmptyOrWhitespace]
        public virtual string RoleId { get; set; }

        /// <summary>
        ///     The source name where the entity was transferred to (i.e. API, active directory).
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        ///     The time stamp when the object has been synchronized the last time.
        /// </summary>
        public DateTime? SynchronizedAt { set; get; }

        /// <summary>
        ///     Identifies the type of this item. In this case it is "function".
        /// </summary>
        public RoleType Type { set; get; } = RoleType.Function;

        /// <summary>
        ///     The time when the resource has been updated lastly.
        /// </summary>
        public DateTime UpdatedAt { set; get; }
    }
}
