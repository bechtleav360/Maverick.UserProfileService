using Newtonsoft.Json;

namespace Maverick.Client.ArangoDb.Public.Configuration;

/// <summary>
///     Represents the default implementation of <see cref="IArangoClientJsonSettings" /> that contains only default
///     <see cref="JsonSerializerSettings" /> values.
/// </summary>
public class DefaultArangoClientJsonSettings : IArangoClientJsonSettings
{
    /// <summary>
    ///     Gets or sets the serializer settings for JSON serialization and deserialization.
    /// </summary>
    public JsonSerializerSettings SerializerSettings { get; } = new JsonSerializerSettings();
}
