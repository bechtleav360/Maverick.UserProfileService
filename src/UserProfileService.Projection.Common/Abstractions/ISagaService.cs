using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UserProfileService.Common;

namespace UserProfileService.Projection.Common.Abstractions;

/// <summary>
///     Describes a service that sends a series of <see cref="EventTuple" /> using saga.
/// </summary>
public interface ISagaService
{
    /// <summary>
    ///     Cancels an existing batch. Events that have already been sent are not undone.
    /// </summary>
    /// <param name="batchId">Id of batch to abort.</param>
    /// <param name="cancellationToken">
    ///     The <see cref="CancellationToken" /> used to propagate notifications that the operation
    ///     should be canceled. Default: <see cref="CancellationToken.None" />
    /// </param>
    /// <returns></returns>
    Task AbortBatchAsync(Guid batchId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Adds a collection of <see cref="EventTuple" /> to the existing batch.
    /// </summary>
    /// <param name="batchId">Id of the batch to be added to the events.</param>
    /// <param name="events">List of <see cref="EventTuple" /> to be added to the batch.</param>
    /// <param name="cancellationToken">
    ///     The <see cref="CancellationToken" /> used to propagate notifications that the operation
    ///     should be canceled. Default: <see cref="CancellationToken.None" />
    /// </param>
    /// <returns></returns>
    Task AddEventsAsync(
        Guid batchId,
        IEnumerable<EventTuple> events,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a batch of <see cref="EventTuple" /> to be sent.
    /// </summary>
    /// <param name="cancellationToken">
    ///     The <see cref="CancellationToken" /> used to propagate notifications that the operation
    ///     should be canceled. Default: <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>Id of the created batch.</returns>
    Task<Guid> CreateBatchAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a batch of <see cref="EventTuple" /> to be sent.
    /// </summary>
    /// <param name="cancellationToken">
    ///     The <see cref="CancellationToken" /> used to propagate notifications that the operation
    ///     should be canceled. Default: <see cref="CancellationToken.None" />
    /// </param>
    /// <param name="initialEvents"></param>
    /// <returns>Id of the created batch.</returns>
    Task<Guid> CreateBatchAsync(CancellationToken cancellationToken = default, params EventTuple[] initialEvents);

    /// <summary>
    ///     Execute the batch.
    /// </summary>
    /// <param name="batchId">Batch to be executed.</param>
    /// <param name="cancellationToken">
    ///     The <see cref="CancellationToken" /> used to propagate notifications that the operation
    ///     should be canceled. Default: <see cref="CancellationToken.None" />
    /// </param>
    /// <returns></returns>
    Task ExecuteBatchAsync(Guid batchId, CancellationToken cancellationToken = default);
}
