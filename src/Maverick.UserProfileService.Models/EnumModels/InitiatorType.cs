namespace Maverick.UserProfileService.Models.EnumModels
{
    /// <summary>
    ///     Specify the type of initiators.
    /// </summary>
    public enum InitiatorType
    {
        /// <summary>
        ///     The initiator is a user.
        /// </summary>
        User,

        /// <summary>
        ///     The initiator is a service account.
        /// </summary>
        ServiceAccount,

        /// <summary>
        ///     The initiator is a system like the Worker, Synchronization.
        /// </summary>
        System,

        /// <summary>
        ///     The initiator is not defined.
        /// </summary>
        Unknown
    }
}
