using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserProfileService.Adapter.Arango.V2.Contracts;
using UserProfileService.Adapter.Arango.V2.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.TicketStore.Abstractions;
using Xunit;

namespace UserProfileService.Arango.UnitTests.V2
{
    public class ServiceCollectionExtensionTests
    {
        private readonly IConfigurationSection _arangoConfiguration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string>
                {
                    {
                        "Arango:ConnectionString",
                        "Endpoints=http://localhost:1234;database=test;username=user;password=123"
                    }
                })
            .Build()
            .GetSection("Arango");

        [Fact]
        public void AddProfileStorageRead()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddArangoRepositoriesToReadFromProfileStorage(
                _arangoConfiguration,
                WellKnownDatabaseKeys.CollectionPrefixUserProfileService);

            ServiceProvider serviceProvider = services.BuildServiceProvider();

            serviceProvider.GetRequiredService<IReadService>();
        }

        [Fact]
        public void AddTicketStore()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddArangoTicketStore(_arangoConfiguration, "ServiceApiTest");

            ServiceProvider serviceProvider = services.BuildServiceProvider();

            serviceProvider.GetRequiredService<ITicketStore>();
        }
    }
}
