using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Maverick.UserProfileService.Models.Annotations;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;

namespace Maverick.UserProfileService.Models.BasicModels
{
    /// <summary>
    ///     Defines a base model of a role.
    /// </summary>
    public class RoleBasic
    {
        /// <summary>
        ///     The time when the role has been created.
        /// </summary>
        public DateTime CreatedAt { set; get; }

        /// <summary>
        ///     Contains terms to reject or denied rights.
        /// </summary>
        [Modifiable]
        public IList<string> DeniedPermissions { set; get; } = new List<string>();

        /// <summary>
        ///     A statement describing the role.
        /// </summary>
        [Modifiable]
        [Searchable]
        public string Description { set; get; }

        /// <summary>
        ///     A collection of ids that are used to identify the resource in an external source.
        /// </summary>
        [Modifiable]
        public IList<ExternalIdentifier> ExternalIds { get; set; }

        /// <summary>
        ///     Used to identify the role.
        /// </summary>
        public string Id { set; get; }

        /// <summary>
        ///     If true, the group is system-relevant, that means it will be treated as read-only.
        /// </summary>
        [Modifiable]
        public bool IsSystem { set; get; } = false;

        /// <summary>
        ///     Defines the name of the role.
        /// </summary>
        [Required]
        [NotEmptyOrWhitespace]
        [Searchable]
        public string Name { set; get; }

        /// <summary>
        ///     Contains terms to authorize or grant rights.
        /// </summary>
        [Modifiable]
        public IList<string> Permissions { set; get; } = new List<string>();

        /// <summary>
        ///     The source name where the entity was transferred to (i.e. API, active directory).
        /// </summary>
        [Modifiable]
        public string Source { get; set; }

        /// <summary>
        ///     The time stamp when the object has been synchronized the last time.
        /// </summary>
        public DateTime? SynchronizedAt { set; get; }

        /// <summary>
        ///     The type of the Role.
        /// </summary>
        public RoleType Type { set; get; } = RoleType.Role;

        /// <summary>
        ///     The time when the role has been updated lastly.
        /// </summary>
        public DateTime UpdatedAt { set; get; }
    }
}
