namespace Maverick.UserProfileService.Models.EnumModels
{
    /// <summary>
    ///     A profile kind is used to identify a container profile. Either it is group or an organizational unit.
    /// </summary>
    public enum ProfileContainerType
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
        Organization = 2
    }
}
