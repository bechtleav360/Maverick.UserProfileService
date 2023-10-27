using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Maverick.Client.ArangoDb.Public;
using Maverick.Client.ArangoDb.Public.Extensions;
using Maverick.UserProfileService.Models.EnumModels;
using UserProfileService.Adapter.Arango.V2.Helpers;
using UserProfileService.Arango.IntegrationTests.V2.Abstractions;
using UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Data;
using UserProfileService.Arango.IntegrationTests.V2.Fixtures;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using Xunit;

namespace UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository
{
    [Collection(nameof(FirstLevelProjectionCollection))]
    public class TemporaryAssignmentTests : ArangoFirstLevelRepoTestBase
    {
        private readonly FirstLevelProjectionFixture _fixture;
        private readonly FirstLevelProjectionReadFixture _readFixture;

        public TemporaryAssignmentTests(
            FirstLevelProjectionFixture fixture,
            FirstLevelProjectionReadFixture readFixture)
        {
            _fixture = fixture;
            _readFixture = readFixture;
        }

        private static async Task UpdateTemporaryAssignmentsAsync(
            IFirstLevelProjectionRepository repo,
            List<FirstLevelProjectionTemporaryAssignment> data,
            IDatabaseTransaction transaction)
        {
            try
            {
                await repo.UpdateTemporaryAssignmentStatesAsync(data, transaction);
                await repo.CommitTransactionAsync(transaction);
            }
            catch (Exception)
            {
                await repo.AbortTransactionAsync(transaction);

                throw;
            }
        }

        [Fact]
        public async Task Get_temporary_assignments_should_work()
        {
            // arrange
            List<FirstLevelProjectionTemporaryAssignment> expected = TemporaryAssignmentsTestData
                .ExistingTemporaryAssignments
                .Assignments
                .Where(a => a.Id.StartsWith("out"))
                .ToList();

            IFirstLevelProjectionRepository repo = await _readFixture.GetFirstLevelRepository();

            // act
            IList<FirstLevelProjectionTemporaryAssignment> found = await repo.GetTemporaryAssignmentsAsync();

            // assert
            found.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task Save_null_temporary_assignments_should_fail()
        {
            // arrange
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            // act & assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () =>
                    repo.UpdateTemporaryAssignmentStatesAsync(null, new ArangoTransaction(), CancellationToken.None));
        }

        [Fact]
        public async Task Save_temporary_assignments_including_new_one_should_fail()
        {
            // arrange
            var expected = new List<FirstLevelProjectionTemporaryAssignment>
            {
                new FirstLevelProjectionTemporaryAssignment
                {
                    Id = "write-tests-new-temp-assignment",
                    Start = DateTime.UtcNow.AddYears(1),
                    End = null,
                    ProfileId = "group-maggots",
                    ProfileType = ObjectType.Group,
                    TargetId = "group-ppl-like-gods",
                    TargetType = ObjectType.Group,
                    State = TemporaryAssignmentState.NotProcessed,
                    LastModified = DateTime.UtcNow
                },
                TemporaryAssignmentsTestData.WriteTestData.Assignments[0]
            };

            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();
            IDatabaseTransaction transaction = await repo.StartTransactionAsync();

            // act & assert
            await Assert.ThrowsAsync<StatesMismatchException>(
                () => UpdateTemporaryAssignmentsAsync(repo, expected, transaction));
        }

        [Fact]
        public async Task Save_temporary_assignments_should_work()
        {
            // arrange
            List<FirstLevelProjectionTemporaryAssignment> expected =
                TemporaryAssignmentsTestData.WriteTestData.Assignments;

            IArangoDbClient client = await _fixture.GetClientAsync();
            string collectionName = GetCollectionName<FirstLevelProjectionTemporaryAssignment>();
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();
            IDatabaseTransaction transaction = await repo.StartTransactionAsync();

            // act
            await UpdateTemporaryAssignmentsAsync(repo, expected, transaction);

            // assert
            MultiApiResponse<FirstLevelProjectionTemporaryAssignment> changedEntity =
                await client.ExecuteQueryAsync<FirstLevelProjectionTemporaryAssignment>(
                    $"FOR x IN {collectionName} FILTER x.Id == \"{expected[0].Id}\" RETURN x");

            changedEntity.QueryResult.Should()
                .BeEquivalentTo(
                    expected,
                    o =>
                        o.Excluding(a => a.State)
                            .Excluding(a => a.LastModified));

            changedEntity.QueryResult.Single().LastModified.Should().BeAfter(DateTime.UtcNow.AddMinutes(-2));
            changedEntity.QueryResult.Single().State.Should().Be(TemporaryAssignmentState.NotProcessed);
        }
    }
}
