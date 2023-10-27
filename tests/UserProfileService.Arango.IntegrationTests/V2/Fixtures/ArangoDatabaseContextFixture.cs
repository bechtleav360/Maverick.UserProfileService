using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.Public.Extensions;
using Maverick.Client.ArangoDb.Public.Models.Collection;
using Microsoft.Extensions.DependencyInjection;
using UserProfileService.Arango.IntegrationTests.V2.Abstractions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Sync.States;

namespace UserProfileService.Arango.IntegrationTests.V2.Fixtures
{
    public class ArangoDatabaseContextFixture : ArangoDbTestBase, IDisposable
    {
        private readonly Lazy<Task> _preparationTask;

        public readonly string CollectionName = "Test_Sync_Process_StateMachine";


        public ArangoDatabaseContextFixture(
            ICollection<ProcessState> syncProcessStateTestData,
            bool includeSeeding = true)
        {
            _preparationTask = new Lazy<Task>(
                () => Task.Run(() => PrepareDatabaseAsync(syncProcessStateTestData, includeSeeding)));
        }

        private async Task SeedTestData(ICollection<ProcessState> syncProcessData)
        {
            if (syncProcessData == null)
            {
                throw new ArgumentNullException(nameof(syncProcessData));
            }

            if (!syncProcessData.Any())
            {
                return;
            }

            IArangoDbClient arangoClient = GetClient();
            await arangoClient.CreateDocumentsAsync(CollectionName, syncProcessData.ToList());

        }

        private IArangoDbClient GetClient()
        {
            return GetArangoClient();
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
            ICollection<ProcessState> eventBatchTestData,
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
            await client.CreateCollectionAsync(CollectionName);
        }

        public async Task<IFirstProjectionEventLogWriter> GetFirstProjectionEventLogWriter()
        {
            await PrepareAsync();

            return GetServiceProvider().GetService<IFirstProjectionEventLogWriter>();
        }
    }
}
