using System.Net.Mime;
using System.Text.Json;
using MassTransit;
using MassTransit.Serialization;

namespace UserProfileService.Messaging.Serialization;

/// <summary>
///     Custom-Implementation of <see cref="SerializerContext" /> based on <see cref="SystemTextJsonSerializerContext" />.
///     Uses custom URN instead of those provided by <see cref="MessageUrn" />.
/// </summary>
public class CloudEventMessageSerializerContext : SystemTextJsonSerializerContext
{
    private readonly IEndpointNameFormatter _nameFormatter;

    /// <inheritdoc />
    public CloudEventMessageSerializerContext(
        IObjectDeserializer objectDeserializer,
        JsonSerializerOptions options,
        ContentType contentType,
        MessageContext messageContext,
        string[] messageTypes,
        IEndpointNameFormatter nameFormatter,
        MessageEnvelope? envelope = null,
        object? message = null)
        : base(
            objectDeserializer,
            options,
            contentType,
            messageContext,
            messageTypes,
            envelope,
            message)
    {
        _nameFormatter = nameFormatter;
    }

    /// <inheritdoc />
    public override bool IsSupportedMessageType<T>()
    {
        string typeUrn = "urn:message:" + _nameFormatter.Message<T>();

        return SupportedMessageTypes.Any(name => typeUrn.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
}
