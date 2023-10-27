using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Maverick.Client.ArangoDb.Public;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Common.Models;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Adapter.Arango.V2.Extensions;
using UserProfileService.Arango.IntegrationTests.V2.Abstractions;
using UserProfileService.Arango.IntegrationTests.V2.Fixtures;
using UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Data;
using UserProfileService.Common.Tests.Utilities.Extensions;
using UserProfileService.Common.Tests.Utilities.MockDataBuilder;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using Xunit;
using AVMember = Maverick.UserProfileService.Models.Models.Member;
using Member = Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models.Member;
using RangeCondition = Maverick.UserProfileService.AggregateEvents.Common.Models.RangeCondition;

namespace UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository
{
    [Collection(nameof(SecondLevelProjectionCollection))]
    public class GroupTests : ArangoSecondLevelRepoTestBase
    {
        private readonly SecondLevelProjectionFixture _fixture;

        public GroupTests(SecondLevelProjectionFixture fixture)
        {
            _fixture = fixture;
        }

        private async Task<IList<Member>> GetMembersOfContainerProfileAsync(
            string profileId
        )
        {
            if (profileId == null)
            {
                throw new ArgumentNullException(nameof(profileId));
            }

            string collectionName = GetCollectionName<IProfileEntityModel>();

            var query = @$" FOR x in {
                collectionName
            }
                    FILTER x.Id == ""{
                        profileId
                    }""
                    RETURN x.Members";

            MultiApiResponse<IList<AVMember>> result = await GetArangoClient()
                .ExecuteQueryAsync<IList<AVMember>>(query);

            if (result is { Error: false, QueryResult: { } } && result.QueryResult.Any())
            {
                return result.QueryResult.FirstOrDefault()
                    ?.Select(
                        r => GetMapper()
                            .Map<Member>(r))
                    .ToList();
            }

            return new List<Member>();
        }

        private static List<CombinedPathData> GetDefault(string relatedProfile, params string[] relatives)
        {
            return relatives
                .Select(
                    id =>
                        new CombinedPathData
                        {
                            Edge = new SecondLevelProjectionProfileEdgeData
                            {
                                Conditions = new List<RangeCondition>
                                {
                                    new RangeCondition()
                                },
                                RelatedProfileId = relatedProfile
                            },
                            Vertex = new SecondLevelProjectionProfileVertexData
                            {
                                Tags = new List<TagAssignment>(),
                                RelatedProfileId = relatedProfile,
                                ObjectId = id
                            }
                        })
                .ToList();
        }

        [Fact]
        public async Task Create_group_should_work()
        {
            ISecondLevelProjectionRepository repo = await _fixture.GetSecondLevelRepository();
            SecondLevelProjectionGroup group = MockDataGenerator.GenerateSecondLevelProjectionGroup().Single();

            await repo.CreateProfileAsync(group);

            var dbGroup = await GetDocumentObjectAsync<IProfileEntityModel>(group.Id);
            var convertedGroup = GetMapper().Map<SecondLevelProjectionGroup>(dbGroup);
            convertedGroup.Should().BeEquivalentTo(group);
        }

        [Fact]
        public async Task AddMember_should_work()
        {
            ISecondLevelProjectionRepository repo = await _fixture.GetSecondLevelRepository();
            SecondLevelProjectionGroup group = MockDataGenerator.GenerateSecondLevelProjectionGroup().Single();
            SecondLevelProjectionUser user = MockDataGenerator.GenerateSecondLevelProjectionUser().Single();

            await repo.CreateProfileAsync(group);
            await repo.CreateProfileAsync(user);

            var userAsMember = GetMapper().Map<Member>(user);
            await repo.AddMemberAsync(group.Id, ContainerType.Group, userAsMember);

            var dbEntry = await GetDocumentObjectAsync<GroupEntityModel>(group.Id);
            dbEntry.HasChildren.Should().BeTrue();

            IList<Member> result = await GetMembersOfContainerProfileAsync(group.Id);
            result.Should().ContainEquivalentOf(userAsMember, options => options.Excluding(s => s.Conditions));
        }

