using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.Public;
using Maverick.Client.ArangoDb.Public.Extensions;
using Maverick.Client.ArangoDb.Public.Models.Collection;
using Microsoft.Extensions.DependencyInjection;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Arango.IntegrationTests.V2.Abstractions;
using UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Seeding.Attributes;

namespace UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Seeding
{
    internal class FirstLevelProjTestsPreparationService : ArangoDbTestBase
    {
        private static FirstLevelProjTestsPreparationService _instance;

        private readonly SemaphoreSlim _sync = new SemaphoreSlim(1, 1);
        private bool _prepared;

        internal ModelBuilderOptions DefaultModelBuilderOptionsWrite { get; }
            = DefaultModelConstellation.CreateNewFirstLevelProjection(FirstLevelProjectionPrefix).ModelsInfo;

        internal ModelBuilderOptions DefaultModelBuilderOptionsRead { get; }
            = DefaultModelConstellation.CreateNewFirstLevelProjection(FirstLevelProjectionReadPrefix).ModelsInfo;

        public static FirstLevelProjTestsPreparationService Instance =>
            _instance ??= new FirstLevelProjTestsPreparationService();

        private FirstLevelProjTestsPreparationService()
        {
        }

        private IArangoDbClient GetClient()
        {
            var aFactory = GetServiceProvider().GetRequiredService<IArangoDbClientFactory>();

            return aFactory.Create(ArangoDbClientName);
        }

        private async Task CleanupDatabaseAsync()
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

        private async Task CreateWriteCollectionsAsync()
        {
            IArangoDbClient client = GetClient();

            foreach (string collectionName in DefaultModelBuilderOptionsWrite.GetDocumentCollections()
                         .Concat(
                             DefaultModelBuilderOptionsWrite
                                 .GetQueryDocumentCollections())
                         .Concat(
                             DefaultModelBuilderOptionsRead
                                 .GetDocumentCollections())
                         .Concat(
                             DefaultModelBuilderOptionsRead
                                 .GetQueryDocumentCollections()))
            {
                await client.CreateCollectionAsync(collectionName);
            }

            foreach (string collectionName in DefaultModelBuilderOptionsWrite.GetEdgeCollections()
                         .Concat(
                             DefaultModelBuilderOptionsRead
                                 .GetEdgeCollections()))
            {
                await client.CreateCollectionAsync(collectionName, ACollectionType.Edge);
            }
        }

        private async Task SeedTestData()
        {
            var seedingService = new FirstLevelProjectionSeedingService(
                GetClient(),
                DefaultModelBuilderOptionsWrite,
                DefaultModelBuilderOptionsRead);

            List<ITestData> testData = GetType()
                .Assembly
                .GetTypes()
                .Where(
                    t => typeof(ITestData).IsAssignableFrom(t)
                        && !t.IsAbstract
                        && t.IsClass
                        && t.GetConstructor(Array.Empty<Type>()) != null)
                .Select(t => t.GetConstructor(Array.Empty<Type>())?.Invoke(Array.Empty<object>()))
                .Where(t => t != null)
                .Cast<ITestData>()
                .ToList();

            List<ReflectionTestData> attributeTestData = GetType()
                .Assembly
                .GetTypes()
                .Where(t => t.GetCustomAttribute<SeedDataAttribute>() != null)
                .Select(t => new ReflectionTestData(t))
                .ToList();

            testData.AddRange(attributeTestData);

            foreach (ITestData data in testData)
            {
                await seedingService.SeedData(data);
            }

            List<ObjectTestData> moreTestData = GetType()
                .Assembly
                .GetTypes()
                .Where(t => t.GetCustomAttribute<SeedGeneralDataAttribute>() != null)
                .Select(t => new ObjectTestData(t))
                .ToList();

            foreach (ObjectTestData data in moreTestData)
            {
                await seedingService.SeedData(data);
            }
        }

        public async Task PrepareDatabaseAsync()
        {
            if (_prepared)
            {
                return;
            }

            await _sync.WaitAsync();

            try
            {
                if (!_prepared)
                {
                    await CleanupDatabaseAsync();
                    await CreateWriteCollectionsAsync();
                    await SeedTestData();
                    _prepared = true;
                }
            }
            finally
            {
                _sync.Release();
            }
        }
    }
}
