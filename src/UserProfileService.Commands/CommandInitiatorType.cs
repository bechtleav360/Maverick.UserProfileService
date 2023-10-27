namespace UserProfileService.Commands;

/// <summary>
///     Types of initiator.
/// </summary>
public enum CommandInitiatorType
{
    /// <summary>
    ///     Initiator is a user.
    /// </summary>
    User,

    /// <summary>
    ///     Initiator is a service account.
    /// </summary>
    ServiceAccount,

    /// <summary>
    ///     Initiator is a ups system.
    /// </summary>
    System,

    /// <summary>
    ///     Initiator is not known.
    /// </summary>
    Unknown
}
