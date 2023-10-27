#if NETCOREAPP3_1
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arango.Client.Async.DotnetCore.Public;
using Arango.Client.Async.DotnetCore.Public.Extensions;
using Arango.Client.Async.DotnetCore.Public.Models.Document;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using UserProfileService.Adapter.Arango.V2.Abstractions;
using UserProfileService.Adapter.Arango.V2.Contracts;
using UserProfileService.Common.Logging;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Contracts;
using UserProfileService.Common.V2.Exceptions;

namespace UserProfileService.Adapter.Arango.V2.Implementations
{
    /// <summary>
    ///     The implementation of the saga status store.
    /// </summary>
    public class ArangoSagaStatusStore : ISagaStatusStore
    {
        private readonly IArangoDbClient _Client;
        private readonly ICollectionDetailsProvider _CollectionDetailsProvider;
        private readonly ILogger<ArangoSagaStatusStore> _Logger;
        private readonly IJsonSerializerSettingsProvider _SerializerSettingsProvider;

        /// <summary>
        ///     The constructor of the class.
        /// </summary>
        /// <param name="arangoClientFactory">The arango client factory to get a named arango client.</param>
        /// <param name="logger">The logger for logging purposes.</param>
        /// <param name="serializerSettingsProvider">Provides access to json serializer settings.</param>
        /// <param name="collectionDetailsProviders">A collection details provider <see cref="ICollectionDetailsProvider" />.</param>
        public ArangoSagaStatusStore(
            IArangoDbClientFactory arangoClientFactory,
            ILogger<ArangoSagaStatusStore> logger,
            IJsonSerializerSettingsProvider serializerSettingsProvider,
            IEnumerable<ICollectionDetailsProvider> collectionDetailsProviders)
        {
            _Client = arangoClientFactory.Create(ArangoConstants.SagaArangoClientName);
            _Logger = logger;
            _SerializerSettingsProvider = serializerSettingsProvider;

            _CollectionDetailsProvider =
                collectionDetailsProviders?.FirstOrDefault(p => p is SagaCollectionsProvider)
                ?? throw new Exception(
                    $"{nameof(SagaCollectionsProvider)} is missing in {nameof(IServiceCollection)}.");
        }

        private string GetCollectionName()
        {
            _Logger.EnterMethod();

            string collectionName =
                _CollectionDetailsProvider.GetCollectionDetails()
                    .FirstOrDefault(cd => cd.CollectionName.Contains(ArangoConstants.SagaStatusStoreCollectionName))
                    ?
                    .CollectionName;

            if (string.IsNullOrEmpty(collectionName))
            {
                throw new ConfigurationException(
                    $"Configuration not valid: missing collection name for the {nameof(ArangoSagaStatusStore)}");
            }

            return _Logger.ExitMethod<string>(collectionName);
        }

        /// <inheritdoc cref="ISagaStatusStore" />
        public async Task<SagaStatus> GetSagaStatusBySagaId(string sagaId, SagaState? state = null)
        {
            _Logger.EnterMethod();

            if (string.IsNullOrEmpty(sagaId))
            {
                _Logger.LogErrorMessage(
                    null,
                    "The variable with the name {sagaId} was null or empty. Saga couldn't be retrieved.",
                    LogHelpers.Arguments(nameof(sagaId)));

                throw new ArgumentNullException(nameof(sagaId));
            }

            MultiApiResponse<List<SagaStatus>> response = await _Client.ExecuteQueryAsync<List<SagaStatus>>(
                $"FOR log IN {GetCollectionName()} FILTER log.{nameof(SagaStatus.Id)} == \"{sagaId}\" RETURN log");

            if (response.Responses.Any(x => x.Error))
            {
                _Logger.LogErrorMessage(
                    null,
                    "Problems occurred retrieving the collection {collectionName}",
                    LogHelpers.Arguments(GetCollectionName()));

                throw response.Responses?.FirstOrDefault()?.Exception
                    ?? new Exception($"Problems occurred retrieving the collection {GetCollectionName()}.");
            }

            IEnumerable<SagaStatus> query = response.QueryResult.SelectMany(x => x);

            if (state != null)
            {
                query = query.Where(saga => saga.StatusCode == (int)state).ToList();
            }

            SagaStatus sagaStatus = query.OrderByDescending(saga => saga.ModifiedAt).FirstOrDefault();

            if (sagaStatus == null)
            {
                _Logger.LogWarnMessage(
                    "No instance of a saga status could be found for the saga id {sagaId}.",
                    LogHelpers.Arguments(sagaId));

                throw new InstanceNotFoundException(
                    $"No instance of a saga status could be found for the saga id {sagaId}.");
            }

            _Logger.LogDebugMessage(
                "Found the sagaStatusStatusStatus with the following parameter:"
                + "Status: {Status}, "
                + "CorrelationId: {CorrelationId}, "
                + "SagaStatusId: {sagaStatus}"
                + "ModifiedAt: {ModifiedAt}, "
                + "JobType:{JobType}, "
                + "StatusCode:{StatusCode}",
                LogHelpers.Arguments(
                    sagaStatus.Status,
                    sagaStatus.CorrelationId,
                    sagaStatus.Id,
                    sagaStatus.ModifiedAt,
                    sagaStatus.JobType,
                    sagaStatus.StatusCode));

            _Logger.ExitMethod();

            return sagaStatus;
        }

