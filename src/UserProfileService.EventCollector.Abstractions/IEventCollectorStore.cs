using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace UserProfileService.EventCollector.Abstractions;

/// <summary>
///     Defines a store containing all operation to for collector agent.
/// </summary>
public interface IEventCollectorStore
{
    /// <summary>
    ///     Get event data for the given process identifier.
    /// </summary>
    /// <param name="processId">Id of process to get event data for.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>List of <see cref="EventData" />.</returns>
    public Task<ICollection<EventData>> GetEventData(
        string processId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Counts the stored event data to the given collecting id.
    /// </summary>
    /// <param name="collectingId">Id of process to count event data.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>Number of event data for the given process id.</returns>
    public Task<int> GetCountOfEventDataAsync(string collectingId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Stores event data and returns the number of events including the event to be stored.
    /// </summary>
    /// <param name="data">Event data to save.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>Number of event data for the given process id.</returns>
    public Task<int> SaveEventDataAsync(EventData data, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Get the entity for the given id.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to return.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>
    ///     The requested entity for given identifier.
    /// </returns>
    public Task<TEntity> GetEntityAsync<TEntity>(string id, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Stores an entity in the event collector store.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity that should be stored.</typeparam>
    /// <param name="entity">The entity that should be stored.</param>
    /// <param name="entityId"> The unique identifier of the entity that should be used as primary key.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>A Task</returns>
    public Task SaveEntityAsync<TEntity>(
        TEntity entity,
        string entityId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Set the amount of items that should be collected from the event collector.
    /// </summary>
    /// <param name="collectingId">The id used to identify the current collecting process. </param>
    /// <param name="collectingAmount">The amount of items that should be collected.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled</param>
    /// <remarks>If the amount of items has been already set this method won't overwrite this and return false. </remarks>
    /// <returns>True if the amount of items has been set, otherwise false</returns>
    public Task<bool> TrySetCollectingItemsAmountAsync(
        Guid collectingId,
        int collectingAmount,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Set the time stamp when the collecting process is done.
    /// </summary>
    /// <param name="collectingId">The id used to identify the current collecting process. </param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled</param>
    /// <returns>True if the time stamp has been set, otherwise false</returns>
    public Task<bool> SetTerminateTimeForCollectingItemsProcessAsync(
        Guid collectingId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Get the external process Id.
    /// </summary>
    /// <param name="collectingId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<string> GetExternalProcessIdAsync(
        string collectingId,
        CancellationToken cancellationToken = default);
}
