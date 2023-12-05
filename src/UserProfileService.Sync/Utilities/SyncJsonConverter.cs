using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UserProfileService.Adapter.Arango.V2.Helpers;
using UserProfileService.Projection.Common.Converters;

namespace UserProfileService.Sync.Utilities;

internal static class SyncJsonConverter
{
    internal static IList<JsonConverter> GetAllConvertersForMartenProjections()
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
