using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UserProfileService.Adapter.Arango.V2.Helpers;
using UserProfileService.Projection.Common.Converters;

namespace UserProfileService.Saga.Worker.Utilities;

internal static class SagaWorkerConverter
{
    internal static IList<JsonConverter> GetAllConvertersForMartenProjections()
    {
        return new List<JsonConverter>
        {
            WellKnownSecondLevelConverter.GetSecondLevelDefaultConverters(),
            WellKnownJsonConverters.DefaultFunctionConverter,
            WellKnownJsonConverters.DefaultProfileConverter,
            new StringEnumConverter()
        };
    }
}
