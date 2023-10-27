using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Enums;
using UserProfileService.Common.V2.Models;
using UserProfileService.EventSourcing.Abstractions.Stores;
using UserProfileService.Projection.Common.Abstractions;

namespace UserProfileService.Projection.Common.Implementations;

/// <summary>
///     Marten implementation of <see cref="IOutboxProcessorService" />.
/// </summary>
internal class MartenEventStoreOutboxProcessorService : IOutboxProcessorService
{
    private const int _MaxConcurrentProcesses = 1;
    private static readonly SemaphoreSlim _SyncObject = new SemaphoreSlim(1, _MaxConcurrentProcesses);
    private readonly IFirstProjectionEventLogWriter _DatabaseClient;
    private readonly IEventStorageClient _EventStoreClient;
    private readonly ILogger<MartenEventStoreOutboxProcessorService> _Logger;

    /// <summary>
    ///     Create an instance of <see cref="MartenEventStoreOutboxProcessorService" />.
    /// </summary>
    /// <param name="databaseClient">Database to write event logs.</param>
    /// <param name="eventStorageClient">client to write events to event store.</param>
    /// <param name="logger">The logger.</param>
    public MartenEventStoreOutboxProcessorService(
        IFirstProjectionEventLogWriter databaseClient,
        IEventStorageClient eventStorageClient,
        ILogger<MartenEventStoreOutboxProcessorService> logger)
    {
        _Logger = logger;

        _EventStoreClient =
            eventStorageClient;

        _DatabaseClient = databaseClient;
    }

    private async Task UpdateBatch(EventBatch batch, EventStatus status, CancellationToken cancellationToken)
    {
        _Logger.EnterMethod();

        batch.Status = status;
        batch.UpdatedAt = DateTime.UtcNow;

        if (_Logger.IsEnabledForTrace())
        {
            _Logger.LogTraceMessage(
                "Update batch with id {id} and data: {data}",
                LogHelpers.Arguments(batch.Id, JsonConvert.SerializeObject(batch)));
        }

        await _DatabaseClient.UpdateBatchAsync(batch.Id, batch, cancellationToken);

        _Logger.ExitMethod();
    }

    private async Task ExecuteBatchForceAsync(string batchId, CancellationToken cancellationToken = default)
    {
        _Logger.EnterMethod();

        _Logger.LogInfoMessage(
            "Current batch with id {id} is the next batch to execute. Get events to send them to event store.",
            batchId.AsArgumentList());

        IEnumerable<EventLogTuple> results = await _DatabaseClient.GetEventsAsync(
            batchId,
            cancellationToken);

        int totalEvents = results.Count();

        _Logger.LogInfoMessage("Found {total} events to send to event store.", totalEvents.AsArgumentList());

        // Events are written separately to reduce problems regarding event and batch size.
        var currentEvent = 0;

        foreach (EventLogTuple eventTuple in results)
        {
            currentEvent++;

            _Logger.LogDebugMessage(
                "Write event with id {id}, type {type} and stream {stream} to event store ({current}/{total}).",
                LogHelpers.Arguments(
                    eventTuple.Id,
                    eventTuple.Type,
                    eventTuple.TargetStream,
                    currentEvent,
                    totalEvents));

            if (_Logger.IsEnabledForTrace())
            {
                _Logger.LogTraceMessage(
                    "Write event with id {id}, type {type} and stream {stream} to event store. Content: {content}",
                    LogHelpers.Arguments(
                        eventTuple.Id,
                        eventTuple.Type,
                        eventTuple.TargetStream,
                        JsonConvert.SerializeObject(eventTuple)));
            }

            bool eventProcessed = eventTuple.Status == EventStatus.Executed
                || eventTuple.Status == EventStatus.Aborted
                || eventTuple.Status == EventStatus.Error;

            if (eventProcessed)
            {
                _Logger.LogDebugMessage(
                    "Event with id {id} is already processed and will be skipped. Status is: {status}",
                    LogHelpers.Arguments(eventTuple.Id, eventTuple.Status));

                continue;
            }

            try
            {
                await _EventStoreClient.WriteEventAsync(eventTuple.Event, eventTuple.TargetStream, cancellationToken);

                _Logger.LogDebugMessage(
                    "Event was written to event store with id {id}, type {type} and stream {stream}.",
                    LogHelpers.Arguments(eventTuple.Id, eventTuple.Type, eventTuple.TargetStream));

                eventTuple.Status = EventStatus.Executed;
                eventTuple.UpdatedAt = DateTime.UtcNow;

                _Logger.LogTraceMessage(
                    "Try to update event with id {id} in database.",
                    eventTuple.Id.AsArgumentList());

                // TODO: How to handle error with event store -> one operation failed?
                await _DatabaseClient.UpdateEventAsync(eventTuple.Id, eventTuple, cancellationToken);
            }
            catch (Exception ex)
            {
                // TODO: How to handle.
                _Logger.LogErrorMessage(
                    ex,
                    "An error occurred while processing event with id {id}, type {type} and stream {stream} to event store. Batch id is: {batchId}.",
                    LogHelpers.Arguments(
                        eventTuple.Id,
                        eventTuple.Type,
                        eventTuple.TargetStream,
                        batchId));
            }
        }

        _Logger.LogInfoMessage(
            "Batch {batchId} committed.",
            batchId.AsArgumentList());

        _Logger.ExitMethod();
    }

    /// <inheritdoc />
    public async Task CheckAndProcessEvents(CancellationToken cancellationToken = default)
    {
        _Logger.EnterMethod();

        _Logger.LogDebugMessage("Wait for lock to check and process batch.", LogHelpers.Arguments());

        await _SyncObject.WaitAsync(cancellationToken);

        _Logger.LogDebugMessage("Got lock for lock to check and process batch.", LogHelpers.Arguments());

        try
        {
            while (true)
            {
                (bool success, EventBatch nextBatch) =
                    await _DatabaseClient.TryGetNextCommittedBatchAsync(cancellationToken);

                if (!success)
                {
                    _Logger.LogDebugMessage(
                        "No batch to execute - skipping method.",
                        LogHelpers.Arguments());

                    break;
                }

                _Logger.LogDebugMessage(
                    "Found batch with id {id} to process in database",
                    nextBatch?.Id.AsArgumentList());

                _Logger.LogInfoMessage("Execute batch with id {id}", nextBatch.Id.AsArgumentList());

                if (_Logger.IsEnabledForTrace())
                {
                    _Logger.LogTraceMessage(
                        "Execute batch with id {id} and data: {data}",
                        LogHelpers.Arguments(nextBatch.Id, JsonConvert.SerializeObject(nextBatch)));
                }

                await UpdateBatch(nextBatch, EventStatus.Processing, cancellationToken);

                await ExecuteBatchForceAsync(nextBatch.Id, cancellationToken);

                await UpdateBatch(nextBatch, EventStatus.Executed, cancellationToken);
            }
        }
        finally
        {
            _Logger.LogDebugMessage("Release lock for check and process batch.", LogHelpers.Arguments());
            _SyncObject.Release();
        }

        _Logger.ExitMethod();
    }
}
