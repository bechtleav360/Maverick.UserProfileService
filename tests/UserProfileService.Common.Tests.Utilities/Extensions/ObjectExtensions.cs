using System.Collections.Generic;
using System.Linq;

namespace UserProfileService.Common.Tests.Utilities.Extensions
{
    public static class ObjectExtensions
    {
        public static IList<TElement> AsCollection<TElement>(
            this TElement first,
            params TElement[] additional)
        {
            return Enumerable
                .Empty<TElement>()
                .Append(first)
                .Concat(additional)
                .ToList();
        }
    }
}
