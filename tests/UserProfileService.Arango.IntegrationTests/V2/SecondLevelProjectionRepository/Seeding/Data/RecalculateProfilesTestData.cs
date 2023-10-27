using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Annotations;
using TagType = Maverick.UserProfileService.Models.EnumModels.TagType;

// ReSharper disable StringLiteralTypo

namespace UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Data
{
    [TestData(true, nameof(RecalculateProfilesTestData))]
    public static class RecalculateProfilesTestData
    {
        public static class RecalculateUserRelatedToGroupWithSubgroup
        {
            [User("Budnik, Calvin")]
            [AssignedTo(GroupId, ContainerType.Group)]
            [AssignedTo(SecondGroupId, ContainerType.Group, -1, 180, true)]
            [HasTag(DeveloperTagId)]
            public const string UserId = "user-d33f475a-e80e-44ce-8d76-740da9e58e30";

            [Group("The Killerbees")]
            [AssignedTo(RootGroupId, ContainerType.Group)]
            [HasTag(AwesomeEntitiesTagId, true)]
            public const string GroupId = "grp-842d9865-b474-471c-a266-c4a1e25a4459";

            [Group("The Cruel Wasps")]
            [HasTag(DreamLifeTagId, true)]
            public const string SecondGroupId = "grp-3760BF2C-92FA-4A69-84ED-1346C4366AAA";

            [Group("The cool root group #4711")]
            [HasTag(AwesomeEntitiesTagId, true)]
            [HasTag(RootGroupTagId)]
            [HasTag(NoPersonTagId)]
            public const string RootGroupId = "grp-e39559db-2134-404d-bcad-0bf8e5e6b928";

            [Tag("I am root", TagType.Custom)]
            public const string RootGroupTagId = "tag-91e8da5a-7d8f-4853-b115";

            [Tag("Awesome entities", TagType.Custom)]
            public const string AwesomeEntitiesTagId = "tag-0c45f7ab-d44d-4458-8770-c009";

            [Tag("Developer guild", TagType.Custom)]
            public const string DeveloperTagId = "tag-4d39b586-b10f-4df2-8f3f-b9fcb08b5d8b";

            [Tag("No person, sorry", TagType.Custom)]
            public const string NoPersonTagId = "tag-19da3908332749cf92a5064310e91df8";

            [Tag("Live your dream", TagType.Custom)]
            public const string DreamLifeTagId = "tag-5ec414f0-b729-4d2d-9a11-96bf1ca0646e";
        }

        public static class RecalculateRelatedUserInGroupWithModifiedGroupAssignment
        {
            [User("Helmut Kohl")]
            [AssignedTo(GroupOfUserId, ContainerType.Group, -11, 10)]
            public const string UserId = "user-c43d7f8c-1d77-4cf2-898d-c24f7fa7bc42";

            [Group("I am a group of a root group")]
            [HasTag(GroupInGroupTagId, true)]
            [AssignedTo(RootGroupId, ContainerType.Group, -1, 10, true)]
            public const string GroupOfUserId = "grp-grp-2d69131c-f16c-432b-bb79-0a4230470a10";

            [Group("Proud to be a root group")]
            [HasTag(RootGroupTagId, true)]
            public const string RootGroupId = "root-grp-7b6118a0-4a5f-4846-9189-7bd1efbc6ffc";

            [Tag("I am another root")]
            public const string RootGroupTagId = "root-tag-1f234dfa-ed82-44c3-95d4-7def0e2e9799";

            [Tag("Whatever")]
            public const string GroupInGroupTagId = "grp-tag-99180f9e-40a0-44a3-9dc7-fc72d9d2b0ce";
        }

        public static class RecalculateUnassignedGroupOfGroup
        {
            [Group("Mehmets Tester")]
            [HasTag(ChildTagId)]
            [AssignedTo(ParentGroupId, ContainerType.Group, -31, -1, true)]
            public const string GroupId = "grp-cfe13876-ba3c-41dd-ab89-d5b93c3906a0";

            [Group("AVS Tester")]
            [HasTag(ParentTagId)]
            public const string ParentGroupId = "grp-c6a4f20b-14b3-4f12-99c4-2738ff5a89ff";

            [Tag("Group of M. Tester")]
            public const string ChildTagId = "3b03845c-5148-4ff9-8f81-c495f8314907";

            [Tag("Group of AVS Tester")]
            public const string ParentTagId = "610eaa7a-9bd0-4bee-a930-fda5f69ef4f9";
        }
    }
}
