using System;
using System.Collections.Generic;
using FluentAssertions;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.FirstLevel.Extensions;
using Xunit;

namespace UserProfileService.Projection.FirstLevel.UnitTests.HandlerTests.MethodTest
{
    public class ClientSettingsExtensionMethodTests
    {
        private const string ProfileToChangeIdFirst = "C62732D9-382E-4D48-AA87-A6B6E2620A27";
        private const string ProfileToChangeIdSecond = "D4185AC2-2B08-4C00-918E-F4D51EDF4C5E";
        private static readonly DateTime _start = DateTime.UtcNow;

        public static IEnumerable<object[]> EmptyAndNullAssignmentsParameter =>
            new List<object[]>
            {
                new object[]
                { ClientSettingsNotValidBecauseOfRangeConditions, EmptyEventListResult, ProfileToChangeIdFirst },
                new object[] { ClientSettingOverwriteOwn, OverwriteClientSettingsWrongResult, ProfileToChangeIdFirst },
                new object[]
                { ClientSettingWeightDifferent, WeightDifferentClientSettingsResult, ProfileToChangeIdFirst },
                new object[]
                { EverythingSameExceptUpdatedAt, EverythingSameExceptUpdatedAtSettingsResult, ProfileToChangeIdSecond },
                new object[] { EverythingSameExceptHops, EverythingSameExceptHopsResult, ProfileToChangeIdSecond },
                new object[]
                {
                    EverythingSameExceptTheRageConditions, EverythingSameExceptTheRageConditionsResult,
                    ProfileToChangeIdSecond
                }
            };

        [Theory]
        [MemberData(nameof(EmptyAndNullAssignmentsParameter))]
        public void ClientSettingsToIUserProfileEvents(
            List<FirstLevelProjectionsClientSetting> clientSettingsToTransform,
            List<IUserProfileServiceEvent> expectedResultEvents,
            string profileId)
        {
            List<IUserProfileServiceEvent> methodResult =
                clientSettingsToTransform.GetClientSettingsCalculatedEvents(profileId);

            expectedResultEvents.Should()
                                .BeEquivalentTo(
                                    methodResult,
                                    opt => opt.RespectingRuntimeTypes());
        }

        [Fact]
        public void clientSettingsToIUserProfileEvents_null_parameter()
        {
            List<FirstLevelProjectionsClientSetting> clientSettingsToTransform = null;

            Assert.Throws<ArgumentNullException>(
                () => clientSettingsToTransform.GetClientSettingsCalculatedEvents(
                    "FEE2CB4D-4AAF-426F-B214-7066F511D685"));
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("\t\n")]
        public void clientSettingsToIUserProfileEvents_profileId_Invalid_ArgumentException(string profileId)
        {
            var clientSettings = new List<FirstLevelProjectionsClientSetting>();

            Assert.Throws<ArgumentException>(() => clientSettings.GetClientSettingsCalculatedEvents(profileId));
        }

        [Fact]
        public void clientSettingsToIUserProfileEvents_empty_parameter()
        {
            var clientSettingsToTransform = new List<FirstLevelProjectionsClientSetting>();

            List<IUserProfileServiceEvent> result =
                clientSettingsToTransform.GetClientSettingsCalculatedEvents("78B3A7B3-05A5-48CE-A392-5013205D7885");

            Assert.Empty(result);
        }

#region InvalidBecauseOfConditions

        internal static List<IUserProfileServiceEvent> EmptyEventListResult = new List<IUserProfileServiceEvent>();

