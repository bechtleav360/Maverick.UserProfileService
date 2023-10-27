using System;
using System.Collections.Generic;
using System.Linq;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Utilities;

namespace UserProfileService.Common.V2.Extensions;

/// <summary>
///     Contains extension methods related to <see cref="IEnumerable{T}" />s.
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    ///     Creates an <see cref="IPaginatedList{TElem}" /> using a specified <see cref="IEnumerable{TElem}" />.
    /// </summary>
    /// <typeparam name="TElem">The type of each element in the <paramref name="collection" />.</typeparam>
    /// <param name="collection">
    ///     The sequence of elements that should be contained by a resulting
    ///     <see cref="PaginatedList{TElem}" />.
    /// </param>
    /// <param name="totalAmount">
    ///     The total amount of elements. That can differ from <see cref="List{T}.Count" />, if the
    ///     pagination settings limit the result.
    /// </param>
    /// <returns>The resulting paginated list.</returns>
    public static IPaginatedList<TElem> ToPaginatedList<TElem>(
        this IEnumerable<TElem> collection,
        long totalAmount)
    {
        if (collection == null)
        {
            throw new ArgumentNullException(nameof(collection));
        }

        return new PaginatedList<TElem>(collection, totalAmount);
    }

    /// <summary>
    ///     Creates an <see cref="IPaginatedList{TElem}" /> using a specified <see cref="IEnumerable{TElem}" />. The total
    ///     amount will be set to the value of <c>Count</c>.
    /// </summary>
    /// <typeparam name="TElem">The type of each element in the <paramref name="collection" />.</typeparam>
    /// <param name="collection">
    ///     The sequence of elements that should be contained by a resulting
    ///     <see cref="PaginatedList{TElem}" />.
    /// </param>
    /// <returns>The resulting paginated list.</returns>
    public static IPaginatedList<TElem> ToPaginatedList<TElem>(this IEnumerable<TElem> collection)
    {
        if (collection == null)
        {
            throw new ArgumentNullException(nameof(collection));
        }

        return new PaginatedList<TElem>(collection);
    }

    /// <summary>
    ///     Takes the first unconverted external Id where is the sources matches.
    /// </summary>
    /// <param name="externalIds"> The external Ids </param>
    /// <param name="sourceSystem"> The source system where the entity comes from. </param>
    /// <returns> The first (or default) unconverted external id. </returns>
    public static T FirstOrDefaultUnconverted<T>(this IEnumerable<T> externalIds, string sourceSystem = null)
        where T : ExternalIdentifier
    {
        return string.IsNullOrWhiteSpace(sourceSystem)
            ? externalIds.FirstOrDefault(extId => !extId.IsConverted)
            : externalIds.FirstOrDefault(
                extId => !extId.IsConverted
                    && string.Compare(extId.Source, sourceSystem, StringComparison.CurrentCultureIgnoreCase) == 0);
    }

    /// <summary>
    /// </summary>
    /// <typeparam name="TElem"></typeparam>
    /// <param name="sequence"></param>
    /// <param name="condition"></param>
    /// <param name="elementToAppend"></param>
    /// <returns></returns>
    public static IEnumerable<TElem> AppendConditional<TElem>(
        this IEnumerable<TElem> sequence,
        bool condition,
        TElem elementToAppend)
    {
        return condition
            ? sequence.Append(elementToAppend)
            : sequence;
    }
}
