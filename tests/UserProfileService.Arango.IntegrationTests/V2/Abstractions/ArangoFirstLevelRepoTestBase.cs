using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.Public;
using Maverick.Client.ArangoDb.Public.Extensions;
using Maverick.Client.ArangoDb.Public.Models.Document;
using Maverick.Client.ArangoDb.Public.Models.Query;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Arango.IntegrationTests.V2.Abstractions
{
    public abstract class ArangoFirstLevelRepoTestBase : ArangoDbTestBase
    {
        protected async Task<IDictionary<string, object>> GetDocumentAsync<TType>(
            string key,
            bool throwExceptions = true)
        {
            IArangoDbClient client = GetArangoClient();
            GetDocumentResponse response = await client.GetDocumentAsync(GetArangoId<TType>(key));

            if (response.Error && throwExceptions)
            {
                throw response.Exception;
            }

            return response.Result?.DocumentData;
        }

        protected async Task<IReadOnlyList<TType>> GetDocumentObjectsAsync<TType>(
            string aqlQuery,
            bool throwExceptions,
            params string[] bindParameterKeyAndValues)
        {
            var bindParameters = new Dictionary<string, object>();

            for (var i = 0; i < bindParameterKeyAndValues.Length; i += 2)
            {
                bindParameters.Add(
                    bindParameterKeyAndValues[i],
                    bindParameterKeyAndValues[i + 1]);
            }

            IArangoDbClient client = GetArangoClient();

            MultiApiResponse<TType> response = await client.ExecuteQueryWithCursorOptionsAsync<TType>(
                new CreateCursorBody
                {
                    Query = aqlQuery,
                    BindVars = bindParameters
                });

            if (response.Error && throwExceptions)
            {
                throw new AggregateException(
                    "Oops. Something went wrong during test.",
                    response.Responses.Select(r => r.Exception));
            }

            return response.QueryResult;
        }

        protected async Task<TType> GetDeserializedObjectAsync<TType>(
            string key,
            params JsonConverter[] usedConverters)
        {
            IArangoDbClient client = new ArangoDbClient(
                "temporary",
                GetArangoConnectionString(),
                clientFactory: GetHttpClientFactory(),
                defaultSerializerSettings: DefaultSerializerSettings);

            client.UsedJsonSerializerSettings.Converters.Clear();

            foreach (JsonConverter converter in usedConverters)
            {
                client.UsedJsonSerializerSettings.Converters.Add(converter);
            }

            GetDocumentResponse<TType> response = await client.GetDocumentAsync<TType>(GetArangoId<TType>(key));

            if (response.Error)
            {
                throw response.Exception ?? new DatabaseException($"Could not retrieve document '{key}' during test", ExceptionSeverity.Fatal);
            }

            return response.Result;
        }

        protected async Task<TType> GetDocumentObjectAsync<TType>(string key, bool throwExceptions = true)
        {
            IArangoDbClient client = GetArangoClient();

            GetDocumentResponse<TType>
                response = await client.GetDocumentAsync<TType>(GetArangoId<TType>(key));

            if (response.Error && throwExceptions)
            {
                throw response.Exception;
            }

            return response.Result;
        }

        protected async Task<IFirstLevelProjectionContainer> GetContainer(
            ObjectIdent container,
            bool throwExceptions = true)
        {
            return container.Type switch
            {
                ObjectType.Group => (IFirstLevelProjectionContainer)await
                    GetDocumentObjectAsync<IFirstLevelProjectionProfile>(container.Id, throwExceptions),
                ObjectType.Organization => (IFirstLevelProjectionContainer)await
                    GetDocumentObjectAsync<IFirstLevelProjectionProfile>(container.Id, throwExceptions),
                ObjectType.Role => await GetDocumentObjectAsync<FirstLevelProjectionRole>(
                    container.Id,
                    throwExceptions),
                ObjectType.Function => await GetFunction(container.Id, throwExceptions),
                _ => throw new NotSupportedException($"{container.Type} type is not a valid container")
            };
        }

        protected async Task<FirstLevelProjectionFunction> GetFunction(string id, bool throwExceptions = true)
        {
            var query = $@"
                WITH @@profilesCollection,  @@rolesCollection, @@functionsCollection
                LET function = DOCUMENT(@@functionsCollection, @functionId)

                LET relations = (
                    FOR v IN 1..1 OUTBOUND function @@functionLinks RETURN v
                )

                RETURN MERGE(
                    function, 
                    {{
                        {
                            nameof(FirstLevelProjectionFunction.Role)
                        }: FIRST(relations[* FILTER IS_SAME_COLLECTION(@@rolesCollection, CURRENT)]),
                        {
                            nameof(FirstLevelProjectionFunction.Organization)
                        }: FIRST(relations[* FILTER IS_SAME_COLLECTION(@@profilesCollection, CURRENT)])
                    }})
                ";

            ModelBuilderOptions modelsInfo =
                DefaultModelConstellation.CreateNewFirstLevelProjection(FirstLevelProjectionPrefix).ModelsInfo;

            MultiApiResponse<FirstLevelProjectionFunction> result = await GetArangoClient()
                .ExecuteQueryWithCursorOptionsAsync<FirstLevelProjectionFunction>(
                    new CreateCursorBody
                    {
                        BindVars = new Dictionary<string, object>

                        {
                            { "@profilesCollection", modelsInfo.GetCollectionName<IFirstLevelProjectionProfile>() },
                            { "@rolesCollection", modelsInfo.GetCollectionName<FirstLevelProjectionRole>() },
                            { "@functionsCollection", modelsInfo.GetCollectionName<FirstLevelProjectionFunction>() },
                            { "functionId", id },
                            {
                                "@functionLinks", modelsInfo
                                    .GetRelation<FirstLevelProjectionFunction, FirstLevelProjectionRole>()
                                    ?.EdgeCollection
                                ?? throw new NotSupportedException(
                                    "The model builder seems to has a missing implementation")
                            }
                        },
                        Query = query
                    });

            return result.QueryResult.FirstOrDefault();
        }

        protected string GetCollectionName<TType>()
        {
            ModelBuilderOptions modelsInfo =
                DefaultModelConstellation.CreateNewFirstLevelProjection(FirstLevelProjectionPrefix).ModelsInfo;

            return modelsInfo.GetCollectionName<TType>();
        }

        protected virtual string GetArangoId<TType>(string key)
        {
            ModelBuilderOptions modelsInfo =
                DefaultModelConstellation.CreateNewFirstLevelProjection(FirstLevelProjectionPrefix).ModelsInfo;

            string collection = modelsInfo.GetCollectionName<TType>();

            return GetArangoId(collection, key);
        }

        protected string GetArangoId(string collection, string key)
        {
            return $"{collection}/{key}";
        }

        protected T[] GetArray<T>(IDictionary<string, object> data, string fieldName)
        {
            return (data[fieldName] as JArray)?.Values<T>().ToArray()
                ?? throw new ArgumentException("Unable to parse " + fieldName);
        }

        protected async Task<long> GetRelationCountAsync<TEntity>(string edge, string id)
        {
            MultiApiResponse<long> result = await GetArangoClient()
                .ExecuteQueryAsync<long>(
                    "RETURN LENGTH("
                    + $"FOR edge IN {edge} "
                    + $"FILTER edge._from == \"{GetArangoId<TEntity>(id)}\" "
                    + $"OR edge._to == \"{GetArangoId<TEntity>(id)}\" "
                    + "RETURN edge)");

            return result.QueryResult.First();
        }

        protected async Task<long> GetResultCountAsync(string query)
        {
            MultiApiResponse<long> result = await GetArangoClient().ExecuteQueryAsync<long>($"RETURN LENGTH({query})");

            return result.QueryResult.First();
        }

        protected string GetEdgeCollection<TFrom, TTo>()
        {
            return DefaultModelConstellation.CreateNewFirstLevelProjection(FirstLevelProjectionPrefix)
                .ModelsInfo.GetRelation<TFrom, TTo>()
                ?.EdgeCollection;
        }

        protected static async Task<Exception> GetThrownExceptionAsync(Func<Task> task)
        {
            try
            {
                await task.Invoke();
            }
            catch (Exception e)
            {
                return e;
            }

            return null;
        }
    }
}