        internal static List<FirstLevelProjectionsClientSetting> ClientSettingsNotValidBecauseOfRangeConditions =
            new List<FirstLevelProjectionsClientSetting>
            {
                new FirstLevelProjectionsClientSetting
                {
                    Conditions = new Dictionary<string, IList<RangeCondition>>
                                 {
                                     {
                                         "A5F82FDD-2A66-4B73-A739-4D8A640B07C7", new List<RangeCondition>
                                             {
                                                 new RangeCondition(_start.AddHours(2), _start.AddHours(23)),
                                                 new RangeCondition(_start.AddHours(23), _start.AddYears(23))
                                             }
                                     }
                                 },
                    ProfileId = ProfileToChangeIdFirst,
                    SettingsKey = "ClientSettingNotValid_1",
                    UpdatedAt = _start,
                    Weight = 2.0,
                    Value = "NotValidBecauseOfRangeConditions_2"
                },

                new FirstLevelProjectionsClientSetting
                {
                    Conditions = new Dictionary<string, IList<RangeCondition>>
                                 {
                                     {
                                         "A5F82FDD-2A66-4B73-A739-4D8A640B07C7", new List<RangeCondition>
                                             {
                                                 new RangeCondition(_start.AddYears(2), _start.AddYears(23)),
                                                 new RangeCondition(_start.AddYears(23), _start.AddYears(23))
                                             }
                                     }
                                 },
                    ProfileId = "37EB3837-DA55-460A-8204-4B5442F8D429",
                    SettingsKey = "ClientSettingNotValid_2",
                    UpdatedAt = _start,
                    Weight = 2.0,
                    Value = "NotValidBecauseOfRangeConditions"
                },

                new FirstLevelProjectionsClientSetting
                {
                    Conditions = new Dictionary<string, IList<RangeCondition>>
                                 {
                                     {
                                         "5F3D4D8D-6F19-4A87-9B57-32C88FD25472", new List<RangeCondition>
                                             {
                                                 new RangeCondition(_start.AddMonths(2), _start.AddMonths(23)),
                                                 new RangeCondition(_start.AddHours(23), _start.AddDays(23))
                                             }
                                     }
                                 },
                    ProfileId = "E7B4B3BA-9344-467D-825C-91EEAF53E278",
                    SettingsKey = "ClientSettingNotValid_3",
                    UpdatedAt = _start,
                    Weight = 2.0,
                    Value = "NotValidBecauseOfRangeConditions_3"
                }
            };

#endregion

#region OwnClientSettingOverwritteInheritated

        internal static List<IUserProfileServiceEvent> OverwriteClientSettingsWrongResult =
            new List<IUserProfileServiceEvent>
            {
                new ClientSettingsCalculated
                {
                    MetaData = new EventMetaData(),
                    CalculatedSettings = "0365-Premium",
                    Key = "Outlook",
                    ProfileId = ProfileToChangeIdFirst
                }
            };

        internal static List<FirstLevelProjectionsClientSetting> ClientSettingOverwriteOwn =
            new List<FirstLevelProjectionsClientSetting>
            {
                new FirstLevelProjectionsClientSetting
                {
                    Conditions = new Dictionary<string, IList<RangeCondition>>
                                 {
                                     {
                                         "Group-3039FD56-F36E-4217-AC21-0AD196681FF6", new List<RangeCondition>
                                             {
                                                 new RangeCondition(_start, _start.AddHours(23)),
                                                 new RangeCondition(_start, _start.AddYears(23))
                                             }
                                     }
                                 },
                    ProfileId = "CBDD438A-FC1E-4BFB-943D-92618866C760",
                    SettingsKey = "Outlook",
                    UpdatedAt = _start,
                    Weight = 1.0,
                    Hops = 1,
                    Value = "0365-TestVersion"
                },

                new FirstLevelProjectionsClientSetting
                {
                    Conditions = new Dictionary<string, IList<RangeCondition>>(),
                    ProfileId = "CBDD438A-FC1E-4BFB-943D-92618866C760",
                    SettingsKey = "Outlook",
                    UpdatedAt = _start,
                    Hops = 0,
                    Weight = 1.0,
                    Value = "0365-Premium"
                }
            };

#endregion

#region SameHopButWeightAreDifferent

        internal static List<IUserProfileServiceEvent> WeightDifferentClientSettingsResult =
            new List<IUserProfileServiceEvent>
            {
                new ClientSettingsCalculated
                {
                    MetaData = new EventMetaData(),
                    CalculatedSettings = "0365-TestVersion",
                    Key = "Outlook",
                    ProfileId = ProfileToChangeIdFirst
                }
            };

