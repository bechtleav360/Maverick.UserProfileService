using System;

namespace UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Annotations
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class FunctionAttribute : Attribute
    {
        public string Name { get; }

        public FunctionAttribute(string name)
        {
            Name = name;
        }

        public FunctionAttribute()
        {
        }
    }
}
