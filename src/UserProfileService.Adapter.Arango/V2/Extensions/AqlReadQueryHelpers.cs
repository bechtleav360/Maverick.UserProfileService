using System;
using System.Collections.Generic;
using UserProfileService.Adapter.Arango.V2.Contracts;

namespace UserProfileService.Adapter.Arango.V2.Extensions;

internal static class AqlReadQueryHelpers
{
    internal static Dictionary<string, Func<string, string>> MethodNameToAqlMethodDefinitionMapping { get; }
        = new Dictionary<string, Func<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            { WellKnownFilterProperties.CountProperty, inner => $"COUNT({inner})" }
        };

    internal static string GetModifiedAqlString(
        this string originalProperty,
        string methodName)
    {
        if (string.IsNullOrWhiteSpace(originalProperty))
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(methodName)
            || !MethodNameToAqlMethodDefinitionMapping.ContainsKey(methodName))
        {
            return originalProperty;
        }

        return MethodNameToAqlMethodDefinitionMapping[methodName].Invoke(originalProperty);
    }

    internal static string AdjustCurrentTerms(this string original)
    {
        if (string.IsNullOrWhiteSpace(original) || original.TrimStart('(').StartsWith("CURRENT"))
        {
            return original;
        }

        return $"CURRENT.{original}";
    }
}
