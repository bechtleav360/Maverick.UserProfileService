using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Maverick.UserProfileService.Models.Annotations;

namespace Maverick.UserProfileService.Models.Modifiable
{
    /// <summary>
    ///     The request to modify role properties.
    /// </summary>
    public class RoleModifiableProperties
    {
        /// <summary>
        ///     Contains term to reject or denied rights.
        /// </summary>
        public IList<string> DeniedPermissions { set; get; }

        /// <summary>
        ///     A statement describing the role.
        /// </summary>
        [Required]
        [NotEmptyOrWhitespace]
        public string Description { set; get; }

        /// <summary>
        ///     If true, the group is system-relevant, that means it will be treated as read-only.
        /// </summary>
        public bool IsSystem { set; get; }

        /// <summary>
        ///     Defines the name of the role.
        /// </summary>
        [Required]
        [NotEmptyOrWhitespace]
        public string Name { set; get; }

        /// <summary>
        ///     Contains terms to authorize or grant rights.
        /// </summary>
        public IList<string> Permissions { set; get; }
    }
}
