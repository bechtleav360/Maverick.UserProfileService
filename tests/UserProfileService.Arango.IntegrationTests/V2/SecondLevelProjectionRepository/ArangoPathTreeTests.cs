using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Common.Models;
using UserProfileService.Arango.IntegrationTests.V2.Abstractions;
using UserProfileService.Arango.IntegrationTests.V2.Fixtures;
using UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Abstractions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using Xunit;
using Xunit.Abstractions;
using static UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Data.PathTreeTestsData;

namespace UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository
{
    [Collection(nameof(SecondLevelProjectionCollection))]
    public class ArangoPathTreeTests : ArangoSecondLevelRepoTestBase
    {
        private readonly SecondLevelProjectionFixture _fixture;
        private readonly ITestOutputHelper _testOutputHelper;

        public ArangoPathTreeTests(SecondLevelProjectionFixture fixture, ITestOutputHelper testOutputHelper)
        {
            _fixture = fixture;
            _testOutputHelper = testOutputHelper;
        }
        
        public static IEnumerable<object[]> NumberConditionToDelete =>
            new List<object[]>
            {
                new object[] { 1},
                new object[] { 2},
                new object[] { 3 },
                new object[] { 4 },
                new object[] { 5 },
            };

        [Theory]
        [MemberData(nameof(NumberConditionToDelete))]
        public async Task deleting_range_conditions_from_edge_should_work(int numberOfDeletingRangeConditions)
        {
            await _fixture.InitializeAsync();
            IPathTreeRepository pathTreeRepository = await _fixture.GetSecondLevelPathReeRepository();
            string relatedId = OneNodeWithGroupAndRangeConditionsFirstCase.RelatedObjectId;
            string objectId = OneNodeWithGroupAndRangeConditionsFirstCase.ObjectIdFirstGroup;
            int rangeConditionsNumber = OneNodeWithGroupAndRangeConditionsFirstCase.RangeConditionsForEdge;

            
            
            SecondLevelProjectionProfileEdgeData testPathEdge = await GetEdgeFromPathTreeAsync(
                OneNodeWithGroupAndRangeConditionsFirstCase.RelatedObjectId,
                OneNodeWithGroupAndRangeConditionsFirstCase.ObjectIdFirstGroup);

            _testOutputHelper.WriteLine($"rangeConditionsNumber:  {rangeConditionsNumber}, testPathEdge: {testPathEdge.Conditions.Count}.");
            
            testPathEdge.Conditions.Count.Should().Be(rangeConditionsNumber);
            
            IEnumerable<RangeCondition> rangeConditionToDelete = testPathEdge.Conditions.Take(numberOfDeletingRangeConditions);

            IEnumerable<RangeCondition> rangeConditionsShouldStay =
                testPathEdge.Conditions.Skip(numberOfDeletingRangeConditions);

            SecondLevelProjectionProfileEdgeData testEdgeWithConditionsStayed=
                await pathTreeRepository.RemoveRangeConditionsFromPathTreeEdgeAsync(
                    relatedId,
                    objectId,
                    relatedId,
                    rangeConditionToDelete.ToList());

            Assert.True(testEdgeWithConditionsStayed.Conditions.Count == rangeConditionsNumber - numberOfDeletingRangeConditions);
            
            rangeConditionsShouldStay.ToList().Should()
                                     .BeEquivalentTo(
                                         testEdgeWithConditionsStayed.Conditions,
                                         opt => opt.RespectingRuntimeTypes());
            
        }
        
        [Theory]
        [InlineData(null)]
        [InlineData("empty")]
        public async Task argument_exception_with_empty_and_null_range_condition_should_work(string conditions)
        {
            IPathTreeRepository pathTreeRepository = await _fixture.GetSecondLevelPathReeRepository();
            string relatedId = OneNodeWithGroupAndRangeConditionsSecondCase.RelatedObjectId;
            string objectId = OneNodeWithGroupAndRangeConditionsSecondCase.ObjectIdFirstGroup;

            await Assert.ThrowsAsync<ArgumentException>( () => pathTreeRepository.RemoveRangeConditionsFromPathTreeEdgeAsync(
                                                             relatedId,
                                                             objectId,
                                                             relatedId,
                                                             conditions == null? null: new List<RangeCondition>()));

        }
        
