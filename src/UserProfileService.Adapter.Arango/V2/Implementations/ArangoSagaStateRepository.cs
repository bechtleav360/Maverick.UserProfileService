#if NETCOREAPP3_1
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arango.Client.Async.DotnetCore.Public;
using Arango.Client.Async.DotnetCore.Public.Extensions;
using Arango.Client.Async.DotnetCore.Public.Models.Document;
using Arango.Client.Async.DotnetCore.Public.Models.Query;
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
    ///     Custom implementation of ISagaStateRepository.
    /// </summary>
    public class ArangoSagaStateRepository : ISagaStateRepository
    {
        private readonly IArangoDbClient _Client;
        private readonly ICollectionDetailsProvider _CollectionDetailsProvider;
        private readonly SagaContext _Context;
        private readonly ILogger<ArangoSagaStateRepository> _Logger;
        private readonly IJsonSerializerSettingsProvider _SerializerSettingsProvider;

        /// <summary>
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <param name="arangoClientFactory"></param>
        /// <param name="context"></param>
        /// <param name="serializerSettingsProvider"></param>
        /// <param name="collectionDetailsProviders"></param>
        public ArangoSagaStateRepository(
            ILoggerFactory loggerFactory,
            IArangoDbClientFactory arangoClientFactory,
            SagaContext context,
            IJsonSerializerSettingsProvider serializerSettingsProvider,
            IEnumerable<ICollectionDetailsProvider> collectionDetailsProviders)
        {
            _Logger = loggerFactory.CreateLogger<ArangoSagaStateRepository>();
            _Client = arangoClientFactory.Create(ArangoConstants.SagaArangoClientName);
            _Context = context;
            _SerializerSettingsProvider = serializerSettingsProvider;

            _CollectionDetailsProvider =
                collectionDetailsProviders?.FirstOrDefault(p => p is SagaCollectionsProvider)
                ?? throw new Exception(
                    $"{nameof(SagaCollectionsProvider)} is missing in {nameof(IServiceCollection)}.");
        }

        private async Task<SagaContext.ContextSagaState> FindInternal(string id, string type)
        {
            var aqlRequestGetLogs =
                $"FOR LOG IN {GetCollectionName()} FILTER LOG.Id == \"{id}\" AND LOG.TypeName == \"{type}\" RETURN LOG";

            MultiApiResponse<SagaContext.ContextSagaState> response =
                await _Client.ExecuteQueryAsync<SagaContext.ContextSagaState>(aqlRequestGetLogs);

            if (response.Responses.Any(x => x.Error))
            {
                _Logger.LogErrorMessage(
                    null,
                    "Could not find the element with the id = {id} in method {methodName} in class {ArangoSagaStateRepository}.",
                    LogHelpers.Arguments(id, nameof(WriteAsync), nameof(ArangoSagaStateRepository)));

                throw response.Responses.FirstOrDefault()?.Exception
                    ?? new Exception(
                        $"Could not find the element with the id = {id} in method {nameof(WriteAsync)} in class {nameof(ArangoSagaStateRepository)}.");
            }

            return response.QueryResult.FirstOrDefault();
        }

        private string GetCollectionName()
        {
            string collectionName =
                _CollectionDetailsProvider
                    .GetCollectionDetails()
                    .FirstOrDefault(
                        cd => cd.CollectionName
                            .Contains(ArangoConstants.SagaStateRepositoryCollectionName))
                    ?.CollectionName;

            if (string.IsNullOrEmpty(collectionName))
            {
                throw new ConfigurationException(
                    $"Configuration not valid: missing collection name for the {nameof(ArangoSagaStateRepository)}");
            }

            return collectionName;
        }

        /// <inheritdoc />
        public async Task<ISagaState> ReadAsync(SagaId id, Type type)
        {
            _Logger.EnterMethod();

            try
            {
                SagaContext.ContextSagaState found = await FindInternal(id.ToString(), type.AssemblyQualifiedName);

                if (found is null)
                {
                    _Logger.LogDebugMessage(
                        "No saga state entry could be found for saga id {sagaId} (ArangoDB collection {collectionName}).",
                        LogHelpers.Arguments(id.Id, GetCollectionName()));

                    return _Logger.ExitMethod((ISagaState)null);
                }

                ISagaState transformed = _Context.TransformFromContext(found);

                _Logger.LogDebugMessage(
                    "A value could be found in the collection {collectionName}. The log has the values: id = {transformedId}, Data = {transformedDate}, State = {transformedState}.",
                    LogHelpers.Arguments(GetCollectionName(), transformed.Id, transformed.Data, transformed.State));

                return _Logger.ExitMethod(transformed);
            }
            catch (Exception e)
            {
                _Logger.LogWarnMessage(
                    e,
                    "Could not read SagaState for saga {id} and type {assemblyQualifiedName}.",
                    LogHelpers.Arguments(id, type.AssemblyQualifiedName));

                return _Logger.ExitMethod((ISagaState)null);
            }
        }

        /// <inheritdoc />
        public async Task WriteAsync(ISagaState state)
        {
            _Logger.EnterMethod();

            SagaContext.ContextSagaState found =
                await FindInternal(state.Id.ToString(), state.Type.AssemblyQualifiedName);

            SagaContext.ContextSagaState container = _Context.TransformForContext(state);

            if (found is null)
            {
                _Logger.LogDebugMessage(
                    "A new log value will be written in the {collectionName}: Id = {containerId}, Data = {serializedData}, State = {containerState}.",
                    LogHelpers.Arguments(GetCollectionName(), container.Id, container.SerializedData, container.State));

                JObject arangoContainer = container.InjectDocumentKey(
                    o => o.Id,
                    _SerializerSettingsProvider.GetNewtonsoftSettings());

                CreateDocumentResponse response = await _Client.CreateDocumentAsync(
                    GetCollectionName(),
                    arangoContainer);

                if (response.Error || response.Result == null)
                {
                    _Logger.ExitMethod(
                        $"Could not create document with the id = {container.Id} in class {nameof(ArangoSagaStateRepository)}.");

                    throw response.Exception
                        ?? new Exception(
                            $"Could not create document with the id = {container.Id} in method {nameof(WriteAsync)} in class {nameof(ArangoSagaStateRepository)}.");
                }
            }
            else
            {
                found.TypeName = container.TypeName;
                found.DataTypeName = container.DataTypeName;
                found.State = container.State;
                found.SerializedData = container.SerializedData;

                _Logger.LogDebugMessage(
                    "A value will be updated in the collection {collectionName}: TypeName={typeName}, DataTypeName = {dataTypeName}, State = {state}, SerializedData = {serializedData}.",
                    LogHelpers.Arguments(
                        GetCollectionName(),
                        found.TypeName,
                        found.DataTypeName,
                        found.State,
                        found.SerializedData));

                var aqlRequestUpdate =
                    $"LET content=@document FOR log in {GetCollectionName()} FILTER log.Id == \"{found.Id}\" UPDATE MERGE(content, {{ _key: log._key }}) in {GetCollectionName()} RETURN log";

                MultiApiResponse<SagaContext.ContextSagaState> response =
                    await _Client.ExecuteQueryWithCursorOptionsAsync<SagaContext.ContextSagaState>(
                        new CreateCursorBody
                        {
                            Query = aqlRequestUpdate,
                            BindVars = new Dictionary<string, object>
                            {
                                // Important to change JSON settings, otherwise duplicate entries will be set (camel case vs. pascal case)
                                {
                                    "document", JObject.FromObject(
                                        found,
                                        JsonSerializer.CreateDefault(
                                            _SerializerSettingsProvider.GetNewtonsoftSettings()))
                                }
                            }
                        });

                if (response.Responses.Any(x => x.Error))
                {
                    _Logger.LogErrorMessage(
                        null,
                        "Could not update the item with the id = {Id} in class {ArangoSagaStateRepository}.",
                        LogHelpers.Arguments(found.Id, nameof(ArangoSagaStateRepository)));

                    throw response.Responses?.FirstOrDefault()?.Exception
                        ?? new Exception(
                            $"Could not update the item with the id = {found.Id} in method {nameof(WriteAsync)} in class {nameof(ArangoSagaStateRepository)}.");
                }
            }

            _Logger.ExitMethod();
        }
    }
}
#endif
