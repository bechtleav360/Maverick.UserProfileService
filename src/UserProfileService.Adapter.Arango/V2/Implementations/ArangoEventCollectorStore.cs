using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.Public;
using Maverick.Client.ArangoDb.Public.Extensions;
using Maverick.Client.ArangoDb.Public.Models.Document;
using Maverick.Client.ArangoDb.Public.Models.Query;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UserProfileService.Adapter.Arango.V2.Abstractions;
using UserProfileService.Adapter.Arango.V2.Contracts;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.EventCollector.Abstractions;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Adapter.Arango.V2.Implementations;

internal class ArangoEventCollectorStore : ArangoRepositoryBase, IEventCollectorStore
{
    private readonly IDbInitializer _initializer;
    private readonly ModelBuilderOptions _modelsInfo;

    /// <inheritdoc cref="ArangoRepositoryBase.ArangoDbClientName" />
    protected override string ArangoDbClientName { get; }

    [ActivatorUtilitiesConstructor]
    public ArangoEventCollectorStore(
        ILogger<ArangoEventCollectorStore> logger,
        IServiceProvider serviceProvider,
        IDbInitializer initializer)
        : this(
            logger,
            serviceProvider,
            initializer,
            ArangoConstants.DatabaseClientNameSagaWorker,
            WellKnownDatabaseKeys.CollectionPrefixUserProfileService)
    {
    }

    public ArangoEventCollectorStore(
        ILogger<ArangoEventCollectorStore> logger,
        IServiceProvider serviceProvider,
        IDbInitializer initializer,
        string clientName,
        string collectionPrefix) : base(logger, serviceProvider)
    {
        ArangoDbClientName = clientName;
        _modelsInfo = DefaultModelConstellation.CreateNewEventCollectorStore(collectionPrefix).ModelsInfo;
        _initializer = initializer;
    }

