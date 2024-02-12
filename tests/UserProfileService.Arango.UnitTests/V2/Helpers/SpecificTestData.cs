using System;
using System.Collections.Generic;
using Maverick.UserProfileService.Models.EnumModels;
using Newtonsoft.Json.Linq;
using UserProfileService.Adapter.Arango.V2.EntityModels;

namespace UserProfileService.Arango.UnitTests.V2.Helpers
{
    /// <summary>
    ///     Contains test data for specific tests
    /// </summary>
    public static class SpecificTestData
    {
        public static class ProfileSettings
        {
            public const string FirstGroupIdWithOlderConfig = "Grp-1";
            public const string SecondGroupIdWithNewerConfig = "Grp-2";
            public const string UserId = "User-1";
            public const string FirstGroupSettingsJson = "{\"value1\":\"23\",\"value3\":\"42\"}";

            public const string SecondGroupSettingsJson =
                "{\"value1\":\"mytest#1\",\"value4\":\"whatever\",\"value3\":\"me again\"}";

            public const string UserSettingsJson = "{\"value1\":\"89-Z\",\"value2\":\"42\"}";
            public const string SettingsKey = "user-config";

            internal static IEnumerable<ClientSettingsEntityModel> GetSettingsEntities(bool justGroups)
            {
                if (!justGroups)
                {
                    yield return new ClientSettingsEntityModel
                    {
                        Hops = 1,
                        ProfileId = FirstGroupIdWithOlderConfig,
                        Value = JObject.Parse(FirstGroupSettingsJson),
                        SettingsKey = SettingsKey,
                        UpdatedAt = new DateTime(
                            2021,
                            5,
                            1,
                            13,
                            56,
                            4),
                        Kind = ProfileKind.User,
                        Weight = 23
                    };
                }

                yield return new ClientSettingsEntityModel
                {
                    Hops = 1,
                    ProfileId = SecondGroupIdWithNewerConfig,
                    Value = JObject.Parse(SecondGroupSettingsJson),
                    SettingsKey = SettingsKey,
                    UpdatedAt = new DateTime(
                        2021,
                        7,
                        1,
                        8,
                        23,
                        56),
                    Kind = ProfileKind.Group,
                    Weight = 580
                };

                yield return new ClientSettingsEntityModel
                {
                    Hops = 0,
                    ProfileId = UserId,
                    Value = JObject.Parse(UserSettingsJson),
                    SettingsKey = SettingsKey,
                    UpdatedAt = new DateTime(
                        2021,
                        1,
                        2,
                        12,
                        38,
                        41),
                    Kind = ProfileKind.Group,
                    Weight = 580
                };
            }
        }
    }
}
