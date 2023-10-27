using System;
using System.Collections.Generic;
using System.Linq;

namespace UserProfileService.Adapter.Arango.V2.Extensions;

/// <summary>
///     Contains methods related to <see cref="IEnumerable{T}" />s.
/// </summary>
internal static class EnumerableExtensions
{
    /// <summary>
    ///     Executes the where clause only if <paramref name="predicate" /> is not <c>null</c>.
    ///     If <paramref name="predicate" /> is provided, the LINQ Where method will be used.
    /// </summary>
    internal static IEnumerable<TElem> WhereSafely<TElem>(
        this IEnumerable<TElem> sequence,
        Func<TElem, bool> predicate)
    {
        return predicate != null
            ? sequence?.Where(predicate)
            : sequence;
    }
}
