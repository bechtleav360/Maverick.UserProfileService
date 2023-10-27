using System.Collections.Generic;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;

namespace Maverick.UserProfileService.Models.Abstraction
{
    /// <summary>
    ///     Represents an object profiles can be assigned to (i.e. a function or a role).
    /// </summary>
    public interface IAssignmentObject
    {
        /// <summary>
        ///     A unique string to identify a function.
        /// </summary>
        string Id { set; get; }

        /// <summary>
        ///     Contains a list of associated profiles.
        /// </summary>
        IList<Member> LinkedProfiles { set; get; }

        /// <summary>
        ///     Defines the name of the resource.
        /// </summary>
        string Name { set; get; }

        /// <summary>
        ///     Identifies the type of this item. In this case it is "function".
        /// </summary>
        RoleType Type { set; get; }

        /// <summary>
        ///     Returns a collection that contains terms to authorize or grant rights.
        /// </summary>
        IList<string> GetPermissions();
    }
}
