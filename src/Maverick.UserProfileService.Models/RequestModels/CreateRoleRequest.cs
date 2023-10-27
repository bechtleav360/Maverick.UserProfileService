using System.Collections.Generic;
using Maverick.UserProfileService.Models.Models;
// ReSharper disable NonReadonlyMemberInGetHashCode

namespace Maverick.UserProfileService.Models.RequestModels
{
    /// <summary>
    ///     The request to create a role.
    /// </summary>
    public class CreateRoleRequest
    {
        /// <summary>
        ///     Contains term to reject or denied rights.
        /// </summary>
        public IList<string> DeniedPermissions { set; get; } = new List<string>();

        /// <summary>
        ///     A statement describing the role.
        /// </summary>
        public string Description { set; get; }

        /// <summary>
        ///     A collection of ids that are used to identify the resource in an external source.
        /// </summary>
        public IList<ExternalIdentifier> ExternalIds { get; set; } = new List<ExternalIdentifier>();

        /// <summary>
        ///     If true, the group is system-relevant, that means it will be treated as read-only.
        /// </summary>
        public bool IsSystem { set; get; }

        /// <summary>
        ///     Defines the name of the role.
        /// </summary>
        public string Name { set; get; }

        /// <summary>
        ///     Contains terms to authorize or grant rights.
        /// </summary>
        public IList<string> Permissions { set; get; }

        /// <summary>
        ///     Tags to assign to role.
        /// </summary>
        public IList<TagAssignment> Tags { set; get; } = new List<TagAssignment>();

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Name != null ? Name.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (Description != null ? Description.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Permissions != null ? Permissions.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ IsSystem.GetHashCode();
                hashCode = (hashCode * 397) ^ (ExternalIds != null ? ExternalIds.GetHashCode() : 0);

                return hashCode;
            }
        }
    }
}