        [Fact]
        public async Task no_deleting_without_range_condition_should_work()
        {
            IPathTreeRepository pathTreeRepository = await _fixture.GetSecondLevelPathReeRepository();
            string relatedId = OneNodeWithGroupAndRangeConditionsSecondCase.RelatedObjectId;
            string objectId = OneNodeWithGroupAndRangeConditionsSecondCase.ObjectIdFirstGroup;

            await Assert.ThrowsAsync<ArgumentException>( () => pathTreeRepository.RemoveRangeConditionsFromPathTreeEdgeAsync(
                                                             relatedId,
                                                             objectId,
                                                             relatedId,
                                                             new List<RangeCondition>()));

        }

        
        [Fact]
        public async Task deleting_real_user_results_in_deleting_edge()
        {
            ISecondLevelProjectionRepository secondLevelRepository = await _fixture.GetSecondLevelRepository();
            string relatedId = OneUserWithGroupAndRangeConditionThirdCase.UserProfile;
            string objectId = OneUserWithGroupAndRangeConditionThirdCase.GroupProfile;

            SecondLevelProjectionProfileEdgeData testPathEdge = await GetEdgeFromPathTreeAsync(
                relatedId,
                objectId);

            testPathEdge.Should().NotBeNull();
            testPathEdge.Conditions.Count.Should().Be(1);
            
            await secondLevelRepository.RemoveMemberOfAsync(
                relatedId,
                relatedId,
                ContainerType.Group,
                objectId,
                testPathEdge.Conditions);

            SecondLevelProjectionProfileEdgeData testEdgeAgain = await GetEdgeFromPathTreeAsync(
                relatedId,
                objectId);

            testEdgeAgain.Should().BeNull();
        }
        
        [Fact]
        public async Task tying_to_delete_user_with_empty_range_conditions()
        {
            ISecondLevelProjectionRepository secondLevelRepository = await _fixture.GetSecondLevelRepository();
            string relatedId = OneUserWithGroupAndRangeConditionFouthCase.UserProfile;
            string objectId = OneUserWithGroupAndRangeConditionFouthCase.GroupProfile;

            SecondLevelProjectionProfileEdgeData testPathEdge = await GetEdgeFromPathTreeAsync(
                relatedId,
                objectId);

            testPathEdge.Should().NotBeNull();
            testPathEdge.Conditions.Count.Should().Be(1);
            
            await secondLevelRepository.RemoveMemberOfAsync(
                relatedId,
                relatedId,
                ContainerType.Group,
                objectId,
                new List<RangeCondition>());

            SecondLevelProjectionProfileEdgeData testEdgeAgain = await GetEdgeFromPathTreeAsync(
                relatedId,
                objectId);

            testEdgeAgain.Should().NotBeNull();
        }
        
        [Fact]
        public async Task tying_to_delete_user_with_null_range_conditions()
        {
            ISecondLevelProjectionRepository secondLevelRepository = await _fixture.GetSecondLevelRepository();
            string relatedId = OneUserWithGroupAndRangeConditionFifthCase.UserProfile;
            string objectId = OneUserWithGroupAndRangeConditionFifthCase.GroupProfile;

            SecondLevelProjectionProfileEdgeData testPathEdge = await GetEdgeFromPathTreeAsync(
                relatedId,
                objectId);

            testPathEdge.Should().NotBeNull();
            testPathEdge.Conditions.Count.Should().Be(1);
            
            await secondLevelRepository.RemoveMemberOfAsync(
                relatedId,
                relatedId,
                ContainerType.Group,
                objectId);

            SecondLevelProjectionProfileEdgeData testEdgeAgain = await GetEdgeFromPathTreeAsync(
                relatedId,
                objectId);

            testEdgeAgain.Should().NotBeNull();
        }
    }
}