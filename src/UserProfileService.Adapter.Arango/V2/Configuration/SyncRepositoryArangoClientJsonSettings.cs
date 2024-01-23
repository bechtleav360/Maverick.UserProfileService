using System;
using System.Collections.Generic;
using JsonSubTypes;
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
    /// <summary>
    ///     Creates an object of type <see cref="SyncRepositoryArangoClientJsonSettings" />.
    /// </summary>
    public SyncRepositoryArangoClientJsonSettings()
    {
        SerializerSettings = GetDefaultSettings();
    }

    /// <summary>
    ///     Creates an object of type <see cref="SyncRepositoryArangoClientJsonSettings" />.
    /// </summary>
    /// <param name="additionalRegistration">An function to extend the json converters.</param>
    public SyncRepositoryArangoClientJsonSettings(Action<JsonSubtypesConverterBuilder> additionalRegistration)
    {
        SerializerSettings = GetDefaultSettings(additionalRegistration);
    }

    /// <inheritdoc />
    public virtual JsonSerializerSettings SerializerSettings { get; }

    private static JsonSerializerSettings GetDefaultSettings(
        Action<JsonSubtypesConverterBuilder> additionalRegistration = null)
    {
        return new JsonSerializerSettings
               {
                   Converters = new List<JsonConverter>
                                {
                                    new StringEnumConverter(),
                                    WellKnownSecondLevelConverter
                                        .GetSecondLevelDefaultConverters(additionalRegistration)
                                },
                   ContractResolver = new DefaultContractResolver(),
                   NullValueHandling = NullValueHandling.Ignore
               };
    }
}
