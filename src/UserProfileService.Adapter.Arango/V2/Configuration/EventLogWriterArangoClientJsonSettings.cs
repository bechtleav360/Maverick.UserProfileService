using System;
using System.Collections.Generic;
using JsonSubTypes;
using Maverick.Client.ArangoDb.Public.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using UserProfileService.Adapter.Arango.V2.Helpers;

namespace UserProfileService.Adapter.Arango.V2.Configuration;

/// <summary>
///     Represents JSON serializer settings specific to an ArangoDB client used in the context of the UPS event log writer.
///     Implements the <see cref="IArangoClientJsonSettings" /> interface.
/// </summary>
public class EventLogWriterArangoClientJsonSettings : IArangoClientJsonSettings
{
    public EventLogWriterArangoClientJsonSettings()
    {
        SerializerSettings = GetDefaultSettings();
    }

    public EventLogWriterArangoClientJsonSettings(Action<JsonSubtypesConverterBuilder> additionalRegistration)
    {
        SerializerSettings = GetDefaultSettings(additionalRegistration);
    }

    /// <inheritdoc />
    public virtual JsonSerializerSettings SerializerSettings { get; }

    private static JsonSerializerSettings GetDefaultSettings(
        Action<JsonSubtypesConverterBuilder> additionalRegistration = null) =>
        new JsonSerializerSettings
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
