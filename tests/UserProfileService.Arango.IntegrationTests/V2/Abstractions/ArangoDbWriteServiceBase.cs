using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.Public;
using Maverick.Client.ArangoDb.Public.Extensions;
using Maverick.Client.ArangoDb.Public.Models.Document;
using Newtonsoft.Json.Linq;
using UserProfileService.Adapter.Arango.V2.EntityModels;

namespace UserProfileService.Arango.IntegrationTests.V2.Abstractions
{
    public abstract class ArangoDbWriteServiceTestBase : ArangoDbTestBase
    {
        protected async Task<IDictionary<string, object>> GetDocumentAsync<TType>(
            string key,
            bool isQuery,
            bool throwExceptions = true)
        {
            IArangoDbClient client = GetArangoClient();
            GetDocumentResponse response = await client.GetDocumentAsync(GetArangoId<TType>(key, isQuery));

            if (response.Error && throwExceptions)
            {
                throw response.Exception;
            }

            return response.Result?.DocumentData;
        }

        protected async Task<TType> GetDocumentObjectAsync<TType>(string key, bool isQuery, bool throwExceptions = true)
        {
            IArangoDbClient client = GetArangoClient();

            GetDocumentResponse<TType>
                response = await client.GetDocumentAsync<TType>(GetArangoId<TType>(key, isQuery));

            if (response.Error && throwExceptions)
            {
                throw response.Exception;
            }

            return response.Result;
        }

        protected async Task<string> GetRevisionAsync<TType>(string key, bool isQuery)
        {
            return (await GetDocumentAsync<TType>(key, isQuery))["Revision"].ToString();
        }

        protected string GetCollectionName<TType>(bool isQuery)
        {
            ModelBuilderOptions modelsInfo =
                DefaultModelConstellation.CreateNew(WriteTestPrefix, WriteTestQueryPrefix).ModelsInfo;

            return isQuery
                ? modelsInfo.GetQueryCollectionName<TType>()
                : modelsInfo.GetCollectionName<TType>();
        }

        protected string GetArangoId<TType>(string key, bool isQuery)
        {
            ModelBuilderOptions modelsInfo =
                DefaultModelConstellation.CreateNew(WriteTestPrefix, WriteTestQueryPrefix).ModelsInfo;

            string collection =
                isQuery ? modelsInfo.GetQueryCollectionName<TType>() : modelsInfo.GetCollectionName<TType>();

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

        protected async Task<long> GetRelationCountAsync<TEntity>(string edge, string id, bool isQuery = false)
        {
            MultiApiResponse<long> result = await GetArangoClient()
                .ExecuteQueryAsync<long>(
                    "RETURN LENGTH("
                    + $"FOR edge IN {edge} "
                    + $"FILTER edge._from == \"{GetArangoId<TEntity>(id, isQuery)}\" "
                    + $"OR edge._to == \"{GetArangoId<TEntity>(id, isQuery)}\" "
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
            return DefaultModelConstellation.CreateNew(WriteTestPrefix)
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
