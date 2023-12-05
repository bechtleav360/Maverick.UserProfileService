using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UserProfileService.Adapter.Arango.V2.Helpers;
using UserProfileService.Projection.Common.Converters;

namespace UserProfileService.Saga.Worker.Utilities;

public static class SagaWorkerConverter
{
    public static IList<JsonConverter> GetAllConvertersForMartenProjections()
    {
        return new List<JsonConverter>
        {
            WellKnownSecondLevelConverter.GetSecondLevelDefaultConverters(),
            WellKnownProjectionJsonConverters.DefaultFunctionConverter,
            WellKnownProjectionJsonConverters.DefaultProfileConverter,
            new StringEnumConverter()
        };
    }
}
