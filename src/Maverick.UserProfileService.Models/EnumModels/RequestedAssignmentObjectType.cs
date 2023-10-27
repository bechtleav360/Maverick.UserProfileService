using System;

namespace Maverick.UserProfileService.Models.EnumModels
{
    /// <summary>
    ///     Defines the assignment object type that is requested.
    /// </summary>
    [Flags]
    public enum RequestedAssignmentObjectType
    {
        /// <summary>
        ///     Should not be used.
        /// </summary>
        Undefined = 0,

        /// <summary>
        ///     Identifies the role as a normal role.
        /// </summary>
        Role = 1,

        /// <summary>
        ///     Identifies the role as a function.
        /// </summary>
        Function = 2,

        /// <summary>
        ///     Both types are requested.
        /// </summary>
        All = Role | Function
    }
}
