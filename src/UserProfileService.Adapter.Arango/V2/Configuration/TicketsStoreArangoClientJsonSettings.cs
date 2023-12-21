using System.Linq;
using Maverick.Client.ArangoDb.Public.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UserProfileService.Adapter.Arango.V2.Helpers;

namespace UserProfileService.Adapter.Arango.V2.Configuration;

/// <summary>
///     Represents JSON serializer settings specific to an ArangoDB client used in the context of the UPS ticket store.
///     Implements the <see cref="IArangoClientJsonSettings" /> interface.
/// </summary>
public class TicketsStoreArangoClientJsonSettings : IArangoClientJsonSettings
{
    /// <inheritdoc />
    public JsonSerializerSettings SerializerSettings { get; } = new JsonSerializerSettings
                                                                {
                                                                    Converters = WellKnownJsonConverters
                                                                        .GetDefaultTicketStoreConverters()
                                                                        .ToList(),
                                                                    ContractResolver = new DefaultContractResolver()
                                                                };
}