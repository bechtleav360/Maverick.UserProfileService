using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.Public;
using Maverick.Client.ArangoDb.Public.Extensions;
using Maverick.Client.ArangoDb.Public.Models.Document;
using Newtonsoft.Json.Linq;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Arango.IntegrationTests.V2.Abstractions
{
    public abstract class ArangoSecondLevelRepoTestBase : ArangoDbTestBase
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

        /// <summary>
        ///     Returns an entity of type <typeparamref name="TResult" /> as part of ArangoDb collection with
        ///     <typeparamref name="TEntityCollection" /> instances.
        /// </summary>
        /// <typeparam name="TEntityCollection">The entity type used in model builder to determine the correct collection.</typeparam>
        /// <typeparam name="TResult">The type the resulting entity should be deserialized to.</typeparam>
        protected async Task<TResult> GetDocumentObjectAsync<TResult, TEntityCollection>(
            string key,
            bool throwExceptions = true)
        {
            IArangoDbClient client = GetArangoClient();

            GetDocumentResponse<TResult>
                response = await client.GetDocumentAsync<TResult>(GetArangoId<TEntityCollection>(key));

            if (response.Error && throwExceptions)
            {
                throw response.Exception;
            }

            return response.Result;
        }

        protected string GetCollectionName<TType>()
        {
            ModelBuilderOptions modelsInfo =
                DefaultModelConstellation.CreateNewSecondLevelProjection(SecondLevelProjectionPrefix).ModelsInfo;

            string collection = modelsInfo.GetCollectionName<TType>();

            if (string.IsNullOrWhiteSpace(collection))
            {
                throw new Exception(
                    $"Error in {GetType().Name} class! Could not determine collection name of type {typeof(TType).Name}");
            }

            return collection;
        }

        protected string GetArangoId<TType>(string key)
        {
            string collection = GetCollectionName<TType>();

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
            return DefaultModelConstellation.CreateNewSecondLevelProjection(SecondLevelProjectionPrefix)
                .ModelsInfo.GetRelation<TFrom, TTo>()
                ?.EdgeCollection;
        }
        
        protected async Task<SecondLevelProjectionProfileEdgeData> GetEdgeFromPathTreeAsync(string relatedId, string objectId)
        {

            var edgeCollection =
                GetEdgeCollection<SecondLevelProjectionProfileVertexData, SecondLevelProjectionProfileVertexData>();

            var pathTreeVertexCollection = GetCollectionName<SecondLevelProjectionProfileVertexData>();

            var fromEdge = $"{pathTreeVertexCollection}/{relatedId}-{relatedId}";
            var toEdge = $"{pathTreeVertexCollection}/{relatedId}-{objectId}";
            
            string aql = $@" FOR edge in {edgeCollection}
                            FILTER edge._to == ""{toEdge}"" and edge._from == ""{fromEdge}""
                            RETURN edge";

            var response = await GetArangoClient().ExecuteQueryAsync<SecondLevelProjectionProfileEdgeData>(aql);

            if (response.QueryResult.Count == 0)
            {
                return null;
            }

            return response.QueryResult.First();
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
