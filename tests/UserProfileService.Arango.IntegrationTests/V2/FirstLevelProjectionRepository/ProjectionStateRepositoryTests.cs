using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using Maverick.Client.ArangoDb.Public;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Adapter.Arango.V2.Extensions;
using UserProfileService.Arango.IntegrationTests.V2.Abstractions;
using UserProfileService.Arango.IntegrationTests.V2.Fixtures;
using UserProfileService.Projection.Abstractions;
using Xunit;

namespace UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository
{
    [Collection(nameof(FirstLevelProjectionCollection))]
    public class ProjectionStateRepositoryTests : ArangoFirstLevelRepoTestBase
    {
        private readonly FirstLevelProjectionFixture _fixture;

        public ProjectionStateRepositoryTests(FirstLevelProjectionFixture fixture)
        {
            _fixture = fixture;
        }

        private ProjectionState GenerateProjectionState(long eventNumber, string streamName)
        {
            return new Faker<ProjectionState>()
                .RuleFor(s => s.StreamName, _ => streamName)
                .RuleFor(s => s.ErrorMessage, f => f.Name.JobType())
                .RuleFor(s => s.EventId, f => f.Random.AlphaNumeric(36))
                .RuleFor(s => s.EventName, f => f.Name.FirstName())
                .RuleFor(s => s.StackTraceMessage, f => f.Name.JobDescriptor())
                .RuleFor(s => s.ErrorOccurred, f => f.Random.Bool())
                .RuleFor(s => s.EventNumberVersion, _ => eventNumber)
                .RuleFor(s => s.ProcessedOn, f => f.Date.PastOffset())
                .Generate();
        }

        [Fact]
        public async Task Read_state_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            var streams = new[] { "stream-1", "stream-2", "stream-3" };

            var expectedResult = new Dictionary<string, ulong>
            {
                { streams[0], 123 },
                { streams[1], 234 },
                { streams[2], 345 }
            };

            await GetArangoClient()
                .CreateDocumentsAsync(
                    GetCollectionName<ProjectionState>(),
                    new[]
                    {
                        GenerateProjectionState(123, streams[0]),
                        GenerateProjectionState(222, streams[1]),
                        GenerateProjectionState(234, streams[1]),
                        GenerateProjectionState(123, streams[2]),
                        GenerateProjectionState(344, streams[2]),
                        GenerateProjectionState(345, streams[2])
                    });

            IDictionary<string, ulong> state = await repo.GetLatestProjectedEventIdsAsync();

            state.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public async Task Create_state_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            ProjectionState state = new Faker<ProjectionState>()
                .RuleFor(s => s.StreamName, f => f.System.FileName())
                .RuleFor(s => s.ErrorMessage, f => f.Name.JobType())
                .RuleFor(s => s.EventId, f => f.Random.AlphaNumeric(36))
                .RuleFor(s => s.EventName, f => f.Name.FirstName())
                .RuleFor(s => s.StackTraceMessage, f => f.Name.JobDescriptor())
                .RuleFor(s => s.ErrorOccurred, f => f.Random.Bool())
                .RuleFor(s => s.EventNumberVersion, f => f.Random.Long())
                .RuleFor(s => s.ProcessedOn, f => f.Date.PastOffset())
                .Generate();

            await repo.SaveProjectionStateAsync(state);

            MultiApiResponse<ProjectionState> results = await GetArangoClient()
                .ExecuteQueryAsync<ProjectionState>(
                    $"FOR p IN {GetCollectionName<ProjectionState>()} FILTER p.EventId == \"{state.EventId}\" RETURN p");

            results.QueryResult.Single().Should().BeEquivalentTo(state);
        }
    }
}
