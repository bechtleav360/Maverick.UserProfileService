using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Maverick.UserProfileService.Models.EnumModels;

// Implementation was based on the redis implementation of redis from the MassTransit project. 
// https://github.com/MassTransit/MassTransit/tree/develop/src/Persistence/MassTransit.RedisIntegration
namespace UserProfileService.Messaging.ArangoDb.Saga;

/// <summary>
///     Describes the context for the database.
/// </summary>
/// <typeparam name="TSaga">Type of database context.</typeparam>
public interface IDatabaseContext<TSaga> :
    IAsyncDisposable
    where TSaga : class, ISaga
{
    /// <summary>
    ///     Add the given context to database.
    /// </summary>
    /// <param name="context">Context to add.</param>
    /// <returns>Represents the async operation of adding context.</returns>
    Task AddAsync(SagaConsumeContext<TSaga> context);

    /// <summary>
    ///     Insert the given saga to database.
    /// </summary>
    /// <param name="instance">Instance to insert.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>Represents the async operation of inserting saga.</returns>
    Task InsertAsync(TSaga instance, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Load a list of sagas.
    /// </summary>
    /// <param name="offset">The number of items to skip before starting to collect the result set.</param>
    /// <param name="limit">The number of items to return.</param>
    /// <param name="sortExpression"> Sort expression</param>
    /// <param name="filterExpression"> Expression containing a predicate to filter result.</param>
    /// <param name="sortOrder">Order so sort expression member.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>Tuple of number of total results and results for query.</returns>
    Task<(int count, IList<TSaga> results)> LoadAsync(
        int limit,
        int offset,
        Expression<Func<TSaga, object>> sortExpression = null,
        Expression<Predicate<TSaga>> filterExpression = null,
        SortOrder sortOrder = SortOrder.Asc,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Load the saga for given correlationId.
    /// </summary>
    /// <param name="correlationId">Correlation identifier of saga to retrieve.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns></returns>
    Task<TSaga> LoadAsync(Guid correlationId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Update the the given saga context.
    /// </summary>
    /// <param name="context">Context to update.</param>
    /// <returns>Represents the async operation of updating saga context.</returns>
    Task UpdateAsync(SagaConsumeContext<TSaga> context);

    /// <summary>
    ///     Update the the given saga object.
    /// </summary>
    /// <param name="instance">Context to update.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>Represents the async operation of updating saga context.</returns>
    Task UpdateAsync(TSaga instance, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Delete the given saga context.
    /// </summary>
    /// <param name="context">Context to delete.</param>
    /// <returns>Represents the async operation of deleting saga context.</returns>
    Task DeleteAsync(SagaConsumeContext<TSaga> context);
}
