using System;
using UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Seeding.Models;

namespace UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Seeding.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SeedGeneralDataAttribute : Attribute
    {
        public Type EntityType { get; }
        public string KeyPropertyName { get; }
        public TestType TestScope { get; }

        public SeedGeneralDataAttribute(
            Type entityType,
            TestType testType)
            : this(entityType, "Id", testType)
        {
        }

        public SeedGeneralDataAttribute(Type entityType) : this(entityType, "Id", TestType.Undefined)
        {
        }

        public SeedGeneralDataAttribute(
            Type entityType,
            string keyPropertyName,
            TestType testType)
        {
            EntityType = entityType;
            KeyPropertyName = keyPropertyName;
            TestScope = testType;
        }
    }
}
