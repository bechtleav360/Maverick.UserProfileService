using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Saga;
using Maverick.UserProfileService.Models.EnumModels;

namespace UserProfileService.Messaging.ArangoDb.Saga;

/// <summary>
///     Describes the query context for saga repository to query sagas.
/// </summary>
/// <typeparam name="TSaga">Type of context saga.</typeparam>
public interface ISagaRepositoryQueryContext<TSaga> : QuerySagaRepositoryContext<TSaga>,
    LoadSagaRepositoryContext<TSaga> where TSaga : class, ISaga
{
    /// <summary>
    ///     Load a list of sagas.
    /// </summary>
    /// <param name="offset">The number of items to skip before starting to collect the result set.</param>
    /// <param name="limit">The number of items to return.</param>
    /// <param name="sortExpression">Expression used to sort.</param>
    /// <param name="filterExpression">Expression used to filter.</param>
    /// <param name="sortOrder">Order so sort expression member.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>Tuple of number of total results and results for query.</returns>
    Task<Tuple<int, IList<TSaga>>> QueryAsync(
        int limit,
        int offset,
        Expression<Func<TSaga, object>> sortExpression = null,
        Expression<Predicate<TSaga>> filterExpression = null,
        SortOrder sortOrder = SortOrder.Asc,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Update the given saga instance
    /// </summary>
    /// <param name="instance">Saga instance of type <see cref="TSaga" /></param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns> A <see cref="Task" /></returns>
    Task UpdateAsync(TSaga instance, CancellationToken cancellationToken = default);
}
