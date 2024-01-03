using System.Linq;
using JsonSubTypes;
using Maverick.Client.ArangoDb.Public.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UserProfileService.Adapter.Arango.V2.Helpers;
using UserProfileService.Arango.IntegrationTests.V2.Implementations;
using UserProfileService.Common.V2.TicketStore.Abstractions;

namespace UserProfileService.Arango.IntegrationTests.V2.Serializer;

public class FirstLevelProjectionTestArangoClientSettings: IArangoClientJsonSettings
{
    public JsonSerializerSettings SerializerSettings { get; set; } = new JsonSerializerSettings
                                                                     {
                                                                         Converters = WellKnownJsonConverters
                                                                             .GetDefaultFirstLevelProjectionConverters()
                                                                             .Append(  JsonSubtypesConverterBuilder
                                                                                 .Of<TicketBase>(nameof(TicketBase.Type))
                                                                                 .RegisterSubtype<TicketA>("TicketA")
                                                                                 .RegisterSubtype<TicketB>("TicketB")
                                                                                 .Build())
                                                                             .Concat(WellKnownJsonConverters.GetDefaultProfileConverters())
                                                                             .ToList(),
                                                                         ContractResolver =
                                                                             new DefaultContractResolver()
                                                                     };
}
