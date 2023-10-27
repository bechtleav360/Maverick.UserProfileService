using System.Collections.Generic;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.BasicModels;

namespace Maverick.UserProfileService.Models.Models
{
    /// <summary>
    ///     Represents an "extended" version of a role based on <see cref="RoleBasic" />. It contains more properties thant its
    ///     base class.
    /// </summary>
    public class RoleView : RoleBasic, IAssignmentObject
    {
        /// <inheritdoc />
        public IList<Member> LinkedProfiles { set; get; } = new List<Member>();

        /// <inheritdoc />
        public IList<string> GetPermissions()
        {
            return Permissions;
        }
    }
}
