using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UserProfileService.Adapter.Arango.V2.Helpers;
using UserProfileService.Projection.Common.Converters;

namespace UserProfileService.Saga.Worker.Utilities;

/// <summary>
///     Provides a collection of JSON converters for Marten projections.
/// </summary>
public static class SagaWorkerConverter
{
    /// <summary>
    ///     Gets all the converters for Marten projections.
    /// </summary>
    /// <returns>A list of <see cref="JsonConverter"/> instances.</returns>
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
