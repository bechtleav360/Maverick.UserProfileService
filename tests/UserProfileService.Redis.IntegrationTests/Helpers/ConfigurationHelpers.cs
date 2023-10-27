using Microsoft.Extensions.Configuration;
using UserProfileService.Redis.Configuration;

namespace UserProfileService.Redis.IntegrationTests.Helpers
{
    public static class ConfigurationHelpers
    {
        public const string ConfigurationKeyRedis = "Redis";

        public static IConfigurationRoot GetConfiguration()
        {
            return new ConfigurationBuilder()
                .AddJsonFile(FileNameConstants.ConfigurationFile)
                .Build();
        }

        public static RedisConfiguration GetRedisSettings()
        {
            return GetConfiguration()
                .GetSection(ConfigurationKeyRedis)
                .Get<RedisConfiguration>();
        }
    }
}
