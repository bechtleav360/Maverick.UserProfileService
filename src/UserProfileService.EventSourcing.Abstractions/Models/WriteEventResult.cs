namespace UserProfileService.EventSourcing.Abstractions.Models;

/// <summary>
///     Response object returned after writing Event operation.
/// </summary>
public class WriteEventResult
{
    /// <summary>
    ///     The version of the stream after writing to it.
    /// </summary>
    public long CurrentVersion { get; set; }

    /// <summary>
    ///     Id of the written event.
    /// </summary>
    public Guid? EventId { get; set; }

    /// <summary>
    ///     The sequential order of this event in the entire event store.
    /// </summary>
    public long? Sequence { get; set; }
}
