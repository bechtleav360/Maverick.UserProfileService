using System.Linq;
using Maverick.Client.ArangoDb.Public.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UserProfileService.Adapter.Arango.V2.Helpers;

namespace UserProfileService.Adapter.Arango.V2.Configuration;

/// <summary>
///     Represents JSON serializer settings specific to an ArangoDB client used in the context of the UPS profile store.
///     Implements the <see cref="IArangoClientJsonSettings" /> interface.
/// </summary>
public class ProfileStoreArangoClientJsonSettings : IArangoClientJsonSettings
{
    /// <inheritdoc />
    public JsonSerializerSettings SerializerSettings { get; } = new JsonSerializerSettings
                                                                {
                                                                    Converters = WellKnownJsonConverters
                                                                        .GetDefaultProfileConverters()
                                                                        .ToList(),
                                                                    ContractResolver = new DefaultContractResolver()
                                                                };
}