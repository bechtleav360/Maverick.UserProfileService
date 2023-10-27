using System.Collections.Generic;
using System.Linq;
using Bogus;
using UserProfileService.Redis.IntegrationTests.Models;

namespace UserProfileService.Redis.IntegrationTests.Helpers
{
    public static class TestDataHelpers
    {
        private static Faker<TestEntity> GetTestEntityFaker()
        {
            return new Faker<TestEntity>()
                .RuleFor(
                    e => e.Name,
                    faker => faker.Name.FullName())
                .RuleFor(
                    e => e.Updated,
                    faker => faker.Date.Past())
                .RuleFor(
                    e => e.Weight,
                    faker => faker.Random.Double(max: 10000));
        }

        public static IList<TestEntity> GenerateTestEntities(int amount = 10)
        {
            return GetTestEntityFaker().Generate(amount);
        }

        public static TestEntity GenerateTestEntity()
        {
            return GetTestEntityFaker().Generate(1).First();
        }
    }
}
