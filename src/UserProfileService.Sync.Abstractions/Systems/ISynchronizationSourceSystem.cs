using System.Threading;
using System.Threading.Tasks;
using UserProfileService.Sync.Abstraction.Models;
using UserProfileService.Sync.Abstraction.Models.Results;

namespace UserProfileService.Sync.Abstraction.Systems;

/// <summary>
///     Defines how the entities can be fetched from the source system.
///     This will happen as a batch, where you have to define the start
///     point and the batch size.
/// </summary>
/// <typeparam name="T">The type the returned entities have.</typeparam>
public interface ISynchronizationSourceSystem<T> where T : ISyncModel
{
    /// <summary>
    ///     Gets the a batch of objects from the source system.
    /// </summary>
    /// <typeparam>
    ///     The type of object from the source.
    ///     <name>T</name>
    /// </typeparam>
    /// <param name="start">Where the batch starts.</param>
    /// <param name="batchSize">The batch size of the returned objects.</param>
    /// <param name="token">The token is used for cancel the started task.</param>
    /// <returns>
    ///     A task that represents the asynchronous read operation. It wraps objects from the source system
    ///     <inheritdoc cref="IBatchResult{T}" />
    /// </returns>
    Task<IBatchResult<T>> GetBatchAsync(int start, int batchSize, CancellationToken token);

    /// <summary>
    ///     Create the entity in the source system.
    /// </summary>
    /// <typeparam>
    ///     The type of object from the source.
    ///     <name>T</name>
    /// </typeparam>
    /// <param name="entity">
    ///     Entity of type
    ///     <see>
    ///         <cref>T</cref>
    ///     </see>
    ///     to create.
    /// </param>
    /// <param name="token">The token is used for cancel the started task.</param>
    /// <returns>A task that represents the created entity in the source system.</returns>
    Task<T> CreateEntity(T entity, CancellationToken token);

    /// <summary>
    ///     Update the entity in the source system.
    /// </summary>
    /// <typeparam>
    ///     The type of object from the source.
    ///     <name>T</name>
    /// </typeparam>
    /// <param name="sourceId">Identifier of the entity in source system.</param>
    /// <param name="entity">
    ///     Entity of type
    ///     <see>
    ///         <cref>T</cref>
    ///     </see>
    ///     to create.
    /// </param>
    /// <param name="token">The token is used for cancel the started task.</param>
    /// <returns>A task that represents the created entity in the source system.</returns>
    Task<T> UpdateEntity(string sourceId, T entity, CancellationToken token);

    /// <summary>
    ///     Delete the entity in the source system.
    /// </summary>
    /// <param name="sourceId">Identifier of the entity in source system.</param>
    /// <param name="token">The token is used for cancel the started task.</param>
    /// <returns>A task that represents the deleted entity in the source system.</returns>
    Task DeleteEntity(string sourceId, CancellationToken token);
}
