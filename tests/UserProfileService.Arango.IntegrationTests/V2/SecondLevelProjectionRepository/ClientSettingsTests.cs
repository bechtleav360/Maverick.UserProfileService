using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Maverick.Client.ArangoDb.Public;
using Newtonsoft.Json.Linq;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Arango.IntegrationTests.V2.Abstractions;
using UserProfileService.Arango.IntegrationTests.V2.Fixtures;
using UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Data;
using UserProfileService.Projection.Abstractions;
using Xunit;

namespace UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository
{
    [Collection(nameof(SecondLevelProjectionCollection))]
    public class ClientSettingsTests : ArangoSecondLevelRepoTestBase
    {
        private readonly SecondLevelProjectionFixture _fixture;

        public ClientSettingsTests(SecondLevelProjectionFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Add_new_client_settings_should_work()
        {
            string settingsCollection = GetCollectionName<ClientSettingsEntityModel>();

            ISecondLevelProjectionRepository repo = await _fixture.GetSecondLevelRepository();
            string userId = ClientSettingsTestData.NewSettings.NewSettingUserId;

            await repo.SetClientSettingsAsync(
                userId,
                ClientSettingsTestData.SettingsKey,
                ClientSettingsTestData.NewSettings.NewSettingsValue,
                false);

            MultiApiResponse<ClientSettingsEntityModel> settings =
                await GetArangoClient()
                    .ExecuteQueryAsync<ClientSettingsEntityModel>(
                        $"for cs in {settingsCollection} filter cs.ProfileId==\"{userId}\" return cs");

            Assert.Single(settings.QueryResult);
            ClientSettingsEntityModel entry = settings.QueryResult.First();
            entry.ProfileId.Should().Be(userId);
            entry.IsInherited.Should().Be(false);
            entry.SettingsKey.Should().Be(ClientSettingsTestData.SettingsKey);
            entry.Value.Should().BeEquivalentTo(JObject.Parse(ClientSettingsTestData.NewSettings.NewSettingsValue));
        }

        [Fact]
        public async Task Invalidate_existing_client_settings_keys_should_work()
        {
            string userId = ClientSettingsTestData.InvalidateSettings.InvalidateSettingsUserId;
            string settingsCollection = GetCollectionName<ClientSettingsEntityModel>();

            ISecondLevelProjectionRepository repo = await _fixture.GetSecondLevelRepository();

            await repo.InvalidateClientSettingsFromProfile(
                userId,
                ClientSettingsTestData.InvalidateSettings.InvalidateSettingsRemainingKeys);

            MultiApiResponse<ClientSettingsEntityModel> settings =
                await GetArangoClient()
                    .ExecuteQueryAsync<ClientSettingsEntityModel>(
                        $"for cs in {settingsCollection} filter cs.ProfileId==\"{userId}\" return cs");

            Assert.Equal(2, settings.QueryResult.Count);

            settings.QueryResult
                .Select(s => s.SettingsKey)
                .Should()
                .BeEquivalentTo(ClientSettingsTestData.InvalidateSettings.InvalidateSettingsRemainingKeys);
        }

        [Fact]
        public async Task Unset_existing_client_settings_key_should_work()
        {
            string userId = ClientSettingsTestData.UnsetSettings.UnsetSettingsUserId;
            string settingsCollection = GetCollectionName<ClientSettingsEntityModel>();

            ISecondLevelProjectionRepository repo = await _fixture.GetSecondLevelRepository();

            await repo.UnsetClientSettingFromProfileAsync(
                userId,
                ClientSettingsTestData.SettingsKey);

            MultiApiResponse<ClientSettingsEntityModel> settings =
                await GetArangoClient()
                    .ExecuteQueryAsync<ClientSettingsEntityModel>(
                        $"for cs in {settingsCollection} filter cs.ProfileId==\"{userId}\" return cs");

            Assert.Empty(settings.QueryResult);
        }

        [Fact]
        public async Task Update_existing_client_settings_should_work()
        {
            string userId = ClientSettingsTestData.UpdateSettings.UpdateSettingUserId;
            string clientSettingsKey = ClientSettingsTestData.SettingsKey;
            string clientSettingsValue = ClientSettingsTestData.UpdateSettings.UpdateSettingsValue;

            string settingsCollection = GetCollectionName<ClientSettingsEntityModel>();

            ISecondLevelProjectionRepository repo = await _fixture.GetSecondLevelRepository();

            await repo.SetClientSettingsAsync(
                userId,
                clientSettingsKey,
                clientSettingsValue,
                false);

            MultiApiResponse<ClientSettingsEntityModel> settings =
                await GetArangoClient()
                    .ExecuteQueryAsync<ClientSettingsEntityModel>(
                        $"for cs in {settingsCollection} filter cs.ProfileId==\"{userId}\" return cs");

            Assert.Single(settings.QueryResult);
            ClientSettingsEntityModel entry = settings.QueryResult.First();
            entry.ProfileId.Should().Be(userId);
            entry.IsInherited.Should().Be(false);
            entry.SettingsKey.Should().Be(clientSettingsKey);
            entry.Value.Should().BeEquivalentTo(JObject.Parse(clientSettingsValue));
        }
    }
}