        [Fact]
        // User added twice but should have one assignment
        public async Task AddMember_twice_should_work()
        {
            ISecondLevelProjectionRepository repo = await _fixture.GetSecondLevelRepository();
            SecondLevelProjectionGroup group = MockDataGenerator.GenerateSecondLevelProjectionGroup().Single();
            SecondLevelProjectionUser user = MockDataGenerator.GenerateSecondLevelProjectionUser().Single();

            await repo.CreateProfileAsync(group);
            await repo.CreateProfileAsync(user);

            var userAsMember = GetMapper().Map<Member>(user);

            userAsMember.Conditions = new List<RangeCondition>
            {
                new RangeCondition
                {
                    End = null,
                    Start = DateTime.Today.ToUniversalTime()
                }
            };

            await repo.AddMemberAsync(group.Id, ContainerType.Group, userAsMember);
            await repo.AddMemberAsync(group.Id, ContainerType.Group, userAsMember);

            var dbEntry = await GetDocumentObjectAsync<GroupEntityModel>(group.Id);
            dbEntry.HasChildren.Should().BeTrue();

            IList<Member> result = await GetMembersOfContainerProfileAsync(group.Id);
            result.Should().ContainEquivalentOf(userAsMember);
        }

        [Fact]
        public async Task AddMember_twice_with_different_condition_should_work()
        {
            ISecondLevelProjectionRepository repo = await _fixture.GetSecondLevelRepository();
            SecondLevelProjectionGroup group = MockDataGenerator.GenerateSecondLevelProjectionGroup().Single();
            SecondLevelProjectionUser user = MockDataGenerator.GenerateSecondLevelProjectionUser().Single();

            await repo.CreateProfileAsync(group);
            await repo.CreateProfileAsync(user);

            var userAsMember = GetMapper().Map<Member>(user);

            userAsMember.Conditions = new List<RangeCondition>
            {
                new RangeCondition
                {
                    End = null,
                    Start = DateTime.Today.ToUniversalTime()
                },
                new RangeCondition
                {
                    Start = DateTime.UnixEpoch,
                    End = DateTime.UnixEpoch.AddYears(10)
                }
            };

            await repo.AddMemberAsync(group.Id, ContainerType.Group, userAsMember);

            userAsMember.Conditions = new List<RangeCondition>
            {
                new RangeCondition
                {
                    End = null,
                    Start = null
                }
            };

            await repo.AddMemberAsync(group.Id, ContainerType.Group, userAsMember);

            userAsMember.Conditions = new List<RangeCondition>
            {
                new RangeCondition
                {
                    End = null,
                    Start = DateTime.Today.ToUniversalTime()
                },
                new RangeCondition
                {
                    End = null,
                    Start = null
                },
                new RangeCondition
                {
                    Start = DateTime.UnixEpoch,
                    End = DateTime.UnixEpoch.AddYears(10)
                }
            };

            var entityModel = await GetDocumentObjectAsync<GroupEntityModel>(group.Id);
            entityModel.Members.Should().NotBeNull().And.ContainEquivalentOf(userAsMember);

            entityModel.Members.FirstOrDefault(x => x.Id == user.Id)
                .Should()
                .NotBeNull()
                .And.Match<AVMember>(x => x.IsActive);
        }

        [Fact]
        public async Task Add_member_to_existing_member_set_in_group_should_work()
        {
            // arrange
            ISecondLevelProjectionRepository repo = await _fixture.GetSecondLevelRepository();

            Member newMember =
                await GetDocumentObjectAsync<Member, IProfileEntityModel>(
                    GroupTestData.AddMemberToExistingSet.NewChildUserId);

            Member oldMember =
                await GetDocumentObjectAsync<Member, IProfileEntityModel>(
                    GroupTestData.AddMemberToExistingSet.ExistingChildGroupId);

            // act
            await repo.AddMemberAsync(
                GroupTestData.AddMemberToExistingSet.ParentGroupId,
                ContainerType.Group,
                newMember.NormalizeRangeConditions());

            // assert
            var modifiedParent =
                await GetDocumentObjectAsync<GroupEntityModel>(GroupTestData.AddMemberToExistingSet.ParentGroupId);

            modifiedParent
                .Members
                .Should()
                .BeEquivalentTo(
                    new List<Member>
                    {
                        oldMember.NormalizeRangeConditions(),
                        newMember.NormalizeRangeConditions()
                    });
        }

