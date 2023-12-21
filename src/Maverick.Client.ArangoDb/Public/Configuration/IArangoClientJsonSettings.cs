using Newtonsoft.Json;

namespace Maverick.Client.ArangoDb.Public.Configuration;

/// <summary>
///     Contains JSON serializer settings for the ArangoDb client.
/// </summary>
public interface IArangoClientJsonSettings
{
    /// <summary>
    ///     Gets the serializer settings for JSON serialization and deserialization.
    /// </summary>
    JsonSerializerSettings SerializerSettings { get; }
}
