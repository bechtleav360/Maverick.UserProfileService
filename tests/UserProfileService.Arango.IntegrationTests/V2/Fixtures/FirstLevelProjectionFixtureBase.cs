using System;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.Public.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using UserProfileService.Arango.IntegrationTests.V2.Abstractions;
using UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Seeding;
using UserProfileService.Projection.Abstractions;
using Xunit;

namespace UserProfileService.Arango.IntegrationTests.V2.Fixtures
{
    public abstract class FirstLevelProjectionFixtureBase : ArangoDbTestBase, IDisposable, IAsyncLifetime
    {
        public JsonSerializer JsonSerializer { get; } = JsonSerializer.CreateDefault(DefaultSerializerSettings);

        protected IArangoDbClient GetClient()
        {
            var aFactory = GetServiceProvider().GetRequiredService<IArangoDbClientFactory>();

            return aFactory.Create(ArangoDbClientName);
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        public Task InitializeAsync()
        {
            return FirstLevelProjTestsPreparationService.Instance.PrepareDatabaseAsync();
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }

        public Task<IArangoDbClient> GetClientAsync()
        {
            return Task.FromResult(GetClient());
        }

        public Task<IFirstLevelProjectionRepository> GetFirstLevelRepository()
        {
            return Task.FromResult(GetServiceProvider().GetService<IFirstLevelProjectionRepository>());
        }
    }
}
