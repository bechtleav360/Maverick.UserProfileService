namespace UserProfileService.Common.V2.TicketStore.Enums;

/// <summary>
///     Represents the status of a ticket.
/// </summary>
public enum TicketStatus
{
    /// <summary>
    ///     The ticket is in a pending state.
    /// </summary>
    Pending = 0,

    /// <summary>
    ///     The ticket is completed without an error.
    /// </summary>
    Complete = 1,

    /// <summary>
    ///     An error occurred and the ticket is in a failure state.
    /// </summary>
    Failure = 2
}
