using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using StackExchange.Redis;
using UserProfileService.Common.Tests.Utilities.Utilities;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Redis.Configuration;
using UserProfileService.Redis.IntegrationTests.Helpers;
using UserProfileService.Redis.IntegrationTests.Models;

namespace UserProfileService.Redis.IntegrationTests.Fixtures
{
    public class RedisFixture : IDisposable
    {
        private readonly AsyncLazy _preparationTask;
        public IServiceCollection Services { get; }

        public string ExampleListKey => GetTempStoreNewKey("exampleList");

        public IList<TestEntity> ExampleList { get; }

        public RedisFixture()
        {
            Services = new ServiceCollection();

            Services.AddLogging(b => b.AddSimpleLogMessageCheckLogger(true));

            RedisConfiguration redis = ConfigurationHelpers.GetRedisSettings();

            Services.Configure<RedisConfiguration>(
                ConfigurationHelpers.GetConfiguration().GetSection(ConfigurationHelpers.ConfigurationKeyRedis));

            var settings = new ConfigurationOptions
            {
                AllowAdmin = redis.AllowAdmin,
                AbortOnConnectFail = redis.AbortOnConnectFail,
                ConnectTimeout = redis.ConnectTimeout,
                ConnectRetry = redis.ConnectRetry,
                User = redis.User,
                Password = redis.Password
            };

            redis.EndpointUrls.ForEach(url => settings.EndPoints.Add(url));

            IConnectionMultiplexer connectionMultiplexer = ConnectionMultiplexer.Connect(settings);

            Services.AddSingleton(connectionMultiplexer);

            Services.AddSingleton<ITempStore, RedisTempObjectStore>();

            ExampleList = TestDataHelpers.GenerateTestEntities();

            _preparationTask = new AsyncLazy(PrepareAsync);
        }

        public async Task<IServiceProvider> GetServiceProvider()
        {
            await _preparationTask;

            return Services.BuildServiceProvider();
        }

        public string GetTempStoreNewKey(string id)
        {
            return $"Ups/RedisTest/TempStore_{id}";
        }

        public string GetTempStoreNewKey()
        {
            return GetTempStoreNewKey(Guid.NewGuid().ToString());
        }

        public async Task PrepareAsync()
        {
            using IServiceScope scope = Services.BuildServiceProvider().CreateScope();
            var connection = scope.ServiceProvider.GetRequiredService<IConnectionMultiplexer>();

            IDatabase db = connection.GetDatabase();

            ITransaction transaction = db.CreateTransaction();

#pragma warning disable 4014
            // transaction commands cannot be awaited otherwise a dead lock will occur.
            transaction.KeyDeleteAsync(ExampleListKey);

            transaction
                .ListRightPushAsync(
                    ExampleListKey,
                    ExampleList
                        .Select(e => new RedisValue(JsonConvert.SerializeObject(e)))
                        .ToArray());

            transaction
                .KeyExpireAsync(
                    ExampleListKey,
                    TimeSpan.FromMinutes(10));
#pragma warning restore 4014

            await transaction.ExecuteAsync();
        }

        public void Dispose()
        {
        }
    }
}
