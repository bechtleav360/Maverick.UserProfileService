using Maverick.Client.ArangoDb.Public.Configuration;
using Newtonsoft.Json;

namespace UserProfileService.Adapter.Arango.V2.Configuration;

/// <summary>
///     Represents JSON serializer settings specific to an ArangoDB client used in the context of the UPS event collector.
///     Implements the <see cref="IArangoClientJsonSettings" /> interface.
/// </summary>
public class EventCollectorArangoClientJsonSettings : IArangoClientJsonSettings
{
    /// <inheritdoc />
    public JsonSerializerSettings SerializerSettings { get; } = new JsonSerializerSettings();
}