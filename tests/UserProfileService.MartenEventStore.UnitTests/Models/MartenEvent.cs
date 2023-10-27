using Marten.Events;

namespace UserProfileService.MartenEventStore.UnitTests.Models;

internal class MartenEvent : IEvent
{
    public string? AggregateTypeName { get; set; }
    public string? CausationId { get; set; }
    public string? CorrelationId { get; set; }
    public object Data { get; set; } = new object();
    public string DotNetTypeName { get; set; } = string.Empty;
    public Type EventType { get; set; } = null!; // suppress nullable warning -> not used
    public string EventTypeName { get; set; } = string.Empty;
    public Dictionary<string, object>? Headers { get; set; }

    public Guid Id { get; set; }
    public bool IsArchived { get; set; }
    public long Sequence { get; set; }
    public Guid StreamId { get; set; }
    public string? StreamKey { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
    public long Version { get; set; }

    public void SetHeader(string key, object value)
    {
        throw new NotImplementedException();
    }

    public object? GetHeader(string key)
    {
        throw new NotImplementedException();
    }
}
