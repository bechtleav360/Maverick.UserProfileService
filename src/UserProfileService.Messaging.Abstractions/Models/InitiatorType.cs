namespace UserProfileService.Messaging.Abstractions.Models;
/// <summary>
///     Represents the initiator of a certain action.
/// </summary>
public enum InitiatorType
{
    /// <summary>
    ///     Manually initiated by a user.
    /// </summary>
    User,
    /// <summary>
    ///     Initiated by a service account.
    /// </summary>
    ServiceAccount,
    /// <summary>
    ///     Initiated by the system.
    /// </summary>
    System,
    /// <summary>
    ///     Unspecified initiator.
    /// </summary>
    Unknown
}
