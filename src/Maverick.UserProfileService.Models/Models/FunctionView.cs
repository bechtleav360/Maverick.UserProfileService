using System.Collections.Generic;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.BasicModels;

namespace Maverick.UserProfileService.Models.Models
{
    /// <summary>
    ///     Defines a view model of a function which is required by the admin-ui2.
    /// </summary>
    public class FunctionView : FunctionBasic, IAssignmentObject
    {
        /// <inheritdoc />
        public IList<Member> LinkedProfiles { set; get; } = new List<Member>();

        /// <inheritdoc />
        public IList<string> GetPermissions()
        {
            return Role?.Permissions;
        }
    }
}
