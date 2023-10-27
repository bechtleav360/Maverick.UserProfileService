using System;

namespace Maverick.UserProfileService.Models.EnumModels
{
    /// <summary>
    ///     Defines the tag type that is requested.
    /// </summary>
    [Flags]
    public enum RequestedTagType
    {
        /// <summary>
        ///     Security tags will be used to determine permissions.
        /// </summary>
        Security = 0,

        /// <summary>
        ///     Custom Tags will be used to mark without any functional impact.
        /// </summary>
        Custom = 1,

        /// <summary>
        ///     In order to get rid of the facade, the FunctionalAccessRights (of the client) are mow set using tags.
        /// </summary>
        FunctionalAccessRights = 2,

        /// <summary>
        ///     In order to get rid of the V1 facade, color information (that are usually inheritable) are stored as tags.
        /// </summary>
        Color = 4,

        /// <summary>
        ///     All profiles can be request. Combine user, group and organization.
        /// </summary>
        All = Security | Custom | FunctionalAccessRights | Color
    }
}
