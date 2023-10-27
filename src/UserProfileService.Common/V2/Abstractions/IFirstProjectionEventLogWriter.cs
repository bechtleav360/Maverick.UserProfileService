using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.AggregateEvents.Common;
using UserProfileService.Common.V2.Models;

namespace UserProfileService.Common.V2.Abstractions;

/// <summary>
///     Writes <see cref="IUserProfileServiceEvent" />s to it's backend for later reporting or monitoring.
/// </summary>
public interface IFirstProjectionEventLogWriter
{
    /// <summary>
    ///     Returns the next batch that is committed and should be executed.
    /// </summary>
    /// <param name="cancellationToken">
    ///     The <see cref="CancellationToken" /> used to propagate notifications that the operation
    ///     should be canceled. Default: <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>Next committed batch as <see cref="EventBatch" />.</returns>
    Task<EventBatch> GetNextCommittedBatchAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Attempts to fetch the next batch that is committed and should be executed.
    /// </summary>
    /// <param name="cancellationToken">
    ///     The <see cref="CancellationToken" /> used to propagate notifications that the operation
    ///     should be canceled. Default: <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous read operation. It wraps a tuple containing a boolean value about the
    ///     success and the next committed batch as <see cref="EventBatch" /> if found.
    /// </returns>
    Task<(bool success, EventBatch result)> TryGetNextCommittedBatchAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns the batch by the given id.
    /// </summary>
    /// <param name="batchId">Id of batch to retrieve.</param>
    /// <param name="cancellationToken">
    ///     The <see cref="CancellationToken" /> used to propagate notifications that the operation
    ///     should be canceled. Default: <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>Batch as <see cref="EventBatch" />.</returns>
    Task<EventBatch> GetBatchAsync(string batchId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Get a collection of events for the provided <paramref name="transactionId" />.
    /// </summary>
    /// <param name="transactionId">The id of the batch to get the events for.</param>
    /// <param name="cancellationToken">
    ///     The <see cref="CancellationToken" /> used to propagate notifications that the operation
    ///     should be canceled. Default: <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>List of events.</returns>
    Task<IEnumerable<EventLogTuple>> GetEventsAsync(
        string transactionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Create a batch in the log/data source.
    /// </summary>
    /// <param name="batchId">Id of the batch.</param>
    /// <param name="cancellationToken">
    ///     The <see cref="CancellationToken" /> used to propagate notifications that the operation
    ///     should be canceled. Default: <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>Batch as <see cref="EventBatch" />.</returns>
    Task<EventBatch> CreateBatchAsync(
        string batchId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Create a batch with the given collection of events in the log/data source.
    /// </summary>
    /// <param name="batchId">Id of the batch.</param>
    /// <param name="eventsToBeAdded">The events to be logged/written in data source.</param>
    /// <param name="cancellationToken">
    ///     The <see cref="CancellationToken" /> used to propagate notifications that the operation
    ///     should be canceled. Default: <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>Batch as <see cref="EventBatch" />.</returns>
    Task<EventBatch> CreateBatchAsync(
        string batchId,
        IList<EventLogTuple> eventsToBeAdded,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Adds a collection of events to the log/data source.
    /// </summary>
    /// <param name="batchId">Id of the batch to add the events to.</param>
    /// <param name="eventsToBeAdded">The events to be logged/written in data source.</param>
    /// <param name="cancellationToken">
    ///     The <see cref="CancellationToken" /> used to propagate notifications that the operation
    ///     should be canceled. Default: <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>The <see cref="Task" /> that represents the asynchronous write operation.</returns>
    Task AddEventsAsync(
        string batchId,
        IList<EventLogTuple> eventsToBeAdded,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Update the batch by the given batch id.
    /// </summary>
    /// <param name="batchId">Id of batch to update.</param>
    /// <param name="batch">Batch to update.</param>
    /// <param name="cancellationToken">
    ///     The <see cref="CancellationToken" /> used to propagate notifications that the operation
    ///     should be canceled. Default: <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>Updated batch as <see cref="EventBatch" />.</returns>
    Task<EventBatch> UpdateBatchAsync(
        string batchId,
        EventBatch batch,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Update the event status of the provided <paramref name="event" />.
    /// </summary>
    /// <param name="id">Id of the event.</param>
    /// <param name="event">Event to update.</param>
    /// <param name="cancellationToken">
    ///     The <see cref="CancellationToken" /> used to propagate notifications that the operation
    ///     should be canceled. Default: <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>The <see cref="Task" /> that represents the asynchronous write operation.</returns>
    Task UpdateEventAsync(string id, EventLogTuple @event, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Aborts the batch with provided <paramref name="batchId" />.
    /// </summary>
    /// <param name="batchId">The id of the batch to be aborted.</param>
    /// <param name="cancellationToken">
    ///     The <see cref="CancellationToken" /> used to propagate notifications that the operation
    ///     should be canceled. Default: <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>The <see cref="Task" /> that represents the asynchronous write operation.</returns>
    Task AbortBatchAsync(string batchId, CancellationToken cancellationToken = default);
}
