using System;

namespace UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Annotations
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class OrganizationAttribute : Attribute
    {
        public string Name { get; }

        public OrganizationAttribute()
        {
        }

        public OrganizationAttribute(string name)
        {
            Name = name;
        }
    }
}
