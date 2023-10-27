using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UserProfileService.Sync.Abstraction.Models;
using UserProfileService.Sync.Abstraction.Models.Results;

namespace UserProfileService.Sync.Utilities;

/// <summary>
///     Contains extension methods to handle batch requests with <see cref="IBatchResult{T}" /> as result..
/// </summary>
public static class BatchUtility
{
    /// <summary>
    ///     Gets all entities from a source system.
    /// </summary>
    /// <typeparam name="T">The Type of the entities.</typeparam>
    /// <param name="batchFunc">A function that is used to get all entities from a source system.</param>
    /// <param name="externalObjectId">The source object id to identify the objects in the destination system.</param>
    /// <param name="token"> A cancellation token <see cref="CancellationToken" /></param>
    /// <returns>
    ///     A task that represents the asynchronous read operation. It wraps a list of all entities that are if the type
    ///     <inheritdoc cref="ISyncModel" />.
    /// </returns>
    public static async Task<IList<T>> GetAllEntities<T>(
        Func<int, int, DateTime, CancellationToken, KeyProperties, Task<IBatchResult<T>>> batchFunc,
        KeyProperties externalObjectId = null,
        CancellationToken token = default) where T : ISyncModel
    {
        var entities = new List<T>();

        var startPosition = 0;
        const int batchSize = 500;

        IBatchResult<T> result;

        do
        {
            result = await batchFunc.Invoke(startPosition, batchSize, DateTime.MinValue, token, externalObjectId);
            entities.AddRange(result.Result);
            startPosition += batchSize;
        }
        while (result.NextBatch);

        return entities;
    }

    /// <summary>
    ///     Gets all entities from a source system.
    /// </summary>
    /// <typeparam name="T">The Type of the entities.</typeparam>
    /// <param name="batchFunc">A function that is used to get all entities from a source system.</param>
    /// <param name="predicate">A function that is used to filter the given entities.</param>
    /// <param name="ctx">Propagates notification that operations should be canceled.</param>
    /// <returns>
    ///     A task that represents the asynchronous read operation. It wraps a list of all entities that are if the type
    ///     <inheritdoc cref="ISyncModel" />.
    /// </returns>
    public static async Task<IList<T>> GetAllEntities<T>(
        Func<int, int, DateTime, CancellationToken, KeyProperties, Task<IBatchResult<T>>> batchFunc,
        Func<T, bool> predicate,
        CancellationToken ctx)
        where T : ISyncModel
    {
        var entities = new List<T>();

        var startPosition = 0;
        const int batchSize = 500;

        IBatchResult<T> result;

        do
        {
            result = await batchFunc.Invoke(startPosition, batchSize, DateTime.MinValue, ctx, default);

            IEnumerable<T> filteredResult = result.Result.Where(predicate);

            entities.AddRange(filteredResult);
            startPosition += batchSize;
        }
        while (result.NextBatch);

        return entities;
    }

    /// <summary>
    ///     Gets all entities from a source system.
    /// </summary>
    /// <typeparam name="T">The Type of the entities.</typeparam>
    /// <param name="batchFunc">A function that is used to get all entities from a source system.</param>
    /// <param name="ctx">Propagates notification that operations should be canceled.</param>
    /// <returns>
    ///     A task that represents the asynchronous read operation. It wraps a list of all entities that are if the type
    ///     <inheritdoc cref="ISyncModel" />.
    /// </returns>
    public static async Task<IList<T>> GetAllEntities<T>(
        Func<int, int, CancellationToken, Task<IBatchResult<T>>> batchFunc,
        CancellationToken ctx) where T : ISyncModel
    {
        var entities = new List<T>();

        var startPosition = 0;
        const int batchSize = 500;

        IBatchResult<T> result;

        do
        {
            result = await batchFunc.Invoke(startPosition, batchSize, ctx);
            entities.AddRange(result.Result);
            startPosition += batchSize;
        }
        while (result.NextBatch);

        return entities;
    }
}
