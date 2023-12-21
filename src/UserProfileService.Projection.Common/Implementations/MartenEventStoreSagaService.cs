using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.AggregateEvents.Common;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UserProfileService.Common;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Enums;
using UserProfileService.Common.V2.Models;
using UserProfileService.EventSourcing.Abstractions.Stores;
using UserProfileService.Projection.Common.Abstractions;

namespace UserProfileService.Projection.Common.Implementations;

/// <summary>
///     Uses EventStore as target source to persist <see cref="IUserProfileServiceEvent" />s. The provided IDatabases
///     implementation will be used to log the status and as internal databaseClient source of the first-level projection.
/// </summary>
internal class MartenEventStoreSagaService : ISagaService
{
    private readonly IFirstProjectionEventLogWriter _DatabaseClient;
    private readonly IEventStorageClient _EventStoreClient;
    private readonly ILogger _Logger;

    public MartenEventStoreSagaService(
        IFirstProjectionEventLogWriter databaseClient,
        IEventStorageClient eventStorageClient,
        ILogger<MartenEventStoreSagaService> logger)
    {
        _DatabaseClient = databaseClient;
        _EventStoreClient = eventStorageClient;
        _Logger = logger;
    }

    private async Task<IList<EventLogTuple>> PrepareAndValidateEventsAsync(
        Guid batchId,
        IEnumerable<EventTuple> events,
        CancellationToken cancellationToken)
    {
        _Logger.EnterMethod();

        List<EventTuple> eventsList = events as List<EventTuple> ?? events.ToList();

        if (_Logger.IsEnabledForTrace())
        {
            _Logger.LogTraceMessage(
                "Input parameter events: {events}",
                LogHelpers.Arguments(eventsList.Select(e => e.ToString()).ToArray().ToLogString()));
        }

        List<EventLogTuple> eventLogTuples = events.Select(e => new EventLogTuple(e, batchId.ToString())).ToList();

        bool validated = await _EventStoreClient.ValidateEventsAsync(eventsList, cancellationToken);

        if (!validated)
        {
            throw new ArgumentException(
                "Parameter events contain invalid events that are not written to the event store.",
                nameof(events));
        }

        return _Logger.ExitMethod(eventLogTuples);
    }

    private async Task CheckBatchIsInitialized(
        Guid batchId,
        EventBatch batch = null,
        CancellationToken cancellationToken = default)
    {
        _Logger.LogDebugMessage("Check status of batch with id {id} is initialized.", batchId.AsArgumentList());

        batch ??= await _DatabaseClient.GetBatchAsync(batchId.ToString(), cancellationToken);

        if (batch.Status != EventStatus.Initialized)
        {
            // TODO:
            throw new Exception($"Batch can not be updated, because it is already in status {batch.Status}");
        }
    }

    /// <inheritdoc />
    public async Task<Guid> CreateBatchAsync(CancellationToken cancellationToken = default)
    {
        _Logger.EnterMethod();

        var batchId = Guid.NewGuid();

        _Logger.LogInfoMessage("Create new batch with id {batchId}.", batchId.AsArgumentList());

        EventBatch batch = await _DatabaseClient.CreateBatchAsync(
            batchId.ToString(),
            cancellationToken);

        _Logger.LogInfoMessage(
            "Successfully create batch without events as batch {batchId}",
            LogHelpers.Arguments(batch.Id));

        return _Logger.ExitMethod(batchId);
    }

    /// <inheritdoc />
    public async Task<Guid> CreateBatchAsync(
        CancellationToken cancellationToken = default,
        params EventTuple[] initialEvents)
    {
        _Logger.EnterMethod();

        if (initialEvents == null)
        {
            throw new ArgumentNullException(nameof(initialEvents));
        }

        if (initialEvents.Length == 0)
        {
            throw new ArgumentException(
                "Parameter initialEvents cannot be an empty collection.",
                nameof(initialEvents));
        }

        if (_Logger.IsEnabledForTrace())
        {
            _Logger.LogTraceMessage(
                "Input parameter initialEvents: {initialEvents}",
                LogHelpers.Arguments(initialEvents.Select(e => e.ToString()).ToArray().ToLogString()));
        }

        var batchId = Guid.NewGuid();

        IList<EventLogTuple> eventLogTuples =
            await PrepareAndValidateEventsAsync(batchId, initialEvents, cancellationToken);

        EventBatch batch = await _DatabaseClient.CreateBatchAsync(
            batchId.ToString(),
            eventLogTuples,
            cancellationToken);

        _Logger.LogInfoMessage(
            "Successfully create batch with events (amount: {batchSize}) as batch {batchId}",
            LogHelpers.Arguments(initialEvents.Length, batch.Id));

        return _Logger.ExitMethod(batchId);
    }

