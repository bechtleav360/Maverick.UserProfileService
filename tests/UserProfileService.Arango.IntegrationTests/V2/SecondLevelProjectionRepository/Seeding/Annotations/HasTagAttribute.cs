using System;

namespace UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Annotations
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class HasTagAttribute : Attribute
    {
        public string TagId { get; }
        public bool IsInheritable { get; }

        public HasTagAttribute(string tagId)
        {
            TagId = tagId;
        }

        public HasTagAttribute(string tagId, bool isInheritable)
        {
            TagId = tagId;
            IsInheritable = isInheritable;
        }
    }
}
