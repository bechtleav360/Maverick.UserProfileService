using System;

namespace UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Annotations
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class UserAttribute : Attribute
    {
        public string Name { get; }

        public UserAttribute(string name)
        {
            Name = name;
        }

        public UserAttribute()
        {
        }
    }
}
