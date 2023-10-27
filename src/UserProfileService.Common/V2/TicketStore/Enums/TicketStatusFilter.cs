namespace UserProfileService.Common.V2.TicketStore.Enums;

public enum TicketStatusFilter
{
    /// <summary>
    ///     All tickets that are in a pending state.
    /// </summary>
    Pending,

    /// <summary>
    ///     All tickets that have been completed without an error.
    /// </summary>
    Complete,

    /// <summary>
    ///     All tickets that are in an failure state.
    /// </summary>
    Failure,

    /// <summary>
    ///     All tickets that are in a finished state - either complete or in failure.
    /// </summary>
    Finished,

    /// <summary>
    ///     All tickets
    /// </summary>
    All
}