        [Fact]
        public async Task Add_memberOf_to_existing_memberOf_set_in_group_should_work()
        {
            // arrange
            ISecondLevelProjectionRepository repo = await _fixture.GetSecondLevelRepository();
            string pathTreeVertexCollection = GetCollectionName<SecondLevelProjectionProfileVertexData>();

            string pathTreeEdgeCollection =
                GetEdgeCollection<SecondLevelProjectionProfileVertexData, SecondLevelProjectionProfileVertexData>();

            Member oldParent =
                await GetDocumentObjectAsync<Member, IProfileEntityModel>(
                    GroupTestData.AddMemberOfEntryToExistingSet.OldParentGroupId);

            SecondLevelProjectionGroup newParentGroup =
                await GetDocumentObjectAsync<SecondLevelProjectionGroup, IProfileEntityModel>(
                    GroupTestData.AddMemberOfEntryToExistingSet.NewParentGroupId);

            var newParent = GetMapper().Map<Member>(newParentGroup);

            // act
            await repo.AddMemberOfAsync(
                GroupTestData.AddMemberOfEntryToExistingSet.ChildGroupId,
                GroupTestData.AddMemberOfEntryToExistingSet.ChildGroupId,
                new List<RangeCondition>
                {
                    new RangeCondition()
                },
                newParentGroup);

            // assert
            var modifiedChild =
                await GetDocumentObjectAsync<GroupEntityModel>(
                    GroupTestData.AddMemberOfEntryToExistingSet.ChildGroupId);

            modifiedChild
                .MemberOf
                .Should()
                .BeEquivalentTo(
                    new List<Member>
                    {
                        oldParent.NormalizeRangeConditions(),
                        newParent.NormalizeRangeConditions()
                    });

            string startVertexInternalId = GetArangoId<SecondLevelProjectionProfileVertexData>(
                string.Concat(
                    GroupTestData.AddMemberOfEntryToExistingSet.ChildGroupId,
                    "-",
                    GroupTestData.AddMemberOfEntryToExistingSet.ChildGroupId));

            MultiApiResponse<CombinedPathData> queryResponse = await GetArangoClient()
                .ExecuteQueryAsync<CombinedPathData>(
                    $@"
                      WITH {
                          pathTreeVertexCollection
                      }
                      FOR v,e,p IN 1..100 OUTBOUND
                        ""{
                            startVertexInternalId
                        }"" {
                            pathTreeEdgeCollection
                        }
                        RETURN {{ Vertex: v, Edge: e }}");

            if (queryResponse.Error || queryResponse.Responses.Any(r => r.Error))
            {
                throw new Exception();
            }

            queryResponse
                .QueryResult
                .Should()
                .BeEquivalentTo(
                    GetDefault(
                        GroupTestData.AddMemberOfEntryToExistingSet.ChildGroupId,
                        oldParent.Id,
                        newParent.Id));
        }

        [Fact]
        public async Task TryUpdateMemberAsync_should_work()
        {
            ISecondLevelProjectionRepository repo = await _fixture.GetSecondLevelRepository();
            SecondLevelProjectionGroup group = MockDataGenerator.GenerateSecondLevelProjectionGroup().Single();
            SecondLevelProjectionGroup secondGroup = MockDataGenerator.GenerateSecondLevelProjectionGroup().Single();
            await repo.CreateProfileAsync(group);
            await repo.CreateProfileAsync(secondGroup);

            Member groupAsMember = GetMapper().MapContainerTypeToAggregateMember(secondGroup);
            await repo.AddMemberAsync(group?.Id, ContainerType.Group, groupAsMember);

            var newProperties = new Dictionary<string, object>
            {
                { "Name", "Emmanuel" },
                { "DisplayName", "Herr Emmanuel" }
            };

            await repo.TryUpdateMemberAsync(group?.Id, groupAsMember.Id, newProperties);

            IList<Member> dbGroup = await GetMembersOfContainerProfileAsync(group?.Id);
            Member result = dbGroup.FirstOrDefault(d => d.Name == "Emmanuel" && d.DisplayName == "Herr Emmanuel");
            Assert.NotNull(result);
        }

        [Fact]
        public async Task RemoveMember_should_work()
        {
            ISecondLevelProjectionRepository repo = await _fixture.GetSecondLevelRepository();

            SecondLevelProjectionUser user = MockDataGenerator.GenerateSecondLevelProjectionUser(minimumMemberOf: 1)
                .Single();

            SecondLevelProjectionGroup group = MockDataGenerator.GenerateSecondLevelProjectionGroup().Single();
            var userAsMember = GetMapper().Map<Member>(user);

            await repo.CreateProfileAsync(group);
            await repo.CreateProfileAsync(user);

            await repo.AddMemberAsync(group.Id, ContainerType.Group, userAsMember);

            await repo.RemoveMemberAsync(group.Id, ContainerType.Group, user.Id);
            ISecondLevelProjectionProfile userDb = await repo.GetProfileAsync(user.Id);
            Member memberDeleted = userDb.MemberOf?.FirstOrDefault(u => u.Id == group.Id);
            memberDeleted.Should().BeNull();

            var dbEntry = await GetDocumentObjectAsync<GroupEntityModel>(group.Id);
            dbEntry.HasChildren.Should().BeFalse();
        }

        [Fact]
        public async Task Add_function_as_security_assignment_to_existing_set_should_work()
        {
            // arrange
            ISecondLevelProjectionRepository repo = await _fixture.GetSecondLevelRepository();

            SecondLevelProjectionFunction newFunction =
                await GetDocumentObjectAsync<SecondLevelProjectionFunction, FunctionObjectEntityModel>(
                    GroupTestData.AddSecurityAssignmentsInExistingSet.NewFunctionId);

            LinkedFunctionObject oldFunction =
                await GetDocumentObjectAsync<LinkedFunctionObject, FunctionObjectEntityModel>(
                    GroupTestData.AddSecurityAssignmentsInExistingSet.ExistingFunctionId);

            var newLinkedObject = GetMapper().Map<ILinkedObject>(newFunction);

            newLinkedObject.Name = $"{newFunction.Organization?.Name} {newFunction.Role?.Name}";

            // act
            await repo.AddMemberOfAsync(
                GroupTestData.AddSecurityAssignmentsInExistingSet.GroupId,
                GroupTestData.AddSecurityAssignmentsInExistingSet.GroupId,
                new List<RangeCondition>
                {
                    new RangeCondition()
                },
                newFunction);

            // assert
            var modifiedGroup =
                await GetDocumentObjectAsync<GroupEntityModel>(
                    GroupTestData.AddSecurityAssignmentsInExistingSet.GroupId);

            modifiedGroup
                .SecurityAssignments
                .Should()
                .BeEquivalentTo(
                    new List<ILinkedObject>
                    {
                        oldFunction.NormalizeRangeConditions().SetIsActive(false),
                        newLinkedObject.NormalizeRangeConditions()
                    });
        }

        private class CombinedPathData
        {
            // ReSharper disable UnusedAutoPropertyAccessor.Local
            // FluentAssertion will use the getter.
            public SecondLevelProjectionProfileVertexData Vertex { get; set; }

            public SecondLevelProjectionProfileEdgeData Edge { get; set; }
            // ReSharper restore UnusedAutoPropertyAccessor.Local
        }
    }
}
