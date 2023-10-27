using System;

namespace UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Annotations
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class RoleAttribute : Attribute
    {
        public string Name { get; }

        public RoleAttribute(string name)
        {
            Name = name;
        }

        public RoleAttribute()
        {
        }
    }
}
