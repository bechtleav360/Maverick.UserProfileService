namespace UserProfileService.Common.V2.Enums;

/// <summary>
///     Status of the event/batch while execute batch of events.
/// </summary>
public enum EventStatus
{
    /// <summary>
    ///     Event/Batch was initialized and not yet written to the event store.
    /// </summary>
    Initialized,

    /// <summary>
    ///     Event/Batch has been committed and is ready to be written to the event store.
    /// </summary>
    Committed,

    /// <summary>
    ///     Event/Batch has been committed and is ready to be written to the event store.
    /// </summary>
    Processing,

    /// <summary>
    ///     Event/Batch was successfully written to the event store.
    /// </summary>
    Executed,

    /// <summary>
    ///     Event/Batch was not written to the event store because the batch was aborted early.
    /// </summary>
    Aborted,

    /// <summary>
    ///     The Event/Batch was not written to the event store,
    ///     because an error occurred while writing to the event store.
    /// </summary>
    Error
}
