namespace UserProfileService.EventSourcing.Abstractions.Models;

/// <summary>
///     Information about a the origins and content of a previously written event
/// </summary>
public class StreamedEventHeader
{
    /// <summary>
    ///     A datetime representing when this event was created in the system
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    ///     The Unique Identifier representing this event
    /// </summary>
    public Guid EventId { get; set; }

    /// <summary>
    ///     The commit position of the current event record.
    /// </summary>
    public long EventNumberSequence { get; set; }

    /// <summary>
    ///     The number of the current event (related to <see cref="StreamId" />).
    /// </summary>
    public long EventNumberVersion { get; set; }

    /// <summary>
    ///     The Event Stream that this event belongs to
    /// </summary>
    public string EventStreamId { get; set; }

    /// <summary>
    ///     The type of event this is
    /// </summary>
    public string EventType { get; set; }

    /// <summary>
    ///     The name of the stream that this event is received from.
    /// </summary>
    public string StreamId { get; set; }
}
