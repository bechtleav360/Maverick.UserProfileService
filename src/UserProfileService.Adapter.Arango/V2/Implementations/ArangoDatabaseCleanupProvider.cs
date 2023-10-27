using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.Public;
using Maverick.Client.ArangoDb.Public.Models.Document;
using Maverick.Client.ArangoDb.Public.Models.Query;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserProfileService.Adapter.Arango.V2.Abstractions;
using UserProfileService.Adapter.Arango.V2.Configuration;
using UserProfileService.Adapter.Arango.V2.Contracts;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Enums;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Adapter.Arango.V2.Implementations;

internal class ArangoDatabaseCleanupProvider : ArangoRepositoryBase, IDatabaseCleanupProvider
{
    private readonly ArangoDbCleanupConfiguration _configuration;

    public ArangoDatabaseCleanupProvider(
        IOptionsSnapshot<ArangoDbCleanupConfiguration> configuration,
        ILogger<ArangoDatabaseCleanupProvider> logger,
        IServiceProvider serviceProvider)
        : base(logger, serviceProvider)
    {
        _configuration = configuration.Value;
    }

    private async Task CleanupFirstLevelAsync(
        DateTime dateFilter,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        ModelBuilderOptions modelsInfoState = DefaultModelConstellation
            .CreateNewFirstLevelProjection(_configuration.FirstLevelCollectionPrefix)
            .ModelsInfo;

        ModelBuilderOptions modelsInfoBatch = DefaultModelConstellation
            .NewEventLogStore(_configuration.FirstLevelCollectionPrefix)
            .ModelsInfo;

        IReadOnlyList<ArangoIdentifier> stateElementsToBeDeletedResponse = await
            GetUnnecessaryDocsByQueryAsync(
                WellKnownAqlQueries.GetQueryToGetFirstLevelProjectionStateItemsForCleanup(
                    modelsInfoState,
                    dateFilter),
                "First-level-projection",
                cancellationToken);

        IReadOnlyList<ArangoIdentifier> batchElementsToBeDeletedResponse = await
            GetUnnecessaryDocsByQueryAsync(
                WellKnownAqlQueries.GetQueryToGetFirstLevelBatchLogItemsForCleanup(
                    modelsInfoBatch,
                    dateFilter,
                    EventStatus.Executed,
                    EventStatus.Aborted),
                "First-level-projection",
                cancellationToken);

        List<ArangoIdentifier> elementsToBeDeletedResponse = stateElementsToBeDeletedResponse
            .Concat(batchElementsToBeDeletedResponse)
            .ToList();

        await DeleteDocumentsAsync(elementsToBeDeletedResponse, cancellationToken);

        Logger.ExitMethod();
    }

    private async Task CleanupEventCollectorAsync(
        DateTime dateFilter,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        ModelBuilderOptions modelsInfo = DefaultModelConstellation
            .CreateNewEventCollectorStore(_configuration.EventCollectorCollectionPrefix)
            .ModelsInfo;

        IReadOnlyList<ArangoIdentifier> elementsToBeDeletedResponse = await
            GetUnnecessaryDocsByQueryAsync(
                WellKnownAqlQueries.GetQueryToGetEventCollectorItemsForCleanup(modelsInfo, dateFilter),
                "EventCollector",
                cancellationToken);

        await DeleteDocumentsAsync(elementsToBeDeletedResponse, cancellationToken);

        Logger.ExitMethod();
    }

