namespace Maverick.UserProfileService.AggregateEvents.Common.Enums
{
    /// <summary>
    ///     Specifies the type of a tag.
    /// </summary>
    public enum TagType
    {
        /// <summary>
        ///     Security tags will be used to determine permissions.
        /// </summary>
        Security,

        /// <summary>
        ///     Custom Tags will be used to mark without any functional impact.
        /// </summary>
        Custom,

        /// <summary>
        ///     In order to get rid of the facade, the FunctionalAccessRights (of the client) are mow set using tags.
        /// </summary>
        FunctionalAccessRights,

        /// <summary>
        ///     In order to get rid of the V1 facade, color information (that are usually inheritable) are stored as tags.
        /// </summary>
        Color
    }
}
