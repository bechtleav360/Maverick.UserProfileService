namespace Maverick.UserProfileService.AggregateEvents.Common.Enums
{
    /// <summary>
    ///     A profile kind is used to identify an object that contains profiles.
    /// </summary>
    public enum ContainerType
    {
        /// <summary>
        ///     The kind of the container cannot be specified (i.e. because it is unknown).
        /// </summary>
        NotSpecified = 0,

        /// <summary>
        ///     Identifies group as a profile.
        /// </summary>
        Group = 1,

        /// <summary>
        ///     Identifies organization as a profile.
        /// </summary>
        Organization = 2,

        /// <summary>
        ///     Identifies a function.
        /// </summary>
        Function = 3,

        /// <summary>
        ///     Identifies a role.
        /// </summary>
        Role = 4
    }
}
