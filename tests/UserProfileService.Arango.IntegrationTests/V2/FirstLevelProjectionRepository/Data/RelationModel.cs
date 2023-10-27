using UserProfileService.Projection.Abstractions.EnumModels;

namespace UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Data
{
    public class RelationTestModel
    {
        public string ProfileId { get; set; }

        public FirstLevelMemberRelation Relation { get; set; }

        public RelationTestModel(string profileId, FirstLevelMemberRelation relation)
        {
            ProfileId = profileId;
            Relation = relation;
        }
    }
}
