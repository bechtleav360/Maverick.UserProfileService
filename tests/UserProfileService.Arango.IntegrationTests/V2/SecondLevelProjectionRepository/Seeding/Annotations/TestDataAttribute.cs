using System;

namespace UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Annotations
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TestDataAttribute : Attribute
    {
        public string TestCase { get; }

        public bool ForWriteTest { get; }

        public TestDataAttribute(bool forWriteTest, string testCase)
        {
            ForWriteTest = forWriteTest;
            TestCase = testCase;
        }
    }
}
