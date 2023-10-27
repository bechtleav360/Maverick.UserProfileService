using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UserProfileService.Common.V2.Contracts;

namespace UserProfileService.Common.V2.Abstractions;

/// <summary>
///     Contains methods to manage and return API cursors.
/// </summary>
public interface ICursorApiProvider
{
    /// <summary>
    ///     Creates a cursor and returns cursor information including the requested object as payload.
    /// </summary>
    /// <typeparam name="TService">Type of the service to be used.</typeparam>
    /// <typeparam name="TEntity">The type of the entity,</typeparam>
    /// <typeparam name="TResult">The complete result type.</typeparam>
    /// <param name="service">The service to be used to get cursor payloads.</param>
    /// <param name="readMethod">The asynchronous method definition to retrieve cursor payloads.</param>
    /// <param name="pageSize">The size of each page returned by every cursor response.</param>
    /// <param name="token">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous write operation. It contains a <see cref="CursorState{TPayload}" />
    ///     object.
    /// </returns>
    Task<CursorState<TEntity>> CreateCursorAsync<TService, TEntity, TResult>(
        TService service,
        Func<TService, CancellationToken, Task<TResult>> readMethod,
        int pageSize,
        CancellationToken token = default)
        where TService : IReadService
        where TResult : IList<TEntity>;

    /// <summary>
    ///     Returns the next cursor page result set.
    /// </summary>
    /// <typeparam name="TEntity">The type of the elements in the result sets.</typeparam>
    /// <param name="id">The cursor id.</param>
    /// <param name="token">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous read operation. It contains the next result set of a
    ///     <see cref="CursorState{TPayload}" /> object.
    /// </returns>
    Task<CursorState<TEntity>> GetNextPageAsync<TEntity>(
        string id,
        CancellationToken token = default
    );

    /// <summary>
    ///     Deletes the cursor.
    /// </summary>
    /// <param name="id">The id of the cursor to be deleted.</param>
    /// <param name="token">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>A task representing the asynchronous write operation.</returns>
    Task DeleteCursorAsync(string id, CancellationToken token = default);
}
