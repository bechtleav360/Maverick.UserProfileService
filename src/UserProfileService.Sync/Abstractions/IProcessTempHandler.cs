using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UserProfileService.Sync.Abstractions;

/// <summary>
///     Interface describe a service to handle the entity states during the synchronization process.
/// </summary>
public interface IProcessTempHandler
{
    /// <summary>
    ///     Return of all temporary object keys for the given synchronization id.
    /// </summary>
    /// <typeparam name="TEntity">Type of object type to sync.</typeparam>
    /// <param name="syncId">Id of the synchronization process.</param>
    /// <returns>List of operation id for temp object.</returns>
    public Task<IList<Guid>> GetTemporaryObjectKeysAsync<TEntity>(string syncId);

    /// <summary>
    ///     Get an object for the given synchronization process and operation id.
    /// </summary>
    /// <typeparam name="TEntity">Type of object to return.</typeparam>
    /// <param name="syncId">Id of the synchronization process.</param>
    /// <param name="operationId">Operation id of the request/operation.</param>
    /// <returns>A task that represents the asynchronous read operation. It wraps a temporary object.</returns>
    public Task<TEntity> GetTemporaryObjectAsync<TEntity>(string syncId, Guid operationId);

    /// <summary>
    ///     Save an object for the current synchronization process in a temporary store for later access.
    /// </summary>
    /// <typeparam name="TEntity">Type of object to save.</typeparam>
    /// <param name="syncId">Id of the synchronization process.</param>
    /// <param name="operationId">Operation id of the request/operation.</param>
    /// <param name="obj">Object to save temporary.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    public Task AddTemporaryObjectAsync<TEntity>(string syncId, Guid operationId, TEntity obj);
}
