using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserProfileService.Sync.Abstraction.Models.Entities;
using UserProfileService.Sync.Abstraction.Models.Results;
using UserProfileService.Sync.Configuration;
using UserProfileService.Sync.Systems;
using Xunit;

namespace UserProfileService.Sync.UnitTests.Systems
{
    public class LdapSourceSystemTests
    {
        private const string FilePath = "Configuration/LdapConfiguration.json";
        private const string ConfigurationSection = "System:Ldap";
        
        [Theory]
        [InlineData(0, 0, 500)]
        [InlineData(250, 0, 500)]
        [InlineData(500, 0, 500)]
        [InlineData(1000, 0, 500)]
        [InlineData(1426, 0, 500)]
        [InlineData(1499, 0, 500)]
        [InlineData(1501, 0, 500)]
        public void GetBatchAsync_ShouldReturnAllUser(int total, int startPosition, int batchSize)
        {
            var config = new ConfigurationBuilder().AddJsonFile(FilePath).Build();
            
            var services = new ServiceCollection();
            services.AddLogging();
            services.Configure<LdapSystemConfiguration>(config.GetSection(ConfigurationSection));

            var provider = services.BuildServiceProvider();

            // Arrange
            var sourceSystem = new LdapSourceSystem(
                provider.GetRequiredService<IOptionsSnapshot<LdapSystemConfiguration>>(),
                provider.GetRequiredService<ILogger<LdapSourceSystem>>());

            List<UserSync> users = Enumerable.Range(0, total)
                                             .Select(
                                                 t => new UserSync
                                                      {
                                                          Id = $"{t}"
                                                      })
                                             .ToList();

            // Act
            var entities = new List<UserSync>();

            IBatchResult<UserSync> result;

            do
            {
                result = sourceSystem.GetBatchResult(users, startPosition, batchSize);
                entities.AddRange(result.Result);
                startPosition += batchSize;
            }
            while (result.NextBatch);

            // Arrange
            Assert.Equal(total, entities.Count);
        }
    }
}