        internal static List<FirstLevelProjectionsClientSetting> ClientSettingWeightDifferent =
            new List<FirstLevelProjectionsClientSetting>
            {
                new FirstLevelProjectionsClientSetting
                {
                    Conditions = new Dictionary<string, IList<RangeCondition>>
                                 {
                                     {
                                         "D7A3A781-F33C-4F26-BE14-56711C403935", new List<RangeCondition>
                                             {
                                                 new RangeCondition(_start, _start.AddHours(22)),
                                                 new RangeCondition(_start, _start.AddYears(1))
                                             }
                                     }
                                 },
                    ProfileId = "09E583D4-03B5-420C-B80D-15CFD5C79948",
                    SettingsKey = "Outlook",
                    UpdatedAt = _start,
                    Weight = 4.0,
                    Hops = 0,
                    Value = "0365-TestVersion"
                },

                new FirstLevelProjectionsClientSetting
                {
                    Conditions = new Dictionary<string, IList<RangeCondition>>(),
                    ProfileId = "09E583D4-03B5-420C-B80D-15CFD5C79948",
                    SettingsKey = "Outlook",
                    UpdatedAt = _start,
                    Hops = 0,
                    Weight = 3.0,
                    Value = "0365-Premium"
                }
            };

#endregion

#region EverythingSameExceptUpdatedAt

        internal static List<IUserProfileServiceEvent> EverythingSameExceptUpdatedAtSettingsResult =
            new List<IUserProfileServiceEvent>
            {
                new ClientSettingsCalculated
                {
                    MetaData = new EventMetaData(),
                    CalculatedSettings = "0365-Premium",
                    Key = "Outlook",
                    ProfileId = ProfileToChangeIdSecond
                }
            };

        internal static List<FirstLevelProjectionsClientSetting> EverythingSameExceptUpdatedAt =
            new List<FirstLevelProjectionsClientSetting>
            {
                new FirstLevelProjectionsClientSetting
                {
                    Conditions = new Dictionary<string, IList<RangeCondition>>
                                 {
                                     {
                                         "D7A3A781-F33C-4F26-BE14-56711C403935", new List<RangeCondition>
                                             {
                                                 new RangeCondition(_start, _start.AddHours(22)),
                                                 new RangeCondition(_start, _start.AddYears(1))
                                             }
                                     }
                                 },
                    ProfileId = "09E583D4-03B5-420C-B80D-15CFD5C79948",
                    SettingsKey = "Outlook",
                    UpdatedAt = _start.AddDays(-2),
                    Weight = 3.0,
                    Hops = 0,
                    Value = "0365-TestVersion"
                },

                new FirstLevelProjectionsClientSetting
                {
                    Conditions = new Dictionary<string, IList<RangeCondition>>(),
                    ProfileId = "09E583D4-03B5-420C-B80D-15CFD5C79948",
                    SettingsKey = "Outlook",
                    UpdatedAt = _start,
                    Hops = 0,
                    Weight = 3.0,
                    Value = "0365-Premium"
                }
            };

#endregion

#region EverythingSameExceptHops

        internal static List<IUserProfileServiceEvent> EverythingSameExceptHopsResult =
            new List<IUserProfileServiceEvent>
            {
                new ClientSettingsCalculated
                {
                    MetaData = new EventMetaData(),
                    CalculatedSettings = "0365-Premium",
                    Key = "Outlook",
                    ProfileId = ProfileToChangeIdSecond,
                    IsInherited = true
                }
            };

