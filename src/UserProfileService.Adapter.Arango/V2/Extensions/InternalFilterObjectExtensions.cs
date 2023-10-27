using System.Collections.Generic;
using System.Linq;
using Maverick.UserProfileService.Models.Models;

namespace UserProfileService.Adapter.Arango.V2.Extensions;

internal static class InternalFilterObjectExtensions
{
    internal static bool AllTagsIncludedIn(this IList<string> first, IList<CalculatedTag> second)
    {
        return first == null || (second != null && first.All(t1 => second.Any(t2 => t1 == t2.Name)));
    }
}
