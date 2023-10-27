namespace UserProfileService.EventSourcing.Abstractions.Models;

/// <summary>
///     Response object returned by loading event operation.
/// </summary>
public class LoadEventResult
{
    /// <summary>
    ///     The actual event data body
    /// </summary>
    public object Data { get; set; }

    /// <summary>
    ///     Unique identifier for the event. Uses a sequential Guid
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     The sequential order of this event in the entire event store
    /// </summary>
    public long Sequence { get; set; }

    /// <summary>
    ///     If using Guid's for the stream identity, this will
    ///     refer to the Stream's Id, otherwise it will always be Guid.Empty
    /// </summary>
    public Guid StreamId { get; set; }

    /// <summary>
    ///     If using strings as the stream identifier, this will refer
    ///     to the containing Stream's Id
    /// </summary>
    public string? StreamKey { get; set; }

    /// <summary>
    ///     The UTC time that this event was originally captured
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    ///     The version of the stream this event reflects. The place in the stream.
    /// </summary>
    public long Version { get; set; }
}
