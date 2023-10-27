using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.Public.Extensions;
using Maverick.Client.ArangoDb.Public.Models.Document;
using Newtonsoft.Json.Linq;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding;
using UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Abstractions;
using UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Annotations;
using UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Data;
using UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Implementations;

namespace UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository
{
    internal class SecondLevelProjPreparationService
    {
        private readonly ModelBuilderOptions _modelBuilderOptions;
        private readonly SecondLevelProjectionSeedingService _seedingService;

        public SecondLevelProjPreparationService(
            SecondLevelProjectionSeedingService seedingService,
            ModelBuilderOptions modelBuilderOptions)
        {
            _seedingService = seedingService;
            _modelBuilderOptions = modelBuilderOptions;
        }

        private async Task SeedDataAsync()
        {
            foreach (ITestData data in
                     GetType()
                         .Assembly
                         .GetTypes()
                         .Where(t => t.GetCustomAttribute<TestDataAttribute>() != null)
                         .Select(t => (ITestData)new ReflectionTestData(t.ToSeedObjects())))
            {
                await _seedingService.SeedDataAsync(data, _modelBuilderOptions);
            }

            await _seedingService.ExecuteSeedAsync(SeedDataForClientSettings);
        }

        private async Task SeedDataForClientSettings(IArangoDbClient client)
        {
            string settingsCollectionName = _modelBuilderOptions
                .GetCollectionName<ClientSettingsEntityModel>();

            CreateDocumentsResponse response = await client.CreateDocumentsAsync(
                settingsCollectionName,
                new List<ClientSettingsEntityModel>
                {
                    new ClientSettingsEntityModel
                    {
                        ProfileId = ClientSettingsTestData.UpdateSettings
                            .UpdateSettingUserId,
                        Value = JObject.Parse("{ \"this could be\": \"an issue\" }"),
                        SettingsKey = ClientSettingsTestData.SettingsKey,
                        IsInherited = true
                    },
                    new ClientSettingsEntityModel
                    {
                        ProfileId = ClientSettingsTestData.UnsetSettings.UnsetSettingsUserId,
                        Value = JObject.Parse("{ \"i will be\": \"deleted\" }"),
                        SettingsKey = ClientSettingsTestData.SettingsKey,
                        IsInherited = false
                    },
                    new ClientSettingsEntityModel
                    {
                        ProfileId = ClientSettingsTestData.InvalidateSettings.InvalidateSettingsUserId,
                        Value = JObject.Parse(ClientSettingsTestData.InvalidateSettings.InvalidateSettingsValue),
                        SettingsKey = ClientSettingsTestData.InvalidateSettings.InvalidateSettingsRemainingKeys[0],
                        IsInherited = false
                    },
                    new ClientSettingsEntityModel
                    {
                        ProfileId = ClientSettingsTestData.InvalidateSettings.InvalidateSettingsUserId,
                        Value = JObject.Parse(ClientSettingsTestData.InvalidateSettings.InvalidateSettingsValue),
                        SettingsKey = "delete#me",
                        IsInherited = true
                    },
                    new ClientSettingsEntityModel
                    {
                        ProfileId = ClientSettingsTestData.InvalidateSettings.InvalidateSettingsUserId,
                        Value = JObject.Parse(ClientSettingsTestData.InvalidateSettings.InvalidateSettingsValue),
                        SettingsKey = ClientSettingsTestData.InvalidateSettings.InvalidateSettingsRemainingKeys[1],
                        IsInherited = true
                    },
                    new ClientSettingsEntityModel
                    {
                        ProfileId = ClientSettingsTestData.InvalidateSettings.InvalidateSettingsUserId,
                        Value = JObject.Parse(ClientSettingsTestData.InvalidateSettings.InvalidateSettingsValue),
                        SettingsKey = "I am wrong",
                        IsInherited = false
                    }
                });

            if (response.Error)
            {
                throw new Exception();
            }
        }

        public async Task PrepareDatabaseAsync()
        {
            await _seedingService.CleanupDatabaseAsync(_modelBuilderOptions);
            await _seedingService.CreateCollectionsAsync(_modelBuilderOptions);
            await SeedDataAsync();
        }
    }
}
