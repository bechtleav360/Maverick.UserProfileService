using System.Collections;
using Newtonsoft.Json.Linq;

namespace UserProfileService.Projection.SecondLevel.UnitTests.Extensions;

internal static class EnumerableExtensions
{
    internal static JArray ToJArray(this IEnumerable list)
    {
        return JArray.FromObject(list);
    }
}