    /// <inheritdoc />
    public async Task AddEventsAsync(
        Guid batchId,
        IEnumerable<EventTuple> events,
        CancellationToken cancellationToken = default)
    {
        _Logger.EnterMethod();

        if (batchId == Guid.Empty)
        {
            throw new ArgumentException("Batch id cannot be an empty guid.", nameof(batchId));
        }

        if (events == null)
        {
            throw new ArgumentNullException(nameof(events), "Parameter events cannot be null.");
        }

        if (!events.Any())
        {
            throw new ArgumentException(
                "Parameter events cannot be an empty collection.",
                nameof(events));
        }

        await CheckBatchIsInitialized(batchId, cancellationToken: cancellationToken);

        IList<EventLogTuple> eventLogTuples =
            await PrepareAndValidateEventsAsync(batchId, events, cancellationToken);

        await _DatabaseClient.AddEventsAsync(batchId.ToString(), eventLogTuples, cancellationToken);

        _Logger.LogDebugMessage(
            "Successfully added events (amount: {batchSize}) as batch {batchId}",
            LogHelpers.Arguments(eventLogTuples.Count, batchId));

        _Logger.ExitMethod();
    }

    /// <inheritdoc />
    public async Task AbortBatchAsync(Guid batchId, CancellationToken cancellationToken = default)
    {
        _Logger.EnterMethod();

        if (batchId == Guid.Empty)
        {
            throw new ArgumentException("Batch id cannot be an empty guid.", nameof(batchId));
        }

        await CheckBatchIsInitialized(batchId, cancellationToken: cancellationToken);

        await _DatabaseClient.AbortBatchAsync(batchId.ToString(), cancellationToken);

        _Logger.LogInfoMessage(
            "Batch {batchId} aborted.",
            batchId.AsArgumentList());

        _Logger.ExitMethod();
    }

    /// <inheritdoc />
    public async Task ExecuteBatchAsync(Guid batchId, CancellationToken cancellationToken = default)
    {
        _Logger.EnterMethod();

        if (batchId == Guid.Empty)
        {
            throw new ArgumentException("Batch id cannot be an empty guid.", nameof(batchId));
        }

        _Logger.LogInfoMessage("Try to execute batch with id {id}.", batchId.AsArgumentList());
        _Logger.LogDebugMessage("Get batch in database for id {id}.", batchId.AsArgumentList());

        EventBatch batch = await _DatabaseClient.GetBatchAsync(batchId.ToString(), cancellationToken);

        _Logger.LogDebugMessage(
            "Found batch for id {id} and status {status}.",
            LogHelpers.Arguments(batch, batch.Status));

        await CheckBatchIsInitialized(batchId, batch, cancellationToken);

        if (_Logger.IsEnabledForTrace())
        {
            _Logger.LogDebugMessage(
                "Found batch for id {id} and data: {data}.",
                LogHelpers.Arguments(batch, JsonConvert.SerializeObject(batch)));
        }

        if (batch.Status != EventStatus.Initialized && batch.Status != EventStatus.Committed)
        {
            throw new Exception("Batch cannot be executed because the batch has not been initialized.");
        }

        _Logger.LogDebugMessage(
            "Status of batch with {id} is initialized and can be updated to committed.",
            batchId.AsArgumentList());

        batch.Status = EventStatus.Committed;
        batch.UpdatedAt = DateTime.UtcNow;

        await _DatabaseClient.UpdateBatchAsync(batchId.ToString(), batch, cancellationToken);

        _Logger.LogDebugMessage("Get and process next committed batch in database.", LogHelpers.Arguments());

        _Logger.ExitMethod();
    }
}
