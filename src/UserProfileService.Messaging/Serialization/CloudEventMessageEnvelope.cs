using CloudNative.CloudEvents;
using MassTransit;
using MassTransit.Serialization;

namespace UserProfileService.Messaging.Serialization;

/// <summary>
///     Implementation of <see cref="MessageEnvelope" /> using CloudEvents as envelope.
/// </summary>
public class CloudEventMessageEnvelope : MessageEnvelope
{
    /// <inheritdoc />
    public string? ConversationId { get; }

    /// <inheritdoc />
    public string? CorrelationId { get; }

    /// <inheritdoc />
    public string? DestinationAddress { get; }

    /// <inheritdoc />
    public DateTime? ExpirationTime { get; }

    /// <inheritdoc />
    public string? FaultAddress { get; }

    /// <inheritdoc />
    public Dictionary<string, object?>? Headers { get; }

    /// <inheritdoc />
    public HostInfo? Host { get; }

    /// <inheritdoc />
    public string? InitiatorId { get; }

    /// <inheritdoc />
    public object? Message { get; }

    /// <inheritdoc />
    public string? MessageId { get; }

    /// <inheritdoc />
    public string[]? MessageType { get; }

    /// <inheritdoc />
    public string? RequestId { get; }

    /// <inheritdoc />
    public string? ResponseAddress { get; }

    /// <inheritdoc />
    public DateTime? SentTime { get; }

    /// <inheritdoc />
    public string? SourceAddress { get; }

    /// <summary>
    ///     Create a new instance of <see cref="CloudEventMessageEnvelope" />
    /// </summary>
    /// <param name="cloudEvent">CloudEvent-Envelope to take data from.</param>
    public CloudEventMessageEnvelope(CloudEvent cloudEvent)
    {
        ConversationId = cloudEvent["conversation"] as string;
        CorrelationId = cloudEvent["correlation"] as string;
        DestinationAddress = cloudEvent["destination"] as string;
        FaultAddress = cloudEvent["fault"] as string;

        Headers = cloudEvent.GetPopulatedAttributes()
            .ToDictionary(
                kvp => kvp.Key.Name,
                kvp => (object?)kvp.Value);

        InitiatorId = cloudEvent["initiator"] as string;
        Message = cloudEvent.Data;
        MessageId = cloudEvent.Id;
        MessageType = new[] { cloudEvent.Type ?? string.Empty };
        ResponseAddress = cloudEvent["response"] as string;
        RequestId = cloudEvent["message"] as string;
        SentTime = cloudEvent.Time?.UtcDateTime;
        SourceAddress = cloudEvent.Source?.ToString();

        if (DateTime.TryParse(
                cloudEvent["expiration"] as string ?? string.Empty,
                out DateTime expiration))
        {
            ExpirationTime = expiration;
        }
    }
}
