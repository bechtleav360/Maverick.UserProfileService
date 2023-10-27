using System.Collections.Generic;
using System.Linq;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Utilities;
using UserProfileService.Sync.Abstraction.Models;

namespace UserProfileService.Sync.Extensions;

/// <summary>
///     Contains extension methods related to <see cref="KeyProperties" />.
/// </summary>
public static class KeyPropertiesExtension
{
    /// <summary>
    ///     Execute the key properties post filter
    ///     including converting between <see cref="IPaginatedList{TEntity}" /> and <see cref="IEnumerable{T}" />.
    /// </summary>
    /// <typeparam name="TEntity">Type of entity for <see cref="IPaginatedList{TEntity}" />.</typeparam>
    /// <param name="keyProperties">KeyProperties including post filter.</param>
    /// <param name="results">List running the filter.</param>
    /// <returns>Filtered list as <see cref="IPaginatedList{TEntity}" />.</returns>
    public static IPaginatedList<TEntity> ExecutePostFilter<TEntity>(
        this KeyProperties keyProperties,
        IPaginatedList<TEntity> results)
    {
        if (keyProperties?.PostFilter == null)
        {
            return results;
        }

        List<TEntity> postFilteredResult =
            keyProperties.PostFilter.Invoke(results.Cast<object>()).Cast<TEntity>().ToList();

        return new PaginatedList<TEntity>(postFilteredResult);
    }

    /// <summary>
    ///     Execute the key properties post filter
    ///     including converting between <see cref="IPaginatedList{TEntity}" /> and <see cref="IEnumerable{T}" />.
    /// </summary>
    /// <typeparam name="TEntity">Type of entity for <see cref="IPaginatedList{TEntity}" />.</typeparam>
    /// <param name="keyProperties">KeyProperties including post filter.</param>
    /// <param name="results">List running the filter.</param>
    /// <returns>Filtered list as <see cref="IPaginatedList{TEntity}" />.</returns>
    public static ICollection<TEntity> ExecutePostFilter<TEntity>(
        this KeyProperties keyProperties,
        ICollection<TEntity> results)
    {
        if (keyProperties?.PostFilter == null)
        {
            return results;
        }

        List<TEntity> postFilteredResult =
            keyProperties.PostFilter.Invoke(results.Cast<object>()).Cast<TEntity>().ToList();

        return new PaginatedList<TEntity>(postFilteredResult);
    }
}
