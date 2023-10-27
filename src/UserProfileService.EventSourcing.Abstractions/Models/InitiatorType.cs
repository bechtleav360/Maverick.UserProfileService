namespace UserProfileService.EventSourcing.Abstractions.Models;

/// <summary>
///     Sets the type of initiator.
/// </summary>
public enum InitiatorType
{
    /// <summary>
    ///     A user was the initiator.
    /// </summary>
    User,

    /// <summary>
    ///     A service account was the initiator.
    /// </summary>
    ServiceAccount,

    /// <summary>
    ///     A system account was the initiator.
    /// </summary>
    System,

    /// <summary>
    ///     The initiator is unknown.
    /// </summary>
    Unknown
}
