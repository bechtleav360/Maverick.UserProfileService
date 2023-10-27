using UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Annotations;

namespace UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Data
{
    [TestData(true, nameof(RoleTestData))]
    public static class RoleTestData
    {
        public static class UpdateRole
        {
            [Role("role with a changeable name")]
            public const string RoleId = "role_dab57a69-da55-45a4-ac51-b3e48a86e2c2";
        }
    }
}
