using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.Public;
using Maverick.Client.ArangoDb.Public.Extensions;
using Maverick.Client.ArangoDb.Public.Models;
using Maverick.Client.ArangoDb.Public.Models.Document;
using Maverick.Client.ArangoDb.Public.Models.Query;
using Maverick.Client.ArangoDb.Public.Models.Transaction;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using UserProfileService.Adapter.Arango.V2.Abstractions;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Enums;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Common.V2.Models;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Adapter.Arango.V2.Implementations;

/// <summary>
///     Arango implementation for <see cref="IFirstProjectionEventLogWriter" />.
/// </summary>
public class ArangoEventLogStore : ArangoRepositoryBase, IFirstProjectionEventLogWriter
{
    private readonly string _batchCollectionName;
    private readonly string _batchToEventEdgeCollectionName;
    private readonly IDbInitializer _dbInitializer;
    private readonly string _eventCollectionName;

    private IArangoDbClient Client => GetArangoDbClient();

    /// <inheritdoc />
    protected override string ArangoDbClientName { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ArangoEventLogStore" />.
    /// </summary>
    /// <param name="logger">The logger instance to be used for writing logging messages to.</param>
    /// <param name="serviceProvider">
    ///     The service provider is needed to create an <see cref="IArangoDbClientFactory" /> that
    ///     manages <see cref="IArangoDbClient" />s.
    /// </param>
    /// <param name="clientName">Name of arango client to use.</param>
    /// <param name="collectionPrefix">Prefix for collection to use.</param>
    public ArangoEventLogStore(
        ILogger<ArangoEventLogStore> logger,
        IServiceProvider serviceProvider,
        string clientName,
        string collectionPrefix) : base(
        logger,
        serviceProvider)
    {
        ModelBuilderOptions modelsInfo = DefaultModelConstellation.NewEventLogStore(collectionPrefix).ModelsInfo;

        _eventCollectionName = modelsInfo.GetCollectionName(typeof(EventLogTuple));
        _batchCollectionName = modelsInfo.GetCollectionName(typeof(EventBatch));
        _batchToEventEdgeCollectionName = modelsInfo.GetRelation<EventBatch, EventLogTuple>().EdgeCollection;

        _dbInitializer = serviceProvider.GetRequiredService<IDbInitializer>();
        ArangoDbClientName = clientName;
    }

    private static EventBatch InitializeBatch(string batchId)
    {
        return new EventBatch
        {
            Status = EventStatus.Initialized,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Id = batchId
        };
    }

    private async Task CreateEdges(
        string batchId,
        IEnumerable<EventLogTuple> eventsToBeAdded,
        IRunningTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        string fromKey = GenerateArangoKey(_batchCollectionName, batchId);

        foreach (EventLogTuple eventLogTuple in eventsToBeAdded)
        {
            string toKey = GenerateArangoKey(_eventCollectionName, eventLogTuple.Id);

            await ExecuteAsync(
                client =>
                    client.CreateEdgeAsync(
                        _batchToEventEdgeCollectionName,
                        fromKey,
                        toKey,
                        new CreateDocumentOptions
                        {
                            Overwrite = true,
                            OverWriteMode = AOverwriteMode.Replace
                        },
                        transaction?.GetTransactionId()),
                true,
                true,
                cancellationToken);
        }
    }

    private static string GenerateArangoKey(string collectionName, string id)
    {
        return $"{collectionName}/{id}";
    }

    private IList<JObject> PrepareEvents(IEnumerable<EventLogTuple> events)
    {
        return events
            .Select(
                s =>
                {
                    JObject jObject = s.InjectDocumentKey(t => t.Id, Client.UsedJsonSerializerSettings);

                    return jObject;
                })
            .ToList();
    }

    private async Task AddEventsAsync(
        string batchId,
        IList<EventLogTuple> eventsToBeAdded,
        IRunningTransaction transaction = null,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (string.IsNullOrWhiteSpace(batchId))
        {
            throw new ArgumentNullException(nameof(batchId), "Batch id can not be null");
        }

        if (!eventsToBeAdded.Any())
        {
            throw new ArgumentException("Events can not be empty", nameof(eventsToBeAdded));
        }

        if (eventsToBeAdded.Any(e => e.BatchId != batchId))
        {
            throw new ArgumentException("All events that are added must have the same batch id.");
        }

        if (transaction != null && string.IsNullOrWhiteSpace(transaction.GetTransactionId()))
        {
            throw new ArgumentException("Id of transaction can not be null or whitespace.", nameof(transaction));
        }

        IList<JObject> convertedEvents = PrepareEvents(eventsToBeAdded);

        var transactionCollections = new List<string>
        {
            _eventCollectionName,
            _batchToEventEdgeCollectionName
        };

        bool executeTransaction = transaction == null;

        transaction ??=
            await Client.BeginTransactionAsync(transactionCollections, transactionCollections);

        CheckTransaction(transaction);

        await ExecuteAsync(
            client =>
                client.CreateDocumentsAsync(
                    _eventCollectionName,
                    convertedEvents,
                    new CreateDocumentOptions
                    {
                        Overwrite = true,
                        OverWriteMode = AOverwriteMode.Replace
                    },
                    transaction.GetTransactionId()),
            true,
            true,
            cancellationToken);

        await CreateEdges(batchId, eventsToBeAdded, transaction, cancellationToken);

        await ExecuteAsync(
            client =>
                client.CommitTransactionAsync(transaction.GetTransactionId()),
            true,
            true,
            cancellationToken);

        if (executeTransaction)
        {
            await Client.CommitTransactionAsync(transaction.GetTransactionId());
        }

        Logger.ExitMethod();
    }

    private static void CheckTransaction(IRunningTransaction transaction)
    {
        if (transaction.Exception != null)
        {
            throw new DatabaseException(
                "Transaction could not be started.",
                transaction.Exception,
                ExceptionSeverity.Error);
        }

        if (string.IsNullOrWhiteSpace(transaction.GetTransactionId()))
        {
            throw new DatabaseException(
                "Id of transaction could not be null or white space.",
                transaction.Exception,
                ExceptionSeverity.Error);
        }
    }

    private async Task ExecuteAsync<TResult>(
        Func<IArangoDbClient, Task<TResult>> method,
        bool throwException,
        bool throwExceptionIfNotFound,
        CancellationToken cancellationToken,
        [CallerMemberName] string caller = null)
        where TResult : IApiResponse
    {
        Logger.EnterMethod();

        await _dbInitializer.EnsureDatabaseAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        TResult response = await SendRequestAsync(
            method,
            throwException,
            throwExceptionIfNotFound,
            CallingServiceContext.CreateNewOf<ArangoEventLogStore>(),
            cancellationToken,
            caller);

        Logger.ExitMethod(response);
    }

    /// <inheritdoc />
    public async Task<EventBatch> GetNextCommittedBatchAsync(CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        (bool success, EventBatch batch) = await TryGetNextCommittedBatchAsync(cancellationToken);

        if (!success)
        {
            throw new InstanceNotFoundException("Found no batch to execute");
        }

        return Logger.ExitMethod(batch);
    }

    /// <inheritdoc />
    public async Task<(bool success, EventBatch result)> TryGetNextCommittedBatchAsync(
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        await _dbInitializer.EnsureDatabaseAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        var vars = new Dictionary<string, object>
        {
            { "@batchCollection", _batchCollectionName },
            { "statusCommitted", EventStatus.Committed.ToString() },
            { "statusProcessing", EventStatus.Processing.ToString() }
        };

        const string query = $@" WITH @@batchCollection
                            FOR eventBatch in @@batchCollection 
                               FILTER eventBatch.{
                                   nameof(EventBatch.Status)
                               } == @statusCommitted ||  eventBatch.{
                                   nameof(EventBatch.Status)
                               } == @statusProcessing
                               SORT   eventBatch.{
                                   nameof(EventBatch.UpdatedAt)
                               }
                               LIMIT 1
                               RETURN eventBatch";

        var cursorBody = new CreateCursorBody
        {
            Count = false,
            Query = query,
            BatchSize = 10,
            Cache = false,
            Ttl = 60,
            BindVars = vars
        };

        MultiApiResponse<EventBatch> result =
            await Client.ExecuteQueryWithCursorOptionsAsync<EventBatch>(
                cursorBody,
                cancellationToken: cancellationToken);

        EventBatch batchEntity = result.QueryResult.FirstOrDefault();

        if (batchEntity == null)
        {
            Logger.LogDebugMessage("Found no batch to execute.", LogHelpers.Arguments());

            return (false, null);
        }

        Logger.LogDebugMessage("Found batch to execute with id {id}.", batchEntity.Id.AsArgumentList());

        return Logger.ExitMethod((true, batchEntity));
    }

    /// <inheritdoc />
    public async Task<EventBatch> GetBatchAsync(string batchId, CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        await _dbInitializer.EnsureDatabaseAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        string dbId = GenerateArangoKey(_batchCollectionName, batchId);

        GetDocumentResponse<EventBatch> result = await Client.GetDocumentAsync<EventBatch>(dbId);

        if (result == null || result.Code == HttpStatusCode.NotFound)
        {
            Logger.LogDebugMessage("Not batch found for id {id} in database.", batchId.AsArgumentList());

            throw new InstanceNotFoundException($"Not batch found for id {batchId} in database.");
        }

        EventBatch batch = result.Result;

        Logger.LogDebugMessage("Found batch with id {id} in database.", batchId.AsArgumentList());

        return Logger.ExitMethod(batch);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EventLogTuple>> GetEventsAsync(
        string transactionId,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (string.IsNullOrWhiteSpace(transactionId))
        {
            throw new ArgumentException("Transaction id can not be a null or empty.", nameof(transactionId));
        }

        await _dbInitializer.EnsureDatabaseAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        var vars = new Dictionary<string, object>
        {
            { "@batchCollection", _batchCollectionName },
            { "@edgeCollection", _batchToEventEdgeCollectionName },
            { "@eventCollection", _eventCollectionName },
            { "transactionId", transactionId }
        };

        var query = $@"
                        WITH  @@batchCollection @@edgeCollection @@eventCollection
                        FOR eventBatch in @@batchCollection FILTER eventBatch._key == @transactionId 
                               FOR eventLogTuple IN 1..1 OUTBOUND eventBatch @@edgeCollection
                                   SORT eventLogTuple.{
                                       nameof(EventLogTuple.CreatedAt)
                                   } Asc
                                   RETURN eventLogTuple";

        var cursorBody = new CreateCursorBody
        {
            Count = true,
            Query = query,
            BatchSize = 10,
            Cache = true,
            Ttl = 60,
            BindVars = vars
        };

        MultiApiResponse<EventLogTuple> results = await Client.ExecuteQueryWithCursorOptionsAsync<EventLogTuple>(
            cursorBody,
            cancellationToken: cancellationToken);

        return Logger.ExitMethod(results.QueryResult);
    }

    /// <inheritdoc />
    public async Task<EventBatch> CreateBatchAsync(
        string batchId,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (string.IsNullOrWhiteSpace(batchId))
        {
            throw new ArgumentNullException(nameof(batchId), "Batch id can not be null or empty.");
        }

        EventBatch batch = InitializeBatch(batchId);

        await ExecuteAsync(
            client =>
                client.CreateDocumentAsync(
                    _batchCollectionName,
                    batch.InjectDocumentKey(t => t.Id, client.UsedJsonSerializerSettings),
                    new CreateDocumentOptions
                    {
                        Overwrite = true,
                        OverWriteMode = AOverwriteMode.Replace
                    }),
            true,
            true,
            cancellationToken);

        return Logger.ExitMethod(batch);
    }

    /// <inheritdoc />
    public async Task<EventBatch> CreateBatchAsync(
        string batchId,
        IList<EventLogTuple> eventsToBeAdded,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (string.IsNullOrWhiteSpace(batchId))
        {
            throw new ArgumentNullException(nameof(batchId), "Batch id can not be null or empty.");
        }

        if (eventsToBeAdded == null)
        {
            throw new ArgumentNullException(nameof(eventsToBeAdded), "Events can not be null");
        }

        if (!eventsToBeAdded.Any())
        {
            throw new ArgumentException("Events can not be empty", nameof(eventsToBeAdded));
        }

        EventBatch batch = InitializeBatch(batchId);

        var transactionCollections = new List<string>
        {
            _eventCollectionName,
            _batchCollectionName,
            _batchToEventEdgeCollectionName
        };

        TransactionOperationResponse transaction =
            await Client.BeginTransactionAsync(transactionCollections, transactionCollections);

        CheckTransaction(transaction);

        await ExecuteAsync(
            client =>
                client.CreateDocumentAsync(
                    _batchCollectionName,
                    batch.InjectDocumentKey(t => t.Id, client.UsedJsonSerializerSettings),
                    new CreateDocumentOptions
                    {
                        Overwrite = true,
                        OverWriteMode = AOverwriteMode.Replace
                    },
                    transaction.GetTransactionId()),
            true,
            true,
            cancellationToken);

        await AddEventsAsync(batchId, eventsToBeAdded, transaction, cancellationToken);

        await ExecuteAsync(
            client =>
                client.CommitTransactionAsync(transaction.GetTransactionId()),
            true,
            true,
            cancellationToken);

        return Logger.ExitMethod(batch);
    }

    /// <inheritdoc />
    public async Task<EventBatch> UpdateBatchAsync(
        string batchId,
        EventBatch batch,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (string.IsNullOrWhiteSpace(batchId))
        {
            throw new ArgumentException("Batch id can not be null or empty.", nameof(batchId));
        }

        if (batch == null)
        {
            throw new ArgumentNullException(nameof(batch), "Batch can not be null.");
        }

        await ExecuteAsync(
            client =>
                client.UpdateDocumentAsync(
                    _batchCollectionName,
                    batchId,
                    batch.InjectDocumentKey(t => t.Id, client.UsedJsonSerializerSettings),
                    new UpdateDocumentOptions
                    {
                        MergeObjects = false,
                        KeepNull = false
                    }),
            true,
            true,
            cancellationToken);

        return Logger.ExitMethod(batch);
    }

    /// <inheritdoc />
    public async Task AddEventsAsync(
        string batchId,
        IList<EventLogTuple> eventsToBeAdded,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        // All parameter will be checked in private method of AddEventsAsync

        await AddEventsAsync(batchId, eventsToBeAdded, null, cancellationToken);

        Logger.ExitMethod();
    }

    /// <inheritdoc />
    public async Task UpdateEventAsync(
        string id,
        EventLogTuple @event,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Id of event log can not be a empty or null.", nameof(id));
        }

        if (@event == null)
        {
            throw new ArgumentNullException(nameof(@event), "Event can not be null");
        }

        if (@event.Id != id)
        {
            throw new ArgumentException("Given id is not equal to the given id in the event.", nameof(id));
        }

        await ExecuteAsync(
            client =>
                client.UpdateDocumentAsync(
                    _eventCollectionName,
                    id,
                    @event,
                    new UpdateDocumentOptions
                    {
                        MergeObjects = false
                    }),
            true,
            true,
            cancellationToken);

        Logger.ExitMethod();
    }

    /// <inheritdoc />
    public async Task AbortBatchAsync(string batchId, CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (string.IsNullOrWhiteSpace(batchId))
        {
            throw new ArgumentException("Batch id can not be a null or empty.", nameof(batchId));
        }

        var vars = new Dictionary<string, object>
        {
            { "@batchCollection", _batchCollectionName },
            { "batchId", batchId },
            { "dateTimeNow", DateTime.UtcNow.ToString("o") },
            { "eventStatusAborted", EventStatus.Aborted.ToString() },
            { "eventStatusInit", EventStatus.Initialized.ToString() },
            { "@eventCollection", _eventCollectionName }
        };

        const string query = @$"
                For batch in @@batchCollection filter batch._key == @batchId
                    UPDATE {{ 
                                _key: batch._key, 
                                {
                                    nameof(EventBatch.Status)
                                }: @eventStatusAborted, 
                                {
                                    nameof(EventBatch.UpdatedAt)
                                }: @dateTimeNow 
                           }} IN @@batchCollection
                        
                    For eventLogTuple in @@eventCollection
                            FILTER eventLogTuple.{
                                nameof(EventLogTuple.BatchId)
                            } == batch._key 
                            AND eventLogTuple.{
                                nameof(EventLogTuple.Status)
                            } == @eventStatusInit
                                UPDATE {{ 
                                            _key: eventLogTuple._key, 
                                            {
                                                nameof(EventLogTuple.Status)
                                            }: @eventStatusAborted, 
                                            {
                                                nameof(EventLogTuple.UpdatedAt)
                                            }: @dateTimeNow 
                                       }} IN @@eventCollection";

        var body = new CreateCursorBody
        {
            Query = query,
            BindVars = vars
        };

        await ExecuteAsync(
            client =>
                client.ExecuteQueryWithCursorOptionsAsync<object>(
                    body,
                    cancellationToken: cancellationToken),
            true,
            true,
            cancellationToken);

        Logger.ExitMethod();
    }
}