    private async Task DeleteDocumentsAsync(
        IReadOnlyList<ArangoIdentifier> idsToBeDeleted,
        CancellationToken cancellationToken,
        [CallerMemberName] string caller = null)
    {
        Logger.EnterMethod();

        if (idsToBeDeleted == null
            || !idsToBeDeleted.Any())
        {
            Logger.LogDebugMessage(
                "No entities to be deleted during method {cleanupMethod}.",
                caller.AsArgumentList());

            Logger.ExitMethod();

            return;
        }

        Dictionary<string, string[]> elementsToBeDeleted =
            idsToBeDeleted
                .GroupBy(
                    i => i.CollectionName,
                    i => i.Key,
                    (collection, identifiers)
                        => new KeyValuePair<string, string[]>(collection, identifiers.ToArray()))
                .ToDictionary(o => o.Key, o => o.Value);

        foreach ((string collectionName, string[] identifiers) in elementsToBeDeleted)
        {
            DeleteDocumentsResponse deleteResponse = await SendRequestAsync(
                c => c.DeleteDocumentsAsync(
                    collectionName,
                    identifiers,
                    new DeleteDocumentOptions
                    {
                        ReturnOld = Logger.IsEnabledForDebug()
                    }),
                false,
                false,
                CallingServiceContext.CreateNewOf<ArangoDatabaseCleanupProvider>(),
                cancellationToken);

            if (deleteResponse.Error)
            {
                Logger.LogWarnMessage(
                    "Could not delete documents in collection {collectionName} during cleaned process. Got HTTP status code:{httpCode} {errorMessage}",
                    LogHelpers.Arguments(collectionName, deleteResponse.Code, deleteResponse.Exception?.Message));

                Logger.ExitMethod();

                return;
            }

            if (Logger.IsEnabledForDebug()
                && deleteResponse.Result?.Any() == true)
            {
                foreach (DocumentBase documentBase in deleteResponse.Result)
                {
                    Logger.LogDebugMessage(
                        "Deleted document during method {cleanupMethod}: {arangoInternalId}",
                        LogHelpers.Arguments(caller, documentBase.Id));
                }

                Logger.LogInfoMessage(
                    "Deleted documents during method {cleanupMethod}: {deletedDocAmount}",
                    LogHelpers.Arguments(caller, deleteResponse.Result.Count));
            }
        }

        Logger.ExitMethod();
    }

    private async Task<IReadOnlyList<ArangoIdentifier>> GetUnnecessaryDocsByQueryAsync(
        ParameterizedAql queryDetails,
        string cleanupContext,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        Logger.LogDebugMessage(
            "Sending AQL query via ArangoDb cursor API: {aqlQueryString}",
            LogHelpers.Arguments(queryDetails.Query));

        MultiApiResponse<ArangoIdentifier> response = await SendRequestAsync(
            c => c.ExecuteQueryWithCursorOptionsAsync<ArangoIdentifier>(
                new CreateCursorBody
                {
                    BindVars = queryDetails.Parameter,
                    Query = queryDetails.Query
                },
                cancellationToken: cancellationToken),
            false,
            false,
            CallingServiceContext.CreateNewOf<ArangoDatabaseCleanupProvider>(),
            cancellationToken);

        if (response.Error
            || response.Responses == null
            || response.Responses.Any(r => r.Error))
        {
            Logger.LogWarnMessage(
                $"Could not determine collections to be cleaned ({cleanupContext})",
                LogHelpers.Arguments());

            return Logger.ExitMethod(new List<ArangoIdentifier>());
        }

        return Logger.ExitMethod(response.QueryResult);
    }

    /// <inheritdoc />
    public async Task CleanupAsync(CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (_configuration.EventCollectorCollections == null
            && _configuration.AssignmentProjectionCollection == null
            && _configuration.FirstLevelProjectionCollection == null
            && _configuration.ServiceProjectionCollection == null)
        {
            Logger.LogInfoMessage(
                "No cleanup jobs configured - skipping operation",
                LogHelpers.Arguments());

            Logger.ExitMethod();

            return;
        }

        if (_configuration.EventCollectorCollections != null)
        {
            Logger.LogInfoMessage(
                "Starting to cleanup event-collector collections",
                LogHelpers.Arguments());

            await CleanupEventCollectorAsync(
                DateTime.UtcNow
                    .Add(-_configuration.EventCollectorCollections.Value),
                cancellationToken);
        }

        if (_configuration.FirstLevelProjectionCollection != null)
        {
            Logger.LogInfoMessage(
                "Starting to cleanup first-level-projection collections",
                LogHelpers.Arguments());

            await CleanupFirstLevelAsync(
                DateTime.UtcNow
                    .Add(-_configuration.FirstLevelProjectionCollection.Value),
                cancellationToken);
        }

        if (_configuration.AssignmentProjectionCollection != null)
        {
            Logger.LogInfoMessage(
                "Cleanup of assignment projection collections is not supported yet",
                LogHelpers.Arguments());
        }

        if (_configuration.ServiceProjectionCollection != null)
        {
            Logger.LogInfoMessage(
                "Cleanup of service-projection collections is not supported yet",
                LogHelpers.Arguments());
        }
        Logger.ExitMethod();
    }
}
