using System.Net.Mime;
using MassTransit;

namespace UserProfileService.Messaging.Serialization;

/// <summary>
///     Factory for instances of <see cref="CloudEventMessageSerializer" />
/// </summary>
public class CloudEventMessageSerializerFactory : ISerializerFactory
{
    private readonly CloudEventMessageSerializer _serializer;

    /// <inheritdoc />
    public ContentType ContentType => _serializer.ContentType;
    
    /// <summary>
    ///     Create a new instance of <see cref="CloudEventMessageSerializerFactory" />
    /// </summary>
    /// <param name="nameFormatter">type-name formatter for all new messages</param>
    public CloudEventMessageSerializerFactory(IEndpointNameFormatter nameFormatter)
    {
        _serializer = new CloudEventMessageSerializer(nameFormatter);
    }

    /// <inheritdoc />
    public IMessageDeserializer CreateDeserializer()
    {
        return _serializer;
    }

    /// <inheritdoc />
    public IMessageSerializer CreateSerializer()
    {
        return _serializer;
    }
}
