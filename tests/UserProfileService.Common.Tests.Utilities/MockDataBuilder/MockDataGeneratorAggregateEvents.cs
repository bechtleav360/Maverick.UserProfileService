using System.Linq;
using Bogus;
using Maverick.UserProfileService.AggregateEvents.Common;

namespace UserProfileService.Common.Tests.Utilities.MockDataBuilder
{
    public class MockDataGeneratorAggregateEvents
    {
        public static EventMetaData GenerateEventMetaData()
        {
            return new Faker<EventMetaData>()
                .RuleFor(
                    md => md.CorrelationId,
                    faker => faker.Random.Guid().ToString())
                .RuleFor(
                    md => md.RelatedEntityId,
                    faker => faker.Random.AlphaNumeric(40))
                .RuleFor(
                    md => md.Timestamp,
                    faker => faker.Date.Past())
                .RuleFor(
                    md => md.VersionInformation,
                    _ => 1L)
                .RuleFor(
                    md => md.Initiator,
                    _ => GenerateEventInitiator())
                .Generate(1)
                .Single();
        }

        public static EventInitiator GenerateEventInitiator()
        {
            return new Faker<EventInitiator>()
                .RuleFor(
                    i => i.Id,
                    faker => faker.System.AndroidId())
                .RuleFor(
                    i => i.Type,
                    faker => faker.PickRandom<InitiatorType>())
                .Generate(1)
                .Single();
        }
    }
}
