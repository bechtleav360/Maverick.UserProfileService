using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.Public;
using Maverick.Client.ArangoDb.Public.Configuration;
using Maverick.Client.ArangoDb.Public.Extensions;
using Maverick.Client.ArangoDb.Public.Models.Collection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserProfileService.Adapter.Arango.V2.Abstractions;
using UserProfileService.Adapter.Arango.V2.Implementations;
using UserProfileService.Arango.IntegrationTests.V2.Fixtures;
using UserProfileService.Arango.IntegrationTests.V2.Mocks;
using UserProfileService.Common.V2.Enums;
using UserProfileService.Common.V2.Models;
using Xunit;

namespace UserProfileService.Arango.IntegrationTests.V2
{
    [Collection(nameof(DatabaseCollection))]
    public class ArangoDbInitTests
    {
        private readonly DatabaseFixture _fixture;

        public ArangoDbInitTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        private async Task<List<string>> GetMissingCollections(params string[] collections)
        {
            IArangoDbClient client = await _fixture.GetClientAsync();

            GetAllCollectionsResponse storedCollectionsInfo = await client.GetAllCollectionsAsync(false);

            if (storedCollectionsInfo.Error)
            {
                throw new Exception("Exception during ArangoDb init test.", storedCollectionsInfo.Exception);
            }

            List<string> storedCollections = storedCollectionsInfo.Result.Select(c => c.Name).ToList();

            return collections.Except(storedCollections).ToList();
        }

        [Fact]
        public async Task InitializeDatabase()
        {
            await _fixture.PrepareAsync();
            IServiceProvider provider = _fixture.GetServiceProvider();

            var initializer = new ArangoDbInitializer(
                provider.GetRequiredService<IOptionsMonitor<ArangoConfiguration>>(),
                provider,
                provider.GetRequiredService<ILogger<ArangoDbInitializer>>(),
                _fixture.GetArangoDbClientName(),
                new List<ICollectionDetailsProvider>
                {
                    new MockCollectionsDetailsProvider("First", ACollectionType.Document, "dbInitTest_"),
                    new MockCollectionsDetailsProvider("Second", ACollectionType.Document, "dbInitTest_")
                });

            SchemaInitializationResponse result = await initializer.EnsureDatabaseAsync(true);

            Assert.Equal(SchemaInitializationStatus.SchemaCreated, result.Status);

            SchemaInitializationResponse secondRunResult = await initializer.EnsureDatabaseAsync(true);

            Assert.Equal(SchemaInitializationStatus.Checked, secondRunResult.Status);

            SchemaInitializationResponse thirdRunResult = await initializer.EnsureDatabaseAsync();

            Assert.Equal(SchemaInitializationStatus.WaitingForNextCheck, thirdRunResult.Status);

            List<string> missing = await GetMissingCollections("dbInitTest_First", "dbInitTest_Second");

            Assert.Empty(missing);
        }
    }
}
