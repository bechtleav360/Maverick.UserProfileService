using System.Linq.Expressions;
using Marten;
using Marten.Pagination;
using UserProfileService.Common.V2.Models;
using UserProfileService.Queries.Language.Extensions;
using UserProfileService.Queries.Language.Visitors;

namespace UserProfileService.Adapter.Marten.Extensions;

/// <summary>
///     The marten queryable extensions are used to filter the Postgres database results with
///     LINQ.
/// </summary>
public static class MartenDbQueryableExtension
{
    private static IQueryable<TEntity> WhereOptions<TEntity>(
        this IQueryable<TEntity> queryOptions,
        QueryOptionsVolatileModel? volatileModel)
    {
        if (queryOptions == null)
        {
            throw new ArgumentNullException(nameof(queryOptions));
        }

        if (volatileModel?.FilterTree == null || string.IsNullOrWhiteSpace(volatileModel.Filter))
        {
            return queryOptions;
        }

        var treeVisitor = new TreeNodeExpressionVisitor();

        Expression<Func<TEntity, bool>> whereExpression = treeVisitor.Visit<TEntity>(volatileModel.FilterTree);

        if (whereExpression == null)
        {
            throw new ArgumentNullException(nameof(whereExpression));
        }

        return queryOptions.Where(whereExpression);
    }

    private static IQueryable<TEntity> OrderByOptions<TEntity>(
        this IQueryable<TEntity> queryOptions,
        QueryOptionsVolatileModel? volatileModel)
    {
        if (queryOptions == null)
        {
            throw new ArgumentNullException(nameof(queryOptions));
        }

        if (volatileModel?.OrderByList == null || !volatileModel.OrderByList.Any())
        {
            return queryOptions;
        }

        return queryOptions.OrderBy(volatileModel.OrderByList.CreateOrderByLinqQuery());
    }

    private static async Task<IEnumerable<TEntity>> ToMartenPaginatedList<TEntity>(
        this IQueryable<TEntity> queryable,
        QueryOptionsVolatileModel volatileModel,
        CancellationToken token = default)
    {
        if (queryable == null)
        {
            throw new ArgumentNullException(nameof(queryable));
        }

        return await queryable.ToPagedListAsync(volatileModel.Offset, volatileModel.Limit, token);
    }

    /// <summary>
    ///     The method apply all query filter that the
    ///     <paramref name="queryVolatileModel" />
    ///     does
    ///     contain. If there are not filters present they will be ignored and only the pagination is used.
    /// </summary>
    /// <param name="queryableObject">The queryable object is used to apply the filters if they are included.</param>
    /// <param name="queryVolatileModel">The query object that contains the filter query that should apply on the result set.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <typeparam name="TEntity">The result entity for whose the filter should apply to.</typeparam>
    /// <returns>Returns the result set as an <see cref="IEnumerable{T}" /> list.</returns>
    /// <exception cref="ArgumentException">
    ///     If the
    ///     <paramref name="queryableObject" />
    ///     or
    ///     <paramref name="queryVolatileModel" />
    ///     are null
    /// </exception>
    public static async Task<IEnumerable<TEntity>> ApplyOptions<TEntity>(
        this IQueryable<TEntity> queryableObject,
        QueryOptionsVolatileModel queryVolatileModel,
        CancellationToken cancellationToken = default)
    {
        if (queryableObject == null)
        {
            throw new ArgumentNullException(nameof(queryableObject));
        }

        if (queryVolatileModel == null)
        {
            throw new ArgumentNullException(nameof(queryVolatileModel));
        }

        return await queryableObject.WhereOptions(queryVolatileModel)
            .OrderByOptions(queryVolatileModel)
            .ToMartenPaginatedList(queryVolatileModel, cancellationToken);
    }
}
