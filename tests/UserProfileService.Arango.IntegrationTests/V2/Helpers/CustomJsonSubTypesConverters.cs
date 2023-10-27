using JsonSubTypes;
using Maverick.UserProfileService.AggregateEvents.Common;
using Newtonsoft.Json;
using UserProfileService.Arango.IntegrationTests.V2.ArangoEventLogStore.Models;

namespace UserProfileService.Arango.IntegrationTests.V2.Helpers;

public static class CustomJsonSubTypesConverters
{
    public static JsonConverter[] TestUserProfileServiceEventConverters =>
        new[]
        {
            JsonSubtypesConverterBuilder.Of<IUserProfileServiceEvent>(nameof(IUserProfileServiceEvent.Type))
                .RegisterSubtype<TestEvent>(nameof(TestEvent))
                .Build()
        };
}
