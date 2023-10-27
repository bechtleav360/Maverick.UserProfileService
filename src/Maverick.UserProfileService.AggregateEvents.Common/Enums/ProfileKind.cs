namespace Maverick.UserProfileService.AggregateEvents.Common.Enums
{
    /// <summary>
    ///     A profile kind is used to identify a profile. Either it is group or a user or an organizational unit.
    /// </summary>
    public enum ProfileKind
    {
        /// <summary>
        ///     Profile could not be defined.
        /// </summary>
        Unknown = 0,

        /// <summary>
        ///     Identifies user as a profile.
        /// </summary>
        User = 1,

        /// <summary>
        ///     Identifies group as a profile.
        /// </summary>
        Group = 2,

        /// <summary>
        ///     Identifies organization as a profile.
        /// </summary>
        Organization = 3
    }
}
