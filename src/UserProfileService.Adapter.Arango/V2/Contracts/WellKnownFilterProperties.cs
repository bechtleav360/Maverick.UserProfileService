using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UserProfileService.Adapter.Arango.V2.Contracts;

/// <summary>
///     Contains names/information about well-known properties used in filter queries.
/// </summary>
internal static class WellKnownFilterProperties
{
    internal const string CountProperty = "Count";

    internal const string LengthProperty = "Length";

    /// <summary>
    ///     Contains a property name-to-method info mapping, about the method that will resolve the requested property name.
    /// </summary>
    internal static Dictionary<string, MethodInfo> EnumerablePropertyMapping { get; }
        = new Dictionary<string, MethodInfo>(StringComparer.OrdinalIgnoreCase)
        {
            { CountProperty, GetMethodInfoOfCount() },
            { LengthProperty, GetMethodInfoOfCount() }
        };

    private static MethodInfo GetMethodInfoOfCount()
    {
        return typeof(Enumerable)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(Enumerable.Count) && m.GetParameters().Length == 1);
    }
}
