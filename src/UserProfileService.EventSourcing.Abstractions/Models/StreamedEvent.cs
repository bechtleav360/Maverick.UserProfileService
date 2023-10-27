namespace UserProfileService.EventSourcing.Abstractions.Models;

public class StreamedEvent
{
    //
    // Summary:
    //     Marten's name for the aggregate type that will be persisted to the streams table.
    //     This will only be available when running within the Async Daemon
    public string? AggregateTypeName { get; set; }

    //
    // Summary:
    //     Optional metadata describing the causation id
    public string? CausationId { get; set; }

    //
    // Summary:
    //     Optional metadata describing the correlation id
    public string? CorrelationId { get; set; }

    //
    // Summary:
    //     The actual event data body
    public object Data { get; }

    //
    // Summary:
    //     Marten's string representation of the event type in assembly qualified name
    public string DotNetTypeName { get; set; }

    //
    // Summary:
    //     The .Net type of the event body
    public Type EventType { get; }

    //
    // Summary:
    //     Marten's type alias string for the Event type
    public string EventTypeName { get; set; }

    //
    // Summary:
    //     Optional user defined metadata values. This may be null.
    public Dictionary<string, object>? Headers { get; set; }

    //
    // Summary:
    //     Unique identifier for the event. Uses a sequential Guid
    public Guid Id { get; set; }

    //
    // Summary:
    //     Has this event been archived and no longer applicable to projected views
    public bool IsArchived { get; set; }

    //
    // Summary:
    //     The sequential order of this event in the entire event store
    public long Sequence { get; set; }

    //
    // Summary:
    //     If using Guid's for the stream identity, this will refer to the Stream's Id,
    //     otherwise it will always be Guid.Empty
    public Guid StreamId { get; set; }

    //
    // Summary:
    //     If using strings as the stream identifier, this will refer to the containing
    //     Stream's Id
    public string? StreamKey { get; set; }

    //
    // Summary:
    //     If using multi-tenancy by tenant id
    public string TenantId { get; set; }

    //
    // Summary:
    //     The UTC time that this event was originally captured
    public DateTimeOffset Timestamp { get; set; }

    //
    // Summary:
    //     The version of the stream this event reflects. The place in the stream.
    public long Version { get; set; }
}
