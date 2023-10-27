using UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Annotations;

namespace UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Data
{
    [TestData(true, nameof(ProfileTestData))]
    public static class ProfileTestData
    {
        public static class UpdateUser
        {
            [User("updateUser - old name")]
            public const string UserId = "u_1ccde18d-f865-4db2-990b-336c98f80706";
        }

        public static class UpdateGroup
        {
            [Group("updateGroup - old name")]
            public const string GroupId = "g_1d82ab82-9da7-44e0-a7de-fdc8b9eee75b";
        }

        public static class UpdateOrganization
        {
            [Organization("updateOU - old name")]
            public const string OrganizationId = "o_803979c2-dcde-4209-92a2-a2a198db17bc";
        }
    }
}
