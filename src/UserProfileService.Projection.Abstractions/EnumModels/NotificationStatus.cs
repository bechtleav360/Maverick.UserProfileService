using System;

namespace UserProfileService.Projection.Abstractions.EnumModels;

/// <summary>
///     Set the status if the assignments has been notified
///     when it has been activated/deactivated.
/// </summary>
[Flags]
public enum NotificationStatus
{
    /// <summary>
    ///     The assignment has not been activated.
    /// </summary>
    NoneSent,

    /// <summary>
    ///     The assignment has been notified that it was
    ///     activated.
    /// </summary>
    ActivationSent,

    /// <summary>
    ///     The assignment has been notified that it was
    ///     deactivated.
    /// </summary>
    DeactivationSent,

    /// <summary>
    ///     The assignment has been notified that it was
    ///     activated and deactivated.
    /// </summary>
    BothSent = ActivationSent | DeactivationSent
}
