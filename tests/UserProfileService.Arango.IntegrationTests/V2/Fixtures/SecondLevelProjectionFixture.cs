using System;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using Maverick.Client.ArangoDb.Public.Extensions;
using Microsoft.Extensions.DependencyInjection;
using UserProfileService.Adapter.Arango.V2.Abstractions;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Arango.IntegrationTests.V2.Abstractions;
using UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository;
using UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding;
using UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Abstractions;
using UserProfileService.Common.Tests.Utilities;
using UserProfileService.Projection.Abstractions;
using Xunit;

namespace UserProfileService.Arango.IntegrationTests.V2.Fixtures
{
    public class SecondLevelProjectionFixture : ArangoDbTestBase, IDisposable, IAsyncLifetime
    {
        private ModelBuilderOptions DefaultModelBuilderOptionsWrite { get; }
            = DefaultModelConstellation.CreateNewSecondLevelProjection(SecondLevelProjectionPrefix).ModelsInfo;

        public IMapper Mapper { get; }

        public SecondLevelProjectionFixture()
        {
            Assembly[] mappingAssemblies =
            {
                typeof(ArangoRepositoryBase).Assembly, typeof(SampleDataHelper).Assembly, GetType().Assembly
            };

            Mapper = new Mapper(
                new MapperConfiguration(
                    c =>
                        c.AddMaps(mappingAssemblies)));
        }

        private Task PrepareDatabaseAsync()
        {
            var seedingService = new SecondLevelProjectionSeedingService(GetClient());

            var preparationService = new SecondLevelProjPreparationService(
                seedingService,
                DefaultModelBuilderOptionsWrite);

            return preparationService.PrepareDatabaseAsync();
        }
        
        private IArangoDbClient GetClient()
        {
            var aFactory = GetServiceProvider().GetRequiredService<IArangoDbClientFactory>();

            return aFactory.Create(ArangoDbClientName);
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }

        public string GetArangoDbClientName()
        {
            return ArangoDbClientName;
        }

        public Task<IArangoDbClient> GetClientAsync()
        {
            return Task.FromResult(GetClient());
        }

        public Task<ISecondLevelProjectionRepository> GetSecondLevelRepository()
        {
            return Task.FromResult(GetServiceProvider().GetService<ISecondLevelProjectionRepository>());
        }

        public string GetCollectionName<T>()
        {
            return DefaultModelBuilderOptionsWrite.GetCollectionName<T>();
        }

        public Task<IPathTreeRepository> GetSecondLevelPathReeRepository()
        {
            return Task.FromResult(GetServiceProvider().GetRequiredService<IPathTreeRepository>());
        }
        

        public Task InitializeAsync()
        {
            return PrepareDatabaseAsync();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
