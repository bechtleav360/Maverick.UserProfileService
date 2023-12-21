using System.Linq;
using Maverick.Client.ArangoDb.Public.Configuration;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UserProfileService.Adapter.Arango.V2.Helpers;

namespace UserProfileService.Adapter.Arango.V2.Configuration;

/// <summary>
///     Represents JSON serializer settings specific to an ArangoDB client used in the context of the UPS sync repository.
///     Implements the <see cref="IArangoClientJsonSettings" /> interface.
/// </summary>
public class SyncRepositoryArangoClientJsonSettings : IArangoClientJsonSettings
{
    /// <inheritdoc />
    public JsonSerializerSettings SerializerSettings { get; } = new JsonSerializerSettings
                                                                {
                                                                    Converters = WellKnownJsonConverters.GetDefaultProfileConverters()
                                                                        .Append(new StringEnumConverter())
                                                                        .ToList(),
                                                                    ContractResolver = new DefaultContractResolver()
                                                                };
}
