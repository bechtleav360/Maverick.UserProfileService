using UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Seeding.Attributes;

namespace UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Data
{
    [SeedData]
    public static class TagWriteTestData
    {
        public static class DeleteTag
        {
            [Tag(nameof(DeleteTag))]
            public const string TagId = "e2c902e2-aafd-4b02-b4f8-868dd97778e3";
        }
    }
}
