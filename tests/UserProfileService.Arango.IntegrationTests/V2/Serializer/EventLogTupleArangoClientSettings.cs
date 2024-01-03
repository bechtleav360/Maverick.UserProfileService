using System.Linq;
using Maverick.Client.ArangoDb.Public.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UserProfileService.Adapter.Arango.V2.Helpers;

namespace UserProfileService.Arango.IntegrationTests.V2.Serializer;

public class EventLogTupleArangoClientSettings : IArangoClientJsonSettings
{
    public JsonSerializerSettings SerializerSettings { get; set; } = new JsonSerializerSettings
                                                                     {
                                                                         Converters = WellKnownJsonConverters
                                                                             .GetDefaultProfileConverters()
                                                                             .Append(new StringEnumConverter())
                                                                             .Append(
                                                                                 new
                                                                                     EventLogTupleReadOnlyMemoryJsonConverter())
                                                                             .Append(
                                                                                 new EventLogIgnoreEventJsonConverter())
                                                                             .ToList(),
                                                                         NullValueHandling = NullValueHandling.Ignore
                                                                     };
}