    private ParameterizedAql GetCountQuery(string collectingId)
    {
        string collection = _modelsInfo.GetCollectionName<EventData>();

        if (string.IsNullOrWhiteSpace(collection))
        {
            throw new ArgumentNullException(nameof(collection));
        }

        return new ParameterizedAql
        {
            Query =
                $@"RETURN COUNT(FOR e IN @@collection FILTER e.{
                    nameof(EventData.CollectingId)
                } == @collectingId return e)",
            Parameter = new Dictionary<string, object>
            {
                { "collectingId", collectingId },
                { "@collection", collection }
            }
        };
    }

    private ParameterizedAql UpdateCollectingItemsAmountQuery(Guid collectingId, int collectingAmount)
    {
        string collection = _modelsInfo.GetCollectionName<StartCollectingEventData>();

        if (string.IsNullOrWhiteSpace(collection))
        {
            throw new ArgumentNullException(nameof(collection));
        }

        return new ParameterizedAql
        {
            Query =
                $@"
                            FOR x IN @@collection 
                            FILTER x.{
                                nameof(StartCollectingEventData.CollectingId)
                            } == @collectingId AND
                            x.{
                                nameof(StartCollectingEventData.CollectItemsAccount)
                            } == null
                            UPDATE x WITH {{
                            {
                                nameof(StartCollectingEventData.CollectItemsAccount)
                            }: @collectingAmount}} IN @@collection
                            RETURN x",

            Parameter = new Dictionary<string, object>
            {
                { "collectingId", collectingId },
                { "@collection", collection },
                { "collectingAmount", collectingAmount }
            }
        };
    }

    private async Task<IReadOnlyList<TResultItem>> ExecuteAqlQueryAsync<TResultItem>(
        ParameterizedAql aql,
        string transactionId = null,
        bool throwException = true,
        bool throwExceptionIfNotFound = false,
        CancellationToken cancellationToken = default,
        [CallerMemberName] string caller = null)
    {
        Logger.EnterMethod();

        var cursorBody = new CreateCursorBody
        {
            Query = aql.Query,
            BindVars = aql.Parameter ?? new Dictionary<string, object>()
        };

        if (Logger.IsEnabledFor(LogLevel.Debug))
        {
            Logger.LogDebugMessage(
                "Executing AQL query (in behalf of {behalfOf}): {aql}",
                Arguments(caller, aql.Query));
        }

        // maybe sensitive data - who knows?!
        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTraceMessage(
                "Used parameter set of AQL query (in behalf of {behalfOf}): {aqlParameter}",
                Arguments(caller, aql.Parameter.ToLogString()));
        }

        cancellationToken.ThrowIfCancellationRequested();

        MultiApiResponse<TResultItem> response = await SendRequestAsync(
            c => c.ExecuteQueryWithCursorOptionsAsync<TResultItem>(
                cursorBody,
                transactionId,
                cancellationToken: cancellationToken),
            throwException,
            throwExceptionIfNotFound,
            CallingServiceContext.CreateNewOf<ArangoEventCollectorStore>(),
            cancellationToken);

        return Logger.ExitMethod(response.QueryResult);
    }

    /// <inheritdoc />
    public async Task<ICollection<EventData>> GetEventData(
        string processId,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (processId == null)
        {
            throw new ArgumentNullException(nameof(processId));
        }

        if (string.IsNullOrWhiteSpace(processId))
        {
            throw new ArgumentException(nameof(processId));
        }

        string collection = _modelsInfo.GetCollectionName<EventData>();

        if (string.IsNullOrWhiteSpace(collection))
        {
            throw new ArgumentNullException(nameof(collection));
        }

        var paramAql = new ParameterizedAql
        {
            Query =
                $"FOR e IN @@collection FILTER e.{nameof(EventData.CollectingId)} == @processId RETURN e",
            Parameter = new Dictionary<string, object>
            {
                { "processId", processId },
                { "@collection", collection }
            }
        };

        IReadOnlyList<EventData> result = await ExecuteAqlQueryAsync<EventData>(
            paramAql,
            cancellationToken: cancellationToken);

        return Logger.ExitMethod(result.ToList());
    }

    /// <inheritdoc />
    public async Task<int> GetCountOfEventDataAsync(string collectingId, CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (collectingId == null)
        {
            throw new ArgumentNullException(nameof(collectingId));
        }

        if (string.IsNullOrWhiteSpace(collectingId))
        {
            throw new ArgumentException(nameof(collectingId));
        }

        ParameterizedAql paramAql = GetCountQuery(collectingId);

        IReadOnlyList<int> result = await ExecuteAqlQueryAsync<int>(
            paramAql,
            cancellationToken: cancellationToken);

        return Logger.ExitMethod(result.First());
    }

    /// <inheritdoc />
    public async Task<bool> TrySetCollectingItemsAmountAsync(
        Guid collectingId,
        int collectingAmount,
        CancellationToken cancellationToken = default)
    {
        if (collectingId == Guid.Empty)
        {
            throw new ArgumentException(nameof(collectingId));
        }

        ParameterizedAql paramAql = UpdateCollectingItemsAmountQuery(collectingId, collectingAmount);

        IReadOnlyList<StartCollectingEventData> result = await ExecuteAqlQueryAsync<StartCollectingEventData>(
            paramAql,
            cancellationToken: cancellationToken);

        return Logger.ExitMethod(result.Any());
    }

    /// <inheritdoc />
    public async Task<bool> SetTerminateTimeForCollectingItemsProcessAsync(
        Guid collectingId,
        CancellationToken cancellationToken = default)
    {
        if (collectingId == Guid.Empty)
        {
            throw new ArgumentException(nameof(collectingId));
        }

        string collectionName = _modelsInfo.GetCollectionName<StartCollectingEventData>();
        var dateNow = DateTime.UtcNow.ToString("O");

        var paramAql = new ParameterizedAql
        {
            Query =
                $@"
                            FOR collectingEvent IN @@collection 
                            FILTER collectingEvent.{
                                nameof(StartCollectingEventData.CollectingId)
                            } == @collectingId 
                            UPDATE collectingEvent WITH {{
                            {
                                nameof(StartCollectingEventData.CompletedAt)
                            }: @dateNow}} IN @@collection
                            RETURN NEW",

            Parameter = new Dictionary<string, object>
            {
                { "collectingId", collectingId },
                { "@collection", collectionName },
                { "dateNow", dateNow }
            }
        };

        IReadOnlyList<StartCollectingEventData> result = await ExecuteAqlQueryAsync<StartCollectingEventData>(
            paramAql,
            cancellationToken: cancellationToken);

        return Logger.ExitMethod(result.Any());
    }

    /// <inheritdoc />
    public async Task<TEntity> GetEntityAsync<TEntity>(
        string id,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentNullException(nameof(id));
        }

        string collectionName = _modelsInfo.GetCollectionName<TEntity>();

        var context = CallingServiceContext.CreateNewOf<ArangoEventCollectorStore>();

        GetDocumentResponse<TEntity> response = await SendRequestAsync(
            c => c.GetDocumentAsync<TEntity>($"{collectionName}/{id}"),
            false,
            false,
            context,
            cancellationToken);

        await CheckAResponseAsync(
            response,
            context: context,
            cancellationToken: cancellationToken);

        return Logger.ExitMethod(response.Result);
    }

    /// <inheritdoc />
    public async Task SaveEntityAsync<TEntity>(
        TEntity entity,
        string entityId = null,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        await _initializer.EnsureDatabaseAsync(false, cancellationToken);
        string collection = _modelsInfo.GetCollectionName<TEntity>();

        await GetArangoDbClient()
            .CreateDocumentAsync(collection, entity.InjectDocumentKey(_ => entityId ?? Guid.NewGuid().ToString()));
    }

    /// <inheritdoc />
    public async Task<string> GetExternalProcessIdAsync(
        string collectingId,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (collectingId == null)
        {
            throw new ArgumentNullException(nameof(collectingId));
        }

        string collectionName = _modelsInfo.GetCollectionName<StartCollectingEventData>();

        var paramAql = new ParameterizedAql
        {
            Query =
                $@"FOR e IN @@collection FILTER e.{
                    nameof(StartCollectingEventData.CollectingId)
                } == @collectingId 
                                   RETURN e.{
                                       nameof(StartCollectingEventData.ExternalProcessId)
                                   }",
            Parameter = new Dictionary<string, object>
            {
                { "collectingId", collectingId },
                { "@collection", collectionName }
            }
        };

        IReadOnlyList<string> result = await ExecuteAqlQueryAsync<string>(
            paramAql,
            cancellationToken: cancellationToken);

        return Logger.ExitMethod<string>(result.First());
    }

    /// <inheritdoc />
    public async Task<int> SaveEventDataAsync(EventData data, CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        await _initializer.EnsureDatabaseAsync(false, cancellationToken);

        string collection = _modelsInfo.GetCollectionName<EventData>();

        if (string.IsNullOrWhiteSpace(collection))
        {
            throw new ArgumentNullException(nameof(collection));
        }

        var paramAql = new ParameterizedAql
        {
            Query =
                $@"
                                    let items = Count(FOR e IN @@collection filter e.{
                                        nameof(EventData.CollectingId)
                                    } == @processId RETURN e)
                                    INSERT {{ 
                                                _key: @key, 
                                                {
                                                    nameof(EventData.CollectingId)
                                                } : @processId, 
                                                {
                                                    nameof(EventData.RequestId)
                                                } : @requestId,
                                                {
                                                    nameof(EventData.Data)
                                                } : @data, 
                                                {
                                                    nameof(EventData.Host)
                                                } : @host, 
                                                {
                                                    nameof(EventData.ErrorOccurred)
                                                } : @errorOccurred 
                                           }} 
                                           INTO @@collection
                                    RETURN items + 1",
            Parameter = new Dictionary<string, object>
            {
                { "key", Guid.NewGuid() },
                { "processId", data.CollectingId },
                { "requestId", data.RequestId },
                { "data", data.Data },
                { "host", data.Host },
                { "errorOccurred", data.ErrorOccurred },
                { "@collection", collection }
            }
        };

        IReadOnlyList<int> result = await ExecuteAqlQueryAsync<int>(
            paramAql,
            cancellationToken: cancellationToken);

        return Logger.ExitMethod(result.First());
    }
}
