#if NETCOREAPP3_1
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arango.Client.Async.DotnetCore.Public;
using Arango.Client.Async.DotnetCore.Public.Extensions;
using Arango.Client.Async.DotnetCore.Public.Models.Document;
using Chronicle;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UserProfileService.Adapter.Arango.V2.Abstractions;
using UserProfileService.Adapter.Arango.V2.Contracts;
using UserProfileService.Common.Logging;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Exceptions;
using SagaContext = UserProfileService.Adapter.Arango.V2.EntityModels.Saga.SagaContext;

namespace UserProfileService.Adapter.Arango.V2.Implementations
{
    /// <summary>
    ///     Custom implementation of <see cref="ISagaLog" /> using <see cref="EntityModels.Saga.SagaContext" />.
    /// </summary>
    public class ArangoSagaLog : ISagaLog
    {
        private readonly IArangoDbClient _Client;
        private readonly ICollectionDetailsProvider _CollectionDetailsProvider;
        private readonly SagaContext _Context;
        private readonly ILogger<ArangoSagaLog> _Logger;
        private readonly IJsonSerializerSettingsProvider _SerializerSettingsProvider;

        /// <summary>
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <param name="context"></param>
        /// <param name="arangoClientFactory"></param>
        /// <param name="serializerSettingsProvider"></param>
        /// <param name="collectionDetailsProviders"></param>
        public ArangoSagaLog(
            ILoggerFactory loggerFactory,
            SagaContext context,
            IArangoDbClientFactory arangoClientFactory,
            IJsonSerializerSettingsProvider serializerSettingsProvider,
            IEnumerable<ICollectionDetailsProvider> collectionDetailsProviders)
        {
            _Client = arangoClientFactory.Create(ArangoConstants.SagaArangoClientName);
            _Logger = loggerFactory.CreateLogger<ArangoSagaLog>();
            _Context = context;
            _SerializerSettingsProvider = serializerSettingsProvider;

            _CollectionDetailsProvider =
                collectionDetailsProviders?.FirstOrDefault(p => p is SagaCollectionsProvider)
                ?? throw new Exception(
                    $"{nameof(SagaCollectionsProvider)} is missing in {nameof(IServiceCollection)}.");
        }

        private async Task<List<SagaContext.ContextSagaLogData>> FindInternal(string id, string type)
        {
            var aqlRequest =
                $"FOR LOG IN {GetCollectionName()} FILTER LOG.Id == \"{id}\" AND LOG.TypeName == \"{type}\" RETURN LOG";

            MultiApiResponse<SagaContext.ContextSagaLogData> response =
                await _Client.ExecuteQueryAsync<SagaContext.ContextSagaLogData>(aqlRequest);

            if (response.Responses.Any(x => x.Error))
            {
                _Logger.LogErrorMessage(
                    null,
                    "Could not find the element with the id = {id} in method {methodName} in class {className}.",
                    LogHelpers.Arguments(id, nameof(WriteAsync), nameof(ArangoSagaLog)));

                throw response.Responses.FirstOrDefault()?.Exception
                    ?? new Exception(
                        $"Could not find the element with the id = {id} in method {nameof(WriteAsync)} in class {nameof(ArangoSagaLog)}.");
            }

            return response.QueryResult?.Select(x => x).ToList();
        }

        private string GetCollectionName()
        {
            string collectionName =
                _CollectionDetailsProvider
                    .GetCollectionDetails()
                    .FirstOrDefault(
                        cd => cd.CollectionName
                            .Contains(ArangoConstants.SagaLogCollectionName))
                    ?.CollectionName;

            if (string.IsNullOrEmpty(collectionName))
            {
                throw new ConfigurationException(
                    $"Configuration not valid: missing collection name for the {nameof(ArangoSagaLog)}");
            }

            return collectionName;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<ISagaLogData>> ReadAsync(SagaId id, Type type)
        {
            _Logger.EnterMethod();

            try
            {
                List<SagaContext.ContextSagaLogData> found =
                    await FindInternal(id.ToString(), type.AssemblyQualifiedName);

                if (found is null || !found.Any())
                {
                    _Logger.LogDebugMessage(
                        "No items where found in the collection {collectionName}.",
                        LogHelpers.Arguments(GetCollectionName()));

                    return Array.Empty<ISagaLogData>();
                }

                List<ISagaLogData> transformed = found.Select(f => _Context.TransformFromContext(f)).ToList();

                _Logger.LogDebugMessage(
                    "Values were found in the collection {collectionName} with the ids: {ids}.",
                    LogHelpers.Arguments(GetCollectionName(), string.Join(";", transformed.Select(x => x.Id))));

                return transformed;
            }
            catch (Exception e)
            {
                _Logger.LogWarnMessage(
                    e,
                    "Could not read ArangoSagaLog for saga {id} and type {assemblyQualifiedName}.",
                    LogHelpers.Arguments(id, type.AssemblyQualifiedName));

                return Array.Empty<ISagaLogData>();
            }
        }

        /// <inheritdoc />
        public async Task WriteAsync(ISagaLogData message)
        {
            _Logger.EnterMethod();

            SagaContext.ContextSagaLogData container = _Context.TransformForContext(message);

            _Logger.LogDebugMessage(
                "The log item id = {containerId}, typename = {containerTypeName}, serializedMessage = {containerSerializedMessage} will be written in the collection.",
                LogHelpers.Arguments(container.Id, container.TypeName, container.SerializedMessage));

            CreateDocumentResponse createResponse =
                await _Client.CreateDocumentAsync(
                    GetCollectionName(),
                    JObject.FromObject(
                        container,
                        JsonSerializer.CreateDefault(_SerializerSettingsProvider.GetNewtonsoftSettings())));

            if (createResponse.Error || createResponse.Result == null)
            {
                _Logger.LogErrorMessage(
                    null,
                    "Could not update the element with the id = {containerId} in class {ArangoSagaLog}.",
                    LogHelpers.Arguments(container.Id, nameof(ArangoSagaLog)));

                throw createResponse.Exception
                    ?? new Exception(
                        $"Could not update the element with the id = {container.Id} in method {nameof(WriteAsync)} in class {nameof(ArangoSagaLog)}.");
            }

            _Logger.ExitMethod();
        }
    }
}
#endif
