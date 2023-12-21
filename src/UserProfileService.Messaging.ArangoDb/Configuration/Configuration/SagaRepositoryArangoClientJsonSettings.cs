using System.Collections.Generic;
using Maverick.Client.ArangoDb.Public.Configuration;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UserProfileService.Messaging.ArangoDb.Configuration.Configuration;

/// <summary>
///     Represents JSON serializer settings specific to an ArangoDB client used in the context of the UPS Saga repository.
///     Implements the <see cref="IArangoClientJsonSettings" /> interface.
/// </summary>
public class SagaRepositoryArangoClientJsonSettings : IArangoClientJsonSettings
{
    /// <inheritdoc />
    public JsonSerializerSettings SerializerSettings { get; } = new JsonSerializerSettings
                                                                {
                                                                    Converters = new List<JsonConverter>
                                                                        {
                                                                            new StringEnumConverter()
                                                                        },
                                                                    ContractResolver = new DefaultContractResolver()
                                                                };
}