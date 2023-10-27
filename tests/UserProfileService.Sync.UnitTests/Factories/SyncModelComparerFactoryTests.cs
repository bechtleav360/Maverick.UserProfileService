using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UserProfileService.Sync.Abstraction.Configurations;
using UserProfileService.Sync.Abstraction.Models;
using UserProfileService.Sync.Abstraction.Models.Entities;
using UserProfileService.Sync.Abstractions;
using UserProfileService.Sync.Factories;
using UserProfileService.Sync.Services.Comparer;
using Xunit;

namespace UserProfileService.Sync.UnitTests.Factories
{
    public class SyncModelComparerFactoryTests
    {
        private static SyncModelComparerFactory InitializeFactory<TSyncComparer>()
        {
            var loggerFactory = new LoggerFactory();
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddTransient<ILoggerFactory>(s => loggerFactory);
            serviceCollection.Configure<SyncConfiguration>(t => { t = new SyncConfiguration(); });

            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            ILogger<SyncModelComparerFactory> logger = loggerFactory.CreateLogger<SyncModelComparerFactory>();

            var factory = new SyncModelComparerFactory(serviceProvider, logger);

            return factory;
        }

        [Fact]
        public void CreateComparer_Should_Create_GroupSyncComparer()
        {
            // Arrange
            SyncModelComparerFactory factory = InitializeFactory<GroupSyncComparer>();

            // Act
            ISyncModelComparer<GroupSync> comparer = factory.CreateComparer<GroupSync>();

            // Assert
            Assert.NotNull(comparer);
            Assert.IsType<GroupSyncComparer>(comparer);
        }

        [Fact]
        public void CreateComparer_Should_Create_Null()
        {
            // Arrange
            SyncModelComparerFactory factory = InitializeFactory<GroupSyncComparer>();

            // Act
            ISyncModelComparer<TestSyncModel> comparer = factory.CreateComparer<TestSyncModel>();

            // Assert
            Assert.Null(comparer);
        }
    }

    internal class TestSyncModel : ISyncModel
    {
        public string Id { get; set; }
        public string Source { get; set; }
        public IList<KeyProperties> ExternalIds { get; set; }
        public List<ObjectRelation> RelatedObjects { get; set; }
    }
}
