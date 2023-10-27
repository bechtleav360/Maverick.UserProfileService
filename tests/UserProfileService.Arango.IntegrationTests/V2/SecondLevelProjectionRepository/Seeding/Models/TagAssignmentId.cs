namespace UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Models
{
    public struct TagAssignmentId
    {
        public string TagId { get; set; }
        public bool IsInheritable { get; set; }

        public TagAssignmentId(string tagId)
        {
            TagId = tagId;
            IsInheritable = false;
        }

        public TagAssignmentId(string tagId, bool isInheritable)
        {
            TagId = tagId;
            IsInheritable = isInheritable;
        }
    }
}
