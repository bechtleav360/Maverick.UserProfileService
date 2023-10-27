using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace UserProfileService.Projection.FirstLevel.Tests.Extensions
{
    internal static class EnumerableExtensions
    {
        internal static JArray ToJArray<T>(this IEnumerable<T> list)
        {
            return JArray.FromObject(list);
        }
    }
}
