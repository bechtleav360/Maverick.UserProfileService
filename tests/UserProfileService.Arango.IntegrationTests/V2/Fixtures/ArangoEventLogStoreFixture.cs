using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.Public;
using Maverick.Client.ArangoDb.Public.Extensions;
using Maverick.Client.ArangoDb.Public.Models.Collection;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Arango.IntegrationTests.V2.Abstractions;
using UserProfileService.Arango.IntegrationTests.V2.ArangoEventLogStore.Models;
using UserProfileService.Arango.IntegrationTests.V2.ArangoEventLogStore.Seeding;
using UserProfileService.Common.V2.Abstractions;

namespace UserProfileService.Arango.IntegrationTests.V2.Fixtures
{
    public class ArangoEventLogStoreFixture : ArangoDbTestBase, IDisposable
    {
        private static readonly JsonSerializer
            _jsonSerializer = JsonSerializer.CreateDefault(DefaultSerializerSettings);

        private readonly Lazy<Task> _preparationTask;

        private ModelBuilderOptions DefaultModelBuilderOptionsWrite { get; }
            = DefaultModelConstellation.NewEventLogStore(FirstLevelProjectionPrefix).ModelsInfo;

        public ArangoEventLogStoreFixture(
            ICollection<EventBatchTestData> eventBatchTestData,
            bool includeSeeding = true)
        {
            _preparationTask = new Lazy<Task>(
                () => Task.Run(() => PrepareDatabaseAsync(eventBatchTestData, includeSeeding)));
        }

        private async Task SeedTestData(ICollection<EventBatchTestData> eventBatchTestData)
        {
            var seedingService = new ArangoEventLogStoreSeedingService(GetClient(), DefaultModelBuilderOptionsWrite);

            await seedingService.SeedData(eventBatchTestData);
        }

        private IArangoDbClient GetClient()
        {
            return GetArangoClient(ArangoDbClientEventLogStoreName);
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }

        public Task PrepareAsync()
        {
            return _preparationTask.Value;
        }

        public string GetArangoDbClientName()
        {
            return ArangoDbClientName;
        }

        public async Task<IArangoDbClient> GetClientAsync()
        {
            await PrepareAsync();

            return GetClient();
        }

        public async Task PrepareDatabaseAsync(
            ICollection<EventBatchTestData> eventBatchTestData,
            bool includeSeeding = true)
        {
            await CleanupDatabaseAsync();
            await CreateWriteCollectionsAsync();

            if (includeSeeding)
            {
                await SeedTestData(eventBatchTestData);
            }
        }

        public async Task CleanupDatabaseAsync()
        {
            IArangoDbClient client = GetClient();
            GetAllCollectionsResponse collectionInfo = await client.GetAllCollectionsAsync(true);

            if (collectionInfo.Error)
            {
                throw new Exception("Error occurred during test clean up.", collectionInfo.Exception);
            }

            foreach (CollectionEntity collectionEntity in collectionInfo.Result)
            {
                if (collectionEntity.IsSystem)
                {
                    continue;
                }

                await client.DeleteCollectionAsync(collectionEntity.Name);
            }
        }

        public async Task CreateWriteCollectionsAsync()
        {
            IArangoDbClient client = GetClient();

            foreach (string collectionName in DefaultModelBuilderOptionsWrite.GetDocumentCollections()
                         .Concat(
                             DefaultModelBuilderOptionsWrite
                                 .GetQueryDocumentCollections()))
            {
                await client.CreateCollectionAsync(collectionName);
            }

            foreach (string collectionName in DefaultModelBuilderOptionsWrite.GetEdgeCollections())
            {
                await client.CreateCollectionAsync(collectionName, ACollectionType.Edge);
            }
        }

        public async Task<IFirstProjectionEventLogWriter> GetFirstProjectionEventLogWriter()
        {
            await PrepareAsync();

            return GetServiceProvider().GetService<IFirstProjectionEventLogWriter>();
        }
    }
}
