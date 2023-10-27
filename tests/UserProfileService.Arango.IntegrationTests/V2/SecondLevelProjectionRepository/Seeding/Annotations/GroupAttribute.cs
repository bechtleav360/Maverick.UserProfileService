using System;

namespace UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Annotations
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class GroupAttribute : Attribute
    {
        public string Name { get; }

        public GroupAttribute(string name)
        {
            Name = name;
        }

        public GroupAttribute()
        {
        }
    }
}
