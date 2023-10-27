using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UserProfileService.Sync.Abstraction.Models;
using UserProfileService.Sync.Abstraction.Models.Results;

namespace UserProfileService.Sync.Abstraction;

/// <summary>
///     This interface is used to read from the destination system.
/// </summary>
public interface ISynchronizationReadDestination<T> where T : ISyncModel
{
    /// <summary>
    ///     Gets the object by an internal id of the destination system.
    /// </summary>
    /// <param name="id">The destination object id to identify the object in the destination system.</param>
    /// <param name="token">The token is used for cancel the started task.</param>
    /// <returns>
    ///     A task that represents the asynchronous read operation. It wraps a generic object that implements the
    ///     interface <see cref="ISyncModel" /> and returns a found element.
    /// </returns>
    Task<T> GetObjectAsync(string id, CancellationToken token);

    /// <summary>
    ///     Get a list of objects by an external id from the destination system.
    /// </summary>
    /// <param name="externalObjectId">The source object id to identify the objects in the destination system.</param>
    /// <param name="token">The token is used for cancel the started task.</param>
    /// <returns>
    ///     A task that represents the asynchronous read operation. It wraps a generic collection
    ///     that contains all found element(s) corresponding to the given ids.
    /// </returns>
    Task<ICollection<T>> GetObjectsAsync(KeyProperties externalObjectId, CancellationToken token);

    /// <summary>
    ///     GetAsync all objects that were not process by the current sync. You can identify the
    ///     not processed objects by the sync time.
    /// </summary>
    /// <param name="start">Where the batch starts.</param>
    /// <param name="batchSize">The batch size of the returned objects.</param>
    /// <param name="stamp">The time stamp from the current sync run.</param>
    /// <param name="externalObjectId">The source object id to identify the objects in the destination system.</param>
    /// <param name="token">The token is used for cancel the started task.</param>
    /// <returns>
    ///     A task that represents the asynchronous read operation. It wraps a list of objects that was not processed by
    ///     the current synchronization run.
    /// </returns>
    Task<IBatchResult<T>> GetObjectsAsync(
        int start,
        int batchSize,
        DateTime stamp,
        CancellationToken token,
        KeyProperties externalObjectId = null);
}
