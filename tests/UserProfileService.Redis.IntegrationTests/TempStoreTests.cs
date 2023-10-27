using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Redis.IntegrationTests.Fixtures;
using UserProfileService.Redis.IntegrationTests.Helpers;
using UserProfileService.Redis.IntegrationTests.Models;
using Xunit;

namespace UserProfileService.Redis.IntegrationTests
{
    public class TempStoreTests : IClassFixture<RedisFixture>
    {
        private readonly RedisFixture _fixture;

        public TempStoreTests(RedisFixture fixture)
        {
            _fixture = fixture;
        }

        private async Task<ITempStore> GetTempStore()
        {
            return (await _fixture.GetServiceProvider())
                .GetRequiredService<ITempStore>();
        }

        [Fact]
        public async Task SetRedisKey()
        {
            string key = _fixture.GetTempStoreNewKey();

            ITempStore tempStore = await GetTempStore();

            TestEntity entity = TestDataHelpers.GenerateTestEntity();

            await tempStore.SetAsync(key, entity, 60);

            await Task.Delay(100);

            var value = await tempStore.GetAsync<TestEntity>(key);

            Assert.Equal(entity, value, TestEntity.DefaultComparer);
        }

        [Fact]
        public async Task DeleteRedisKey()
        {
            string key = _fixture.GetTempStoreNewKey();

            ITempStore tempStore = await GetTempStore();

            TestEntity entity = TestDataHelpers.GenerateTestEntity();

            await tempStore.SetAsync(key, entity, 60 * 60);

            await Task.Delay(100);

            await tempStore.DeleteAsync(key);

            var value = await tempStore.GetAsync<TestEntity>(key);

            Assert.Null(value);
        }

        [Fact]
        public async Task AddRedisList()
        {
            string key = _fixture.GetTempStoreNewKey();

            ITempStore tempStore = await GetTempStore();

            IList<TestEntity> entities = TestDataHelpers.GenerateTestEntities();

            await tempStore.AddListAsync(key, entities, 60);

            await Task.Delay(100);

            IList<TestEntity> values = await tempStore.GetListAsync<TestEntity>(key);

            Assert.Equal(entities.Count, values.Count);
        }

        [Fact]
        public async Task AddRange()
        {
            string key = _fixture.GetTempStoreNewKey();

            ITempStore tempStore = await GetTempStore();

            IList<TestEntity> entities = TestDataHelpers.GenerateTestEntities();
            TestEntity entity = TestDataHelpers.GenerateTestEntity();

            await tempStore.AddListAsync(key, entities, 60);
            await tempStore.AddListAsync(key, entity.AsEnumerable());

            await Task.Delay(100);

            IList<TestEntity> values = await tempStore.GetListAsync<TestEntity>(key);

            Assert.Equal(entities.Count + 1, values.Count);
        }

        [Fact]
        public async Task GetPartOfList()
        {
            ITempStore tempStore = await GetTempStore();

            IList<TestEntity> firstPage = await tempStore.GetListAsync<TestEntity>(
                _fixture.ExampleListKey,
                0,
                2);

            Assert.Equal(3, firstPage.Count);

            IList<TestEntity> secondPage = await tempStore.GetListAsync<TestEntity>(
                _fixture.ExampleListKey,
                3,
                5);

            Assert.Equal(3, secondPage.Count);

            Assert.NotEqual(firstPage, secondPage, TestEntity.DefaultComparer);

            IList<TestEntity> thirdPage = await tempStore.GetListAsync<TestEntity>(
                _fixture.ExampleListKey,
                6,
                9);

            Assert.Equal(
                _fixture.ExampleList,
                firstPage.Concat(secondPage).Concat(thirdPage),
                TestEntity.DefaultComparer);
        }
    }
}
