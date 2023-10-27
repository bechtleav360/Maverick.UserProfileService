using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Maverick.Client.ArangoDb.Public;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Arango.IntegrationTests.V2.Abstractions;
using UserProfileService.Arango.IntegrationTests.V2.Fixtures;
using UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Data;
using UserProfileService.Projection.Abstractions;
using Xunit;

namespace UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository
{
    [Collection(nameof(SecondLevelProjectionCollection))]
    public class RecalculateAssignmentsTests : ArangoSecondLevelRepoTestBase
    {
        private readonly SecondLevelProjectionFixture _fixture;

        public RecalculateAssignmentsTests(SecondLevelProjectionFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Recalculate_user_due_own_active_group_assignment_change()
        {
            const string idOfUserToBeModified = RecalculateProfilesTestData.RecalculateUserRelatedToGroupWithSubgroup
                .UserId;

            const string assignedGroupId = RecalculateProfilesTestData.RecalculateUserRelatedToGroupWithSubgroup
                .SecondGroupId;

            const string rootGroupId = RecalculateProfilesTestData.RecalculateUserRelatedToGroupWithSubgroup
                .RootGroupId;

            const string alreadyActiveGroupOfUserId = RecalculateProfilesTestData
                .RecalculateUserRelatedToGroupWithSubgroup
                .GroupId;

            ISecondLevelProjectionRepository repo = await _fixture.GetSecondLevelRepository();
            string profileCollection = GetCollectionName<IProfileEntityModel>();

            await repo.RecalculateAssignmentsAsync(
                new ObjectIdent(
                    idOfUserToBeModified,
                    ObjectType.User),
                idOfUserToBeModified,
                assignedGroupId,
                Maverick.UserProfileService.AggregateEvents.Common.Enums.ObjectType.Group,
                true);

            MultiApiResponse<UserEntityModel> modifiedProfileResponse = await GetArangoClient()
                .ExecuteQueryAsync<UserEntityModel>(
                    $"FOR p IN {profileCollection} FILTER p.Id==\"{idOfUserToBeModified}\" RETURN p");

            if (modifiedProfileResponse.Error)
            {
                throw new Exception();
            }

            UserEntityModel modifiedGroup = modifiedProfileResponse.QueryResult.Single();

            modifiedGroup.Paths
                .Should()
                .BeEquivalentTo(
                    new List<string>
                    {
                        string.Join('/', rootGroupId, alreadyActiveGroupOfUserId, idOfUserToBeModified),
                        string.Join('/', assignedGroupId, idOfUserToBeModified),
                        idOfUserToBeModified
                    });

            modifiedGroup.Tags
                .Should()
                .BeEquivalentTo(
                    new List<CalculatedTag>
                    {
                        // user's own tag
                        new CalculatedTag
                        {
                            Id = RecalculateProfilesTestData.RecalculateUserRelatedToGroupWithSubgroup
                                .DeveloperTagId,
                            IsInherited = false
                        },
                        // tag of group whose assignment has been activated
                        new CalculatedTag
                        {
                            Id = RecalculateProfilesTestData.RecalculateUserRelatedToGroupWithSubgroup
                                .DreamLifeTagId,
                            IsInherited = true
                        },
                        // tag of group that was assigned previously
                        new CalculatedTag
                        {
                            Id = RecalculateProfilesTestData.RecalculateUserRelatedToGroupWithSubgroup
                                .AwesomeEntitiesTagId,
                            IsInherited = true
                        }
                    },
                    o => o.Excluding(t => t.Name));
        }

        [Fact]
        public async Task Recalculate_user_due_parent_group_assignment_change()
        {
            const string idOfUserToBeModified = RecalculateProfilesTestData
                .RecalculateRelatedUserInGroupWithModifiedGroupAssignment
                .UserId;

            const string assignedGroupId = RecalculateProfilesTestData
                .RecalculateRelatedUserInGroupWithModifiedGroupAssignment
                .GroupOfUserId;

            const string parentGroupId = RecalculateProfilesTestData
                .RecalculateRelatedUserInGroupWithModifiedGroupAssignment
                .RootGroupId;

            ISecondLevelProjectionRepository repo = await _fixture.GetSecondLevelRepository();
            string profileCollection = GetCollectionName<IProfileEntityModel>();

            await repo.RecalculateAssignmentsAsync(
                new ObjectIdent(
                    idOfUserToBeModified,
                    ObjectType.User),
                assignedGroupId,
                parentGroupId,
                Maverick.UserProfileService.AggregateEvents.Common.Enums.ObjectType.Group,
                true);

            MultiApiResponse<UserEntityModel> modifiedGroupResponse = await GetArangoClient()
                .ExecuteQueryAsync<UserEntityModel>(
                    $"FOR p IN {profileCollection} FILTER p.Id==\"{idOfUserToBeModified}\" RETURN p");

            if (modifiedGroupResponse.Error)
            {
                throw new Exception();
            }

            UserEntityModel modifiedGroup = modifiedGroupResponse.QueryResult.Single();

            modifiedGroup.Paths
                .Should()
                .BeEquivalentTo(
                    new List<string>
                    {
                        string.Join('/', parentGroupId, assignedGroupId, idOfUserToBeModified),
                        idOfUserToBeModified
                    });

            modifiedGroup.Tags
                .Should()
                .BeEquivalentTo(
                    new List<CalculatedTag>
                    {
                        new CalculatedTag
                        {
                            Id = RecalculateProfilesTestData.RecalculateRelatedUserInGroupWithModifiedGroupAssignment
                                .RootGroupTagId,
                            IsInherited = true
                        },
                        new CalculatedTag
                        {
                            Id = RecalculateProfilesTestData.RecalculateRelatedUserInGroupWithModifiedGroupAssignment
                                .GroupInGroupTagId,
                            IsInherited = true
                        }
                    },
                    o => o.Excluding(t => t.Name));
        }

        [Fact]
        public async Task Recalculate_user_due_of_parent_group_unassigned()
        {
            ISecondLevelProjectionRepository repo = await _fixture.GetSecondLevelRepository();

            const string idOfGroupToBeModified = RecalculateProfilesTestData.RecalculateUnassignedGroupOfGroup
                .GroupId;

            string profileCollection = GetCollectionName<IProfileEntityModel>();

            await repo.RecalculateAssignmentsAsync(
                new ObjectIdent(idOfGroupToBeModified, ObjectType.Group),
                idOfGroupToBeModified,
                RecalculateProfilesTestData.RecalculateUnassignedGroupOfGroup
                    .ParentGroupId,
                Maverick.UserProfileService.AggregateEvents.Common.Enums.ObjectType.Group,
                false);

            MultiApiResponse<GroupEntityModel> modifiedGroupResponse = await GetArangoClient()
                .ExecuteQueryAsync<GroupEntityModel>(
                    $"FOR p IN {profileCollection} FILTER p.Id==\"{idOfGroupToBeModified}\" RETURN p");

            if (modifiedGroupResponse.Error)
            {
                throw new Exception();
            }

            GroupEntityModel modifiedGroup = modifiedGroupResponse.QueryResult.Single();

            modifiedGroup.Paths
                .Should()
                .BeEquivalentTo(
                    new List<string>
                    {
                        idOfGroupToBeModified
                    });

            modifiedGroup.Tags
                .Should()
                .BeEquivalentTo(
                    new List<CalculatedTag>
                    {
                        new CalculatedTag
                        {
                            Id = RecalculateProfilesTestData.RecalculateUnassignedGroupOfGroup.ChildTagId,
                            IsInherited = false
                        }
                    },
                    o => o.Excluding(t => t.Name).Excluding(t => t.Type));
        }
    }
}
