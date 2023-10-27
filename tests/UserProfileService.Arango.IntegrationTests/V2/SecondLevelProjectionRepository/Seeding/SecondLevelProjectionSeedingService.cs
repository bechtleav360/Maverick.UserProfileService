using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.Public;
using Maverick.Client.ArangoDb.Public.Extensions;
using Maverick.Client.ArangoDb.Public.Models.Document;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Arango.IntegrationTests.V2.Extensions;
using UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Abstractions;
using UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Models;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding
{
    internal class SecondLevelProjectionSeedingService
    {
        private readonly IArangoDbClient _client;

        internal SecondLevelProjectionSeedingService(IArangoDbClient client)
        {
            _client = client;
        }

        private static JsonSerializer GetJsonSerializer()
        {
            return JsonSerializer.CreateDefault(
                new JsonSerializerSettings
                {
                    Culture = CultureInfo.InvariantCulture,
                    Converters = new List<JsonConverter>
                    {
                        new StringEnumConverter()
                    },
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                    Formatting = Formatting.None,
                    ContractResolver = new DefaultContractResolver()
                });
        }

        private Task ExecuteConcurrentlyAsync<TInput>(
            IEnumerable<TInput> input,
            Func<TInput, Task> method)
        {
            if (input == null)
            {
                return Task.CompletedTask;
            }

            List<Task> tasks =
                input
                    .Select(item => Task.Run(() => method.Invoke(item)))
                    .ToList();

            return Task.WhenAll(tasks);
        }

        public async Task ExecuteSeedAsync(Func<IArangoDbClient, Task> seedingMethod)
        {
            await seedingMethod.Invoke(_client);
        }

        public async Task SeedDataAsync(
            ITestData testData,
            ModelBuilderOptions modelBuilderOptions)
        {
            await ExecuteConcurrentlyAsync(
                testData.Users,
                e => CreateEntityAsync(e, u => u.Id, modelBuilderOptions));

            await ExecuteConcurrentlyAsync(
                testData.Functions,
                e => CreateEntityAsync(e, f => f.Id, modelBuilderOptions));

            await ExecuteConcurrentlyAsync(
                testData.Groups,
                e => CreateEntityAsync(e, g => g.Id, modelBuilderOptions));

            await ExecuteConcurrentlyAsync(
                testData.Organizations,
                e => CreateEntityAsync(e, o => o.Id, modelBuilderOptions));

            await ExecuteConcurrentlyAsync(
                testData.Tags,
                e => CreateEntityAsync(e, t => t.Id, modelBuilderOptions));

            await ExecuteConcurrentlyAsync(
                testData.Roles,
                e => CreateEntityAsync(e, r => r.Id, modelBuilderOptions));

            await CreateObjectTreeDataAsync(
                testData.VertexData,
                testData.EdgeData,
                modelBuilderOptions);
        }

        public async Task CreateObjectTreeDataAsync(
            IList<ExtendedProfileVertexData> vertices,
            IList<ExtendedProfileEdgeData> edges,
            ModelBuilderOptions modelBuilderOptions)
        {
            string vertexCollectionName =
                modelBuilderOptions.GetCollectionName<SecondLevelProjectionProfileVertexData>();

            CreateDocumentsResponse createVerticesResponse = await _client.CreateDocumentsAsync(
                vertexCollectionName,
                vertices);

            if (createVerticesResponse.Error)
            {
                throw new Exception(
                    "Ooops... something went wrong during test preparation of second level proj. repo.");
            }

            string edgeCollectionName = modelBuilderOptions
                .GetRelatedOutboundEdgeCollections<SecondLevelProjectionProfileVertexData>()
                .Single();

            foreach (ExtendedProfileEdgeData edge in edges)
            {
                edge.AddCollectionNames(vertexCollectionName);
            }

            CreateDocumentsResponse createEdgesResponse = await _client.CreateDocumentsAsync(
                edgeCollectionName,
                edges);

            if (createEdgesResponse.Error)
            {
                throw new Exception(
                    "Ooops... something went wrong during test preparation of second level proj. repo.");
            }
        }

        public async Task CreateEntityAsync<TEntity>(
            TEntity entity,
            Func<TEntity, string> keySelector,
            ModelBuilderOptions modelBuilderOptions)
        {
            string collectionName = modelBuilderOptions.GetCollectionName<TEntity>();

            CreateDocumentResponse createResponse = await _client.CreateDocumentAsync(
                collectionName,
                entity.GetJsonObjectWithInjectedKey(GetJsonSerializer(), keySelector));

            if (createResponse.Error)
            {
                throw new Exception(
                    "Ooops... something went wrong during test preparation of second level proj. repo.");
            }
        }

        public async Task CreateCollectionsAsync(ModelBuilderOptions modelBuilderOptions)
        {
            foreach (string collection in modelBuilderOptions.GetDocumentCollections()
                         .Concat(modelBuilderOptions.GetQueryDocumentCollections()))
            {
                await _client.CreateCollectionAsync(collection);
            }

            foreach (string collection in modelBuilderOptions.GetEdgeCollections())
            {
                await _client.CreateCollectionAsync(
                    collection,
                    ACollectionType.Edge);
            }
        }

        public async Task CleanupDatabaseAsync(ModelBuilderOptions modelBuilderOptions)
        {
            List<string> collections = modelBuilderOptions.GetDocumentCollections()
                .Concat(modelBuilderOptions.GetEdgeCollections())
                .Concat(modelBuilderOptions.GetQueryDocumentCollections())
                .ToList();

            foreach (string collection in collections)
            {
                await _client.DeleteCollectionAsync(collection);
            }
        }
    }
}
