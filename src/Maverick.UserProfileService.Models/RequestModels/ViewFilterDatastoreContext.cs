using Maverick.UserProfileService.Models.Annotations;

namespace Maverick.UserProfileService.Models.RequestModels
{
    /// <summary>
    /// Defines constants of types of entities that are relevant for a specific view filter query.<br/>
    /// </summary>
    public enum ViewFilterDataStoreContext
    {
        /// <summary>
        /// The resulting data set is only related to users.
        /// </summary>
        [FilterSerialize("users")]
        User = 0,

        /// <summary>
        /// The resulting data set is only related to groups.
        /// </summary>
        [FilterSerialize("groups")]
        Groups = 1,

        /// <summary>
        /// The resulting data set is only related to functions.
        /// </summary>
        [FilterSerialize("functions")]
        Functions = 2,

        /// <summary>
        /// The resulting data set is only related to roles.
        /// </summary>
        [FilterSerialize("roles")]
        Roles = 3,

        /// <summary>
        /// The resulting data set is only related to organizations.
        /// </summary>
        [FilterSerialize("organizations")]
        Organization = 4,

        /// <summary>
        /// The resulting data set is only related to roles in functions.
        /// </summary>
        [FilterSerialize("functionRoles")]
        FunctionRoles = 5,

        /// <summary>
        /// The resulting data set is only related to organizations in functions.
        /// </summary>
        [FilterSerialize("functionOrgUnits")]
        FunctionOrgUnits = 6
    }
}