        internal static List<FirstLevelProjectionsClientSetting> EverythingSameExceptHops =
            new List<FirstLevelProjectionsClientSetting>
            {
                new FirstLevelProjectionsClientSetting
                {
                    Conditions = new Dictionary<string, IList<RangeCondition>>
                                 {
                                     {
                                         "D7A3A781-F33C-4F26-BE14-56711C403935", new List<RangeCondition>
                                             {
                                                 new RangeCondition(_start, _start.AddHours(22)),
                                                 new RangeCondition(_start, _start.AddYears(1))
                                             }
                                     }
                                 },
                    ProfileId = "09E583D4-03B5-420C-B80D-15CFD5C79948",
                    SettingsKey = "Outlook",
                    UpdatedAt = _start,
                    Weight = 3.0,
                    Hops = 3,
                    Value = "0365-TestVersion"
                },

                new FirstLevelProjectionsClientSetting
                {
                    Conditions = new Dictionary<string, IList<RangeCondition>>
                                 {
                                     {
                                         "E070CCE5-86F7-4A33-A416-C91E3299E972", new List<RangeCondition>
                                             {
                                                 new RangeCondition(_start, _start.AddHours(222)),
                                                 new RangeCondition(_start, _start.AddYears(12))
                                             }
                                     }
                                 },
                    ProfileId = "09E583D4-03B5-420C-B80D-15CFD5C79948",
                    SettingsKey = "Outlook",
                    UpdatedAt = _start,
                    Hops = 2,
                    Weight = 3.0,
                    Value = "0365-Premium"
                }
            };

#endregion

#region EverythingSameExceptRangeConditions

        internal static List<IUserProfileServiceEvent> EverythingSameExceptTheRageConditionsResult =
            new List<IUserProfileServiceEvent>
            {
                new ClientSettingsCalculated
                {
                    MetaData = new EventMetaData(),
                    CalculatedSettings = "0365-Premium",
                    Key = "Outlook",
                    ProfileId = ProfileToChangeIdSecond,
                    IsInherited = true
                },
                new ClientSettingsCalculated
                {
                    MetaData = new EventMetaData(),
                    CalculatedSettings = "Klauke-AG",
                    Key = "AddressNow",
                    ProfileId = ProfileToChangeIdSecond,
                    IsInherited = true
                }
            };

        internal static List<FirstLevelProjectionsClientSetting> EverythingSameExceptTheRageConditions =
            new List<FirstLevelProjectionsClientSetting>
            {
                new FirstLevelProjectionsClientSetting
                {
                    Conditions = new Dictionary<string, IList<RangeCondition>>
                                 {
                                     {
                                         "D7A3A781-F33C-4F26-BE14-56711C403935", new List<RangeCondition>
                                             {
                                                 new RangeCondition(_start, _start.AddHours(22))
                                             }
                                     }
                                 },
                    ProfileId = "09E583D4-03B5-420C-B80D-15CFD5C79948",
                    SettingsKey = "AddressNow",
                    UpdatedAt = _start,
                    Weight = 3.0,
                    Hops = 3,
                    Value = "Klauke-AG"
                },

                new FirstLevelProjectionsClientSetting
                {
                    Conditions = new Dictionary<string, IList<RangeCondition>>
                                 {
                                     {
                                         "D7A3A781-F33C-4F26-BE14-56711C403935", new List<RangeCondition>
                                             {
                                                 new RangeCondition(_start.AddYears(2), _start.AddHours(22))
                                             }
                                     }
                                 },
                    ProfileId = "09E583D4-03B5-420C-B80D-15CFD5C79948",
                    SettingsKey = "AddressInNearFuture",
                    UpdatedAt = _start,
                    Weight = 3.0,
                    Hops = 3,
                    Value = "Bechtle-AG"
                },

                new FirstLevelProjectionsClientSetting
                {
                    Conditions = new Dictionary<string, IList<RangeCondition>>
                                 {
                                     {
                                         "E070CCE5-86F7-4A33-A416-C91E3299E972", new List<RangeCondition>
                                             {
                                                 new RangeCondition(_start, _start.AddHours(222)),
                                                 new RangeCondition(_start, _start.AddYears(12))
                                             }
                                     }
                                 },
                    ProfileId = "09E583D4-03B5-420C-B80D-15CFD5C79948",
                    SettingsKey = "Outlook",
                    UpdatedAt = _start,
                    Hops = 2,
                    Weight = 3.0,
                    Value = "0365-Premium"
                }
            };

#endregion
    }
}
