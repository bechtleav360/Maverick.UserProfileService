using System.Diagnostics;
using System.Net.Mime;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Encodings.Web;
using System.Text.Json;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Core;
using CloudNative.CloudEvents.SystemTextJson;
using MassTransit;
using MassTransit.Initializers;
using MassTransit.Initializers.TypeConverters;
using MassTransit.Metadata;
using MassTransit.Serialization;
using MassTransit.Serialization.JsonConverters;

namespace UserProfileService.Messaging.Serialization;

/// <summary>
///     De-/Serializer for CloudEvent-Wrapped-Messages.
/// </summary>
public class CloudEventMessageSerializer :
    IMessageDeserializer,
    IMessageSerializer,
    IObjectDeserializer
{
    private readonly IEndpointNameFormatter _nameFormatter;

    /// <inheritdoc cref="IMessageSerializer.ContentType" />
    public ContentType ContentType { get; } = new ContentType(MimeUtilities.MediaType);

    /// <summary>
    ///     serialization-options used by all instances of this component.
    /// </summary>
    public JsonSerializerOptions Options { get; }

    /// <summary>
    ///     Create a new instance of <see cref="CloudEventMessageSerializer" />
    /// </summary>
    /// <param name="nameFormatter">type-name formatter for all new messages</param>
    public CloudEventMessageSerializer(IEndpointNameFormatter nameFormatter)
    {
        _nameFormatter = nameFormatter;

        GlobalTopology.MarkMessageTypeNotConsumable(typeof(JsonElement));

        Options = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReadCommentHandling = JsonCommentHandling.Skip,
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        Options.Converters.Add(new SystemTextJsonMessageDataConverter());
        Options.Converters.Add(new SystemTextJsonConverterFactory());
    }

    private T? GetObject<T>(JsonElement jsonElement)
        where T : class
    {
        if (typeof(T).GetTypeInfo().IsInterface && MessageTypeCache<T>.IsValidMessageType)
        {
            Type? messageType = TypeMetadataCache<T>.ImplementationType;

            if (jsonElement.Deserialize(messageType, Options) is T obj)
            {
                return obj;
            }
        }

        return jsonElement.Deserialize<T>(Options);
    }

    /// <inheritdoc />
    public ConsumeContext Deserialize(ReceiveContext receiveContext)
    {
        return new BodyConsumeContext(
            receiveContext,
            Deserialize(
                receiveContext.Body,
                receiveContext.TransportHeaders,
                receiveContext.InputAddress));
    }

    /// <inheritdoc />
    public SerializerContext Deserialize(MessageBody body, Headers headers, Uri? destinationAddress = null)
    {
        try
        {
            CloudEvent cloudEvent = new JsonEventFormatter().DecodeStructuredModeMessage(
                body.GetStream(),
                new ContentType(MimeUtilities.MediaType),
                Array.Empty<CloudEventAttribute>());

            MessageEnvelope envelope = new CloudEventMessageEnvelope(cloudEvent);

            var messageContext = new EnvelopeMessageContext(envelope, this);

            string[] messageTypes = envelope.MessageType ?? Array.Empty<string>();

            var serializerContext = new CloudEventMessageSerializerContext(
                this,
                Options,
                ContentType,
                messageContext,
                messageTypes,
                _nameFormatter,
                envelope);

            return serializerContext;
        }
        catch (SerializationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new SerializationException("An error occured while deserializing the message envelope", ex);
        }
    }

    /// <inheritdoc />
    public MessageBody GetMessageBody(string text)
    {
        return new StringMessageBody(text);
    }

    /// <inheritdoc />
    public MessageBody GetMessageBody<T>(SendContext<T> context)
        where T : class
    {
        try
        {
            context.ContentType = new ContentType(MimeUtilities.MediaType);

            var envelope = new CloudEvent
            {
                Id = context.MessageId?.ToString(),
                Source = context.SourceAddress,
                Type = "urn:message:" + _nameFormatter.Message<T>(),
                Time = context.SentTime,
                Data = context.Message,
                DataContentType = MediaTypeNames.Application.Json
            };

            envelope["traceparent"] = Activity.Current?.Id ?? string.Empty;
            envelope["conversation"] = context.ConversationId?.ToString("N") ?? string.Empty;

            envelope["request"] = context.RequestId?.ToString("N") ?? string.Empty;
            envelope["correlation"] = context.CorrelationId?.ToString("N") ?? string.Empty;

            envelope["destination"] = context.DestinationAddress?.ToString() ?? string.Empty;
            envelope["response"] = context.ResponseAddress?.ToString() ?? string.Empty;
            envelope["fault"] = context.FaultAddress?.ToString() ?? string.Empty;

            envelope["conversation"] = context.ConversationId?.ToString("N") ?? string.Empty;
            envelope["initiator"] = context.InitiatorId?.ToString("N") ?? string.Empty;

            envelope["expiration"] = context.TimeToLive is not null
                ? (context.SentTime + context.TimeToLive)?.ToString("O") ?? string.Empty
                : string.Empty;

            var cloudFormatter = new JsonEventFormatter();
            ReadOnlyMemory<byte> messageBody = cloudFormatter.EncodeStructuredModeMessage(envelope, out ContentType _);

            return new MemoryMessageBody(messageBody);
        }
        catch (SerializationException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new SerializationException("Failed to serialize message", e);
        }
    }

    /// <inheritdoc />
    public T? DeserializeObject<T>(object? value, T? defaultValue = default)
        where T : class
    {
        switch (value)
        {
            case null:
                return defaultValue;

            case T returnValue:
                return returnValue;

            case string text when TypeConverterCache.TryGetTypeConverter(out ITypeConverter<T, string>? typeConverter)
                && typeConverter.TryConvert(text, out T? result):
                return result;

            case string text:
                return GetObject<T>(JsonSerializer.Deserialize<JsonElement>(text));

            case JsonElement jsonElement:
                return GetObject<T>(jsonElement);
        }

        JsonElement element = JsonSerializer.SerializeToElement(value, Options);

        return element.ValueKind == JsonValueKind.Null
            ? defaultValue
            : GetObject<T>(element);
    }

    /// <inheritdoc />
    public T? DeserializeObject<T>(object? value, T? defaultValue = null)
        where T : struct
    {
        switch (value)
        {
            case null:
                return defaultValue;

            case T returnValue:
                return returnValue;

            case string text when TypeConverterCache.TryGetTypeConverter(out ITypeConverter<T, string>? typeConverter)
                && typeConverter.TryConvert(text, out T result):
                return result;

            case string text:
                return JsonSerializer.Deserialize<T>(text, Options);

            case JsonElement jsonElement:
                return jsonElement.Deserialize<T>(Options);
        }

        JsonElement element = JsonSerializer.SerializeToElement(value, Options);

        return element.ValueKind == JsonValueKind.Null
            ? defaultValue
            : element.Deserialize<T>(Options);
    }

    /// <inheritdoc />
    public MessageBody SerializeObject(object? value)
    {
        // used by mt saga-repositories and not much else(?!) docs are pretty sparse here
        // normal serialization should be handled by GetMessageBody-overloads
        // ---
        // redis-repository seems to call .GetString() on this body which is pretty fucking stupid
        // and leads to DeserializeObject<T> getting
        // "System.ReadOnlyMemory<Byte>[426]" when the message-body doesn't properly handle this case.
        return new SystemTextJsonObjectMessageBody(
            value ?? throw new ArgumentNullException(nameof(value)),
            Options);
    }

    /// <inheritdoc />
    public void Probe(ProbeContext context)
    {
        ProbeContext scope = context.CreateScope("json");
        scope.Add("contentType", ContentType.MediaType);
        scope.Add("provider", "System.Text.Json");
    }
}
