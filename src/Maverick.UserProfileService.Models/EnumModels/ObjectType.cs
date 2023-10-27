namespace Maverick.UserProfileService.Models.EnumModels
{
    /// <summary>
    ///     Specifies the Type of UPS-Object.
    /// </summary>
    public enum ObjectType
    {
        /// <summary>
        ///     The targeted Object is unknown.
        /// </summary>
        Unknown,

        /// <summary>
        ///     The targeted Object is a Profile (User, Group or Organization).
        /// </summary>
        Profile,

        /// <summary>
        ///     The targeted Object is a Role.
        /// </summary>
        Role,

        /// <summary>
        ///     The targeted Object is a Function.
        /// </summary>
        Function,

        /// <summary>
        ///     The targeted Object is a Group.
        /// </summary>
        Group,

        /// <summary>
        ///     The targeted Object is a User.
        /// </summary>
        User,

        /// <summary>
        ///     The targeted Object is a Organization.
        /// </summary>
        Organization,

        /// <summary>
        ///     The targeted Object is a Tag.
        /// </summary>
        Tag
    }
}
