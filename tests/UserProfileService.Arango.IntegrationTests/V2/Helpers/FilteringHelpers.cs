using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Maverick.UserProfileService.Models.Annotations;

namespace UserProfileService.Arango.IntegrationTests.V2.Helpers
{
    internal static class FilteringHelpers
    {
        internal static bool MatchProperties(
            this object obj,
            string searchString)
        {
            List<PropertyInfo> relevantProperties = obj.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(
                    p => p.GetCustomAttributes(true)
                            ?
                            .Any(a => a.GetType() == typeof(SearchableAttribute))
                        == true)
                .ToList();

            return relevantProperties.Count > 0
                && relevantProperties.Any(
                    prop =>
                        (prop.GetValue(obj) as string)?
                        .Contains(searchString, StringComparison.OrdinalIgnoreCase)
                        == true);
        }
    }
}