        /// <inheritdoc cref="ISagaStatusStore" />
        public async Task SaveSagaStatus(SagaStatus sagaStatus)
        {
            _Logger.EnterMethod();

            if (string.IsNullOrEmpty(sagaStatus.Id))
            {
                _Logger.LogWarnMessage(
                    "The variable with the name {nameof(sagaStatus.Id)} was null or empty. Saga couldn't be saved.",
                    LogHelpers.Arguments(nameof(sagaStatus.Id)));

                throw new ArgumentNullException(nameof(sagaStatus.Id));
            }

            if (string.IsNullOrEmpty(sagaStatus.CorrelationId))
            {
                _Logger.LogWarnMessage(
                    "The variable with the name {nameof(sagaStatus.CorrelationId)} was null or empty. Saga couldn't be saved.",
                    LogHelpers.Arguments(nameof(sagaStatus.CorrelationId)));
            }

            if (string.IsNullOrEmpty(sagaStatus.JobType))
            {
                _Logger.LogErrorMessage(
                    null,
                    "The variable with the name {JobType} was null or empty. Saga couldn't be saved.",
                    LogHelpers.Arguments(nameof(sagaStatus.JobType)));

                throw new ArgumentNullException(nameof(sagaStatus.JobType));
            }

            if (sagaStatus.Status != SagaState.Success
                && sagaStatus.Status != SagaState.Pending
                && sagaStatus.Status != SagaState.Failed)
            {
                _Logger.LogErrorMessage(
                    null,
                    "The variable with the name {Status} was null or empty. Saga couldn't be saved.",
                    LogHelpers
                        .Arguments(nameof(sagaStatus.Status)));

                throw new ArgumentException(nameof(sagaStatus.Status));
            }

            JObject arangoDocument = sagaStatus.InjectDocumentKey(
                o => o.Id,
                _SerializerSettingsProvider.GetNewtonsoftSettings());

            if (sagaStatus.CreatedAt == DateTime.MinValue)
            {
                JProperty existingCreated = arangoDocument.Properties()
                    .FirstOrDefault(
                        p => p.Name.Equals(
                            nameof(SagaStatus.CreatedAt),
                            StringComparison.OrdinalIgnoreCase));

                existingCreated?.Remove();
            }

            CreateDocumentResponse response =
                await _Client.CreateDocumentAsync(
                    GetCollectionName(),
                    arangoDocument,
                    new CreateDocumentOptions
                    {
                        OverWriteMode = AOverwriteMode.Update,
                        Overwrite = true
                    });

            if (response.Error)
            {
                _Logger.LogErrorMessage(
                    null,
                    "The saga status could not be saved in the collection {collectionName}.",
                    LogHelpers.Arguments(GetCollectionName()));

                throw response.Exception
                    ?? new Exception($"The saga status could not be saved in the collection {GetCollectionName()}.");
            }

            _Logger.LogDebugMessage(
                "Saved the sagaStatus with the following parameter:"
                + "{Status}: {Status}, "
                + "{CorrelationId}: {CorrelationId}, "
                + "{sagaStatusId}: {Id}"
                + "{ModifiedAt}: {ModifiedAt}, "
                + "{JobType}:{JobType}, "
                + "{StatusCode}:{StatusCode}",
                LogHelpers.Arguments(
                    nameof(sagaStatus.Status),
                    sagaStatus.Status,
                    nameof(sagaStatus.CorrelationId),
                    sagaStatus.CorrelationId,
                    nameof(sagaStatus.Id),
                    sagaStatus.Id,
                    nameof(sagaStatus.ModifiedAt),
                    sagaStatus.ModifiedAt,
                    nameof(sagaStatus.JobType),
                    sagaStatus.JobType,
                    nameof(sagaStatus.StatusCode),
                    sagaStatus.StatusCode));

            _Logger.ExitMethod();
        }
    }
}
#endif
