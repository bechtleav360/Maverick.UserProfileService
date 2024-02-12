using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.Public.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UserProfileService.Adapter.Arango.V2.Contracts;
using UserProfileService.Adapter.Arango.V2.Implementations;
using UserProfileService.Arango.UnitTests.V2.Helpers;
using UserProfileService.Arango.UnitTests.V2.Mocks;
using UserProfileService.Common.V2.Enums;
using UserProfileService.Common.V2.Models;
using Xunit;
using Xunit.Abstractions;

namespace UserProfileService.Arango.UnitTests.V2
{
    public class DbInitializerTests
    {
        private static readonly ILoggerFactory _loggerFactory =
            LoggerFactory.Create(b => b.SetMinimumLevel(LogLevel.Debug).AddDebug());

        private readonly ITestOutputHelper _output;

        public DbInitializerTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private static IHttpClientFactory GetHttpClientFactory(
            Func<HttpRequestMessage, Task<HttpResponseMessage>> messageHandler,
            ITestOutputHelper output)
        {
            return new MockHttpClientFactory(
                $"^{ArangoConstants.DatabaseClientNameUserProfileStorage}$",
                () => new HttpClient(HttpMockHelpers.GetHttpMessageHandlerMock(messageHandler).Object),
                output);
        }

        [Fact]
        public async Task Ensure_database()
        {
            var arango = new MockedArangoDb(_output, _ => true);

            IArangoDbClientFactory clientFactory
                = new SingletonArangoDbClientFactory(GetHttpClientFactory(arango.HandleMessage, _output))
                    .AddClient(ArangoConstants.DatabaseClientNameUserProfileStorage);

            IServiceCollection serviceCollection = new ServiceCollection().AddSingleton(clientFactory);
            ServiceProvider provider = serviceCollection.BuildServiceProvider();

            var dbInit = new ArangoDbInitializer(
                new MockArangoConfigInOptionsMonitor(),
                provider,
                _loggerFactory.CreateLogger<ArangoDbInitializer>(),
                new List<MockCollectionsDetailsProvider>
                {
                    new MockCollectionsDetailsProvider(WellKnownDatabaseKeys.CollectionPrefixUserProfileService)
                });

            SchemaInitializationResponse result = await dbInit.EnsureDatabaseAsync();

            Assert.NotEqual(SchemaInitializationStatus.ErrorOccurred, result.Status);
            Assert.Null(result.Exception);
            Assert.True(arango.History.Count(i => i.Action == "created") == 1);
        }
    }
}
