using System;

namespace UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Seeding.Attributes
{
    public class HasTagAttribute : Attribute
    {
        public string TagId { get; set; }
        public bool IsInheritable { get; set; }

        public HasTagAttribute(string tagId, bool isInheritable)
        {
            TagId = tagId;
            IsInheritable = isInheritable;
        }
    }
}
