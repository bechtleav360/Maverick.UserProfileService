using System;

namespace Maverick.UserProfileService.Models.EnumModels
{
    /// <summary>
    ///     Defines the profile kind that is requested.
    /// </summary>
    [Flags]
    public enum RequestedProfileKind
    {
        /// <summary>
        ///     No profile kind was defined.
        /// </summary>
        Undefined = 0,

        /// <summary>
        ///     Defines a user as a profile.
        /// </summary>
        User = 1,

        /// <summary>
        ///     Defines a group as a profile.
        /// </summary>
        Group = 2,

        /// <summary>
        ///     Defines a organization as a profile.
        /// </summary>
        Organization = 4,

        /// <summary>
        ///     All profiles can be request. Combine user, group and organization.
        /// </summary>
        All = User | Group | Organization
    }
}
