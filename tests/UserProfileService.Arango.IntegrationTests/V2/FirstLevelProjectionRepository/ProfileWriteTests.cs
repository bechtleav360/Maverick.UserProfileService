using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Maverick.Client.ArangoDb.Public;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Adapter.Arango.V2.EntityModels.FirstLevel;
using UserProfileService.Arango.IntegrationTests.V2.Abstractions;
using UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Data;
using UserProfileService.Arango.IntegrationTests.V2.Fixtures;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Helpers;
using UserProfileService.Projection.Abstractions.Models;
using Xunit;
using ObjectType = Maverick.UserProfileService.Models.EnumModels.ObjectType;

namespace UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository
{
    [Collection(nameof(FirstLevelProjectionCollection))]
    public class ProfileWriteTests : ArangoFirstLevelRepoTestBase
    {
        private readonly FirstLevelProjectionFixture _fixture;

        public ProfileWriteTests(FirstLevelProjectionFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Create_user_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();
            FirstLevelProjectionUser user = MockDataGenerator.GenerateUser();

            await repo.CreateProfileAsync(user);

            var dbUser = await GetDocumentObjectAsync<FirstLevelProjectionUser>(user.Id);
            dbUser.Should().BeEquivalentTo(user);
        }

        [Fact]
        public async Task Create_group_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();
            FirstLevelProjectionGroup group = MockDataGenerator.GenerateGroup();

            await repo.CreateProfileAsync(group);

            var dbGroup = await GetDocumentObjectAsync<FirstLevelProjectionGroup>(group.Id);
            dbGroup.Should().BeEquivalentTo(group);
        }

        [Fact]
        public async Task Create_organization_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();
            FirstLevelProjectionOrganization organization = MockDataGenerator.GenerateOrganization();

            await repo.CreateProfileAsync(organization);

            var dbOrganization = await GetDocumentObjectAsync<FirstLevelProjectionOrganization>(organization.Id);
            dbOrganization.Should().BeEquivalentTo(organization);
        }

        [Fact]
        public async Task Delete_user_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            await repo.DeleteProfileAsync(ProfilesWriteTestData.DeleteUser.UserId);

            var dbUser =
                await GetDocumentObjectAsync<FirstLevelProjectionUser>(ProfilesWriteTestData.DeleteUser.UserId, false);

            Assert.Null(dbUser);
            //TODO check edges & clientSettings
        }

        [Fact]
        public async Task Delete_group_including_temporary_assignments_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();
            string collectionName = GetCollectionName<FirstLevelProjectionTemporaryAssignment>();

            await repo.DeleteProfileAsync(ProfilesWriteTestData.DeleteSecondGroup.GroupId);

            var dbGroup =
                await GetDocumentObjectAsync<FirstLevelProjectionGroup>(
                    ProfilesWriteTestData.DeleteSecondGroup.GroupId,
                    false);

            Assert.Null(dbGroup);

            IReadOnlyList<TemporaryAssignmentTestEntity> storedAssignmentsToRoot =
                await GetDocumentObjectsAsync<TemporaryAssignmentTestEntity>(
                    "FOR a IN @@collection"
                    + "FILTER a.ProfileId==@pId AND a.TargetId==@tId"
                    + "RETURN a",
                    true,
                    "pId",
                    ProfilesWriteTestData.DeleteSecondGroup.GroupId,
                    "tId",
                    ProfilesWriteTestData.DeleteSecondGroup.RootGroupId,
                    "@collection",
                    collectionName);

            Assert.Empty(storedAssignmentsToRoot);

            IReadOnlyList<TemporaryAssignmentTestEntity> storedAssignments =
                await GetDocumentObjectsAsync<TemporaryAssignmentTestEntity>(
                    "FOR a IN @@collection"
                    + "FILTER a.ProfileId==@pId AND a.TargetId==@tId"
                    + "RETURN a",
                    true,
                    "pId",
                    ProfilesWriteTestData.DeleteSecondGroup.UserId,
                    "tId",
                    ProfilesWriteTestData.DeleteSecondGroup.GroupId,
                    "@collection",
                    collectionName);

            Assert.Empty(storedAssignments);
        }

        [Fact]
        public async Task Delete_group_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            await repo.DeleteProfileAsync(ProfilesWriteTestData.DeleteGroup.TargetId);

            var dbGroup =
                await GetDocumentObjectAsync<FirstLevelProjectionGroup>(
                    ProfilesWriteTestData.DeleteGroup.TargetId,
                    false);

            Assert.Null(dbGroup);
            //TODO check edges & clientSettings
        }

        [Fact]
        public async Task Delete_organization_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            await repo.DeleteProfileAsync(ProfilesWriteTestData.DeleteOrganization.TargetId);

            var dbOrganization =
                await GetDocumentObjectAsync<FirstLevelProjectionOrganization>(
                    ProfilesWriteTestData.DeleteOrganization.TargetId,
                    false);

            Assert.Null(dbOrganization);
            //TODO check edges & clientSettings
        }

        [Fact]
        public async Task Delete_not_existing_profile_should_throw()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            await Assert.ThrowsAsync<InstanceNotFoundException>(
                () => repo.DeleteProfileAsync("this-user-does-not-exist"));
        }

        [Fact]
        public async Task Update_user_should_work()
        {
            string id = ProfilesWriteTestData.UpdateUser.UserId;
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            var previousUser = await GetDocumentObjectAsync<FirstLevelProjectionUser>(id);
            FirstLevelProjectionUser newUser = MockDataGenerator.GenerateUser(id);

            await repo.UpdateProfileAsync(newUser);

            var dbUser = await GetDocumentObjectAsync<FirstLevelProjectionUser>(id);
            dbUser.Should().BeEquivalentTo(newUser);
            dbUser.Should().NotBeEquivalentTo(previousUser);
        }

        [Fact]
        public async Task Update_group_should_work()
        {
            string id = ProfilesWriteTestData.UpdateGroup.GroupId;
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            var previousGroup = await GetDocumentObjectAsync<FirstLevelProjectionGroup>(id);
            FirstLevelProjectionGroup newGroup = MockDataGenerator.GenerateGroup(id);

            await repo.UpdateProfileAsync(newGroup);

            var dbGroup = await GetDocumentObjectAsync<FirstLevelProjectionGroup>(id);
            dbGroup.Should().BeEquivalentTo(newGroup);
            dbGroup.Should().NotBeEquivalentTo(previousGroup);
        }

        [Fact]
        public async Task Update_organization_should_work()
        {
            string id = ProfilesWriteTestData.UpdateOrganization.OrganizationId;
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            var previousOrganization = await GetDocumentObjectAsync<FirstLevelProjectionOrganization>(id);
            FirstLevelProjectionOrganization newOrganization = MockDataGenerator.GenerateOrganization(id);

            await repo.UpdateProfileAsync(newOrganization);

            var dbUser = await GetDocumentObjectAsync<FirstLevelProjectionOrganization>(id);
            dbUser.Should().BeEquivalentTo(newOrganization);
            dbUser.Should().NotBeEquivalentTo(previousOrganization);
        }

        [Fact]
        public async Task Update_not_existing_profile_should_throw()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();
            IFirstLevelProjectionProfile newProfile = MockDataGenerator.GenerateUser("this-profile-does-not-exist");

            await Assert.ThrowsAsync<InstanceNotFoundException>(() => repo.UpdateProfileAsync(newProfile));
        }

        [Fact]
        public async Task Set_client_settings_of_user_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();
            string id = ProfilesWriteTestData.AddClientSettingsToUser.UserId;
            var key = "test";
            var value = "{\"test\":2}";

            await repo.SetClientSettingsAsync(id, value, key);

            Assert.Equal(
                1,
                await GetRelationCountAsync<IFirstLevelProjectionProfile>(
                    GetEdgeCollection<IFirstLevelProjectionProfile, FirstLevelProjectionsClientSetting>(),
                    id));

            string clientSettingsCollection = GetCollectionName<FirstLevelProjectionsClientSetting>();

            Assert.Equal(
                1,
                await GetResultCountAsync(
                    $@"
                    FOR cs IN {
                        clientSettingsCollection
                    } 
                    FILTER cs.{
                        nameof(FirstLevelProjectionClientSettingsBasic.Key)
                    } == '{
                        key
                    }' 
                    AND cs.{
                        nameof(FirstLevelProjectionClientSettingsBasic.ProfileId)
                    } == '{
                        id
                    }'
                    AND cs.{
                        nameof(FirstLevelProjectionClientSettingsBasic.Value)
                    } == '{
                        value
                    }'
                    RETURN cs"));
        }

        [Fact]
        public async Task Set_client_settings_of_group_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();
            string id = ProfilesWriteTestData.AddClientSettingsToGroup.GroupId;
            var key = "test";
            var value = "{\"test\":2}";

            await repo.SetClientSettingsAsync(id, value, key);

            Assert.Equal(
                1,
                await GetRelationCountAsync<IFirstLevelProjectionProfile>(
                    GetEdgeCollection<IFirstLevelProjectionProfile, FirstLevelProjectionClientSettingsBasic>(),
                    id));

            string clientSettingsCollection = GetCollectionName<FirstLevelProjectionClientSettingsBasic>();

            Assert.Equal(
                1,
                await GetResultCountAsync(
                    $@"
                    FOR cs IN {
                        clientSettingsCollection
                    } 
                    FILTER cs.{
                        nameof(FirstLevelProjectionClientSettingsBasic.Key)
                    } == '{
                        key
                    }' 
                    AND cs.{
                        nameof(FirstLevelProjectionClientSettingsBasic.ProfileId)
                    } == '{
                        id
                    }'
                    AND cs.{
                        nameof(FirstLevelProjectionClientSettingsBasic.Value)
                    } == '{
                        value
                    }'
                    RETURN cs"));
        }

        [Fact]
        public async Task Set_client_settings_of_unknown_profile_should_throw()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();
            var id = "this-profile-does-not-exist";

            await Assert.ThrowsAsync<InstanceNotFoundException>(
                () => repo.SetClientSettingsAsync(id, "{\"test:\":0}", "test"));

            string clientSettingsCollection = GetCollectionName<FirstLevelProjectionClientSettingsBasic>();

            Assert.Equal(
                0,
                await GetResultCountAsync(
                    $@"
                    FOR cs IN {
                        clientSettingsCollection
                    } 
                    FILTER cs.{
                        nameof(FirstLevelProjectionClientSettingsBasic.ProfileId)
                    } == '{
                        id
                    }'
                    RETURN cs"));
        }

        [Fact]
        public async Task Update_client_settings_of_user_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();
            string id = ProfilesWriteTestData.UpdateClientSettingsOfUser.UserId;
            string key = ProfilesWriteTestData.UpdateClientSettingsOfUser.Key;
            var value = "{\"value\":2}";

            await repo.SetClientSettingsAsync(id, value, key);

            Assert.Equal(
                1,
                await GetRelationCountAsync<IFirstLevelProjectionProfile>(
                    GetEdgeCollection<IFirstLevelProjectionProfile, FirstLevelProjectionClientSettingsBasic>(),
                    id));

            string clientSettingsCollection = GetCollectionName<FirstLevelProjectionClientSettingsBasic>();

            // check if the correct client setting is set
            Assert.Equal(
                1,
                await GetResultCountAsync(
                    $@"
                    FOR cs IN {
                        clientSettingsCollection
                    } 
                    FILTER cs.{
                        nameof(FirstLevelProjectionClientSettingsBasic.Key)
                    } == '{
                        key
                    }' 
                    AND cs.{
                        nameof(FirstLevelProjectionClientSettingsBasic.ProfileId)
                    } == '{
                        id
                    }'
                    AND cs.{
                        nameof(FirstLevelProjectionClientSettingsBasic.Value)
                    } == '{
                        value
                    }'
                    RETURN cs"));

            // check if old client setting was deleted
            Assert.Equal(
                1,
                await GetResultCountAsync(
                    $@"
                    FOR cs IN {
                        clientSettingsCollection
                    } 
                    FILTER cs.{
                        nameof(FirstLevelProjectionClientSettingsBasic.Key)
                    } == '{
                        key
                    }' 
                    AND cs.{
                        nameof(FirstLevelProjectionClientSettingsBasic.ProfileId)
                    } == '{
                        id
                    }'
                    RETURN cs"));
        }

        [Fact]
        public async Task Update_client_settings_of_group_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();
            string id = ProfilesWriteTestData.UpdateClientSettingsOfGroup.GroupId;
            string key = ProfilesWriteTestData.UpdateClientSettingsOfGroup.Key;
            var value = "{\"value\":2}";

            await repo.SetClientSettingsAsync(id, value, key);

            Assert.Equal(
                1,
                await GetRelationCountAsync<IFirstLevelProjectionProfile>(
                    GetEdgeCollection<IFirstLevelProjectionProfile, FirstLevelProjectionClientSettingsBasic>(),
                    id));

            string clientSettingsCollection = GetCollectionName<FirstLevelProjectionClientSettingsBasic>();

            // check if the correct client setting is set
            Assert.Equal(
                1,
                await GetResultCountAsync(
                    $@"
                    FOR cs IN {
                        clientSettingsCollection
                    } 
                    FILTER cs.{
                        nameof(FirstLevelProjectionClientSettingsBasic.Key)
                    } == '{
                        key
                    }' 
                    AND cs.{
                        nameof(FirstLevelProjectionClientSettingsBasic.ProfileId)
                    } == '{
                        id
                    }'
                    AND cs.{
                        nameof(FirstLevelProjectionClientSettingsBasic.Value)
                    } == '{
                        value
                    }'
                    RETURN cs"));

            // check if old client setting was deleted
            Assert.Equal(
                1,
                await GetResultCountAsync(
                    $@"
                    FOR cs IN {
                        clientSettingsCollection
                    } 
                    FILTER cs.{
                        nameof(FirstLevelProjectionClientSettingsBasic.Key)
                    } == '{
                        key
                    }' 
                    AND cs.{
                        nameof(FirstLevelProjectionClientSettingsBasic.ProfileId)
                    } == '{
                        id
                    }'
                    RETURN cs"));
        }

        [Fact]
        public async Task Unset_client_settings_of_user_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();
            string id = ProfilesWriteTestData.UnsetClientSettingsOfUser.UserId;
            string key = ProfilesWriteTestData.UnsetClientSettingsOfUser.Key;

            await repo.UnsetClientSettingsAsync(id, key);

            Assert.Equal(
                0,
                await GetRelationCountAsync<IFirstLevelProjectionProfile>(
                    GetEdgeCollection<IFirstLevelProjectionProfile, FirstLevelProjectionClientSettingsBasic>(),
                    id));

            string clientSettingsCollection = GetCollectionName<FirstLevelProjectionClientSettingsBasic>();

            Assert.Equal(
                0,
                await GetResultCountAsync(
                    $@"
                    FOR cs IN {
                        clientSettingsCollection
                    } 
                    FILTER cs.{
                        nameof(FirstLevelProjectionClientSettingsBasic.Key)
                    } == '{
                        key
                    }' 
                    AND cs.{
                        nameof(FirstLevelProjectionClientSettingsBasic.ProfileId)
                    } == '{
                        id
                    }'
                    RETURN cs"));
        }

        [Fact]
        public async Task Unset_client_settings_of_group_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();
            string id = ProfilesWriteTestData.UnsetClientSettingsOfGroup.GroupId;
            string key = ProfilesWriteTestData.UnsetClientSettingsOfGroup.Key;

            await repo.UnsetClientSettingsAsync(id, key);

            Assert.Equal(
                0,
                await GetRelationCountAsync<IFirstLevelProjectionProfile>(
                    GetEdgeCollection<IFirstLevelProjectionProfile, FirstLevelProjectionClientSettingsBasic>(),
                    id));

            string clientSettingsCollection = GetCollectionName<FirstLevelProjectionClientSettingsBasic>();

            Assert.Equal(
                0,
                await GetResultCountAsync(
                    $@"
                    FOR cs IN {
                        clientSettingsCollection
                    } 
                    FILTER cs.{
                        nameof(FirstLevelProjectionClientSettingsBasic.Key)
                    } == '{
                        key
                    }' 
                    AND cs.{
                        nameof(FirstLevelProjectionClientSettingsBasic.ProfileId)
                    } == '{
                        id
                    }'
                    RETURN cs"));
        }

        [Fact]
        public async Task Unset_client_settings_of_unknown_profile_should_throw()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();
            var id = "this-profile-does-not-exist";

            await Assert.ThrowsAsync<InstanceNotFoundException>(() => repo.UnsetClientSettingsAsync(id, "test"));

            string clientSettingsCollection = GetCollectionName<FirstLevelProjectionClientSettingsBasic>();

            Assert.Equal(
                0,
                await GetResultCountAsync(
                    $@"
                    FOR cs IN {
                        clientSettingsCollection
                    } 
                    FILTER cs.{
                        nameof(FirstLevelProjectionClientSettingsBasic.ProfileId)
                    } == '{
                        id
                    }'
                    RETURN cs"));
        }

        [Fact]
        public async Task Add_tag_to_user_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            await repo.AddTagToProfileAsync(
                new FirstLevelProjectionTagAssignment
                {
                    IsInheritable = true,
                    TagId = ProfilesWriteTestData.AddTagToUser.TagId
                },
                ProfilesWriteTestData.AddTagToUser.UserId);

            var dbUser =
                await GetDocumentObjectAsync<IFirstLevelProjectionProfile>(ProfilesWriteTestData.AddTagToUser.UserId);

            Assert.Equal(
                1,
                await GetRelationCountAsync<IFirstLevelProjectionProfile>(
                    GetEdgeCollection<FirstLevelProjectionUser, FirstLevelProjectionTag>(),
                    dbUser.Id));
        }

        [Fact]
        public async Task Add_tag_to_group_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            await repo.AddTagToProfileAsync(
                new FirstLevelProjectionTagAssignment
                {
                    IsInheritable = true,
                    TagId = ProfilesWriteTestData.AddTagToGroup.TagId
                },
                ProfilesWriteTestData.AddTagToGroup.GroupId);

            var dbUser =
                await GetDocumentObjectAsync<IFirstLevelProjectionProfile>(ProfilesWriteTestData.AddTagToGroup.GroupId);

            Assert.Equal(
                1,
                await GetRelationCountAsync<IFirstLevelProjectionProfile>(
                    GetEdgeCollection<FirstLevelProjectionGroup, FirstLevelProjectionTag>(),
                    dbUser.Id));
        }

        [Fact]
        public async Task Add_tag_to_organization_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            await repo.AddTagToProfileAsync(
                new FirstLevelProjectionTagAssignment
                {
                    IsInheritable = true,
                    TagId = ProfilesWriteTestData.AddTagToOrganization.TagId
                },
                ProfilesWriteTestData.AddTagToOrganization.OrganizationId);

            var dbUser =
                await GetDocumentObjectAsync<IFirstLevelProjectionProfile>(
                    ProfilesWriteTestData.AddTagToOrganization.OrganizationId);

            Assert.Equal(
                1,
                await GetRelationCountAsync<IFirstLevelProjectionProfile>(
                    GetEdgeCollection<FirstLevelProjectionOrganization, FirstLevelProjectionTag>(),
                    dbUser.Id));
        }

        [Fact]
        public async Task Add_tag_to_not_existing_profile_should_throw()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            await Assert.ThrowsAsync<InstanceNotFoundException>(
                () => repo.AddTagToProfileAsync(
                    new FirstLevelProjectionTagAssignment
                    {
                        IsInheritable = true,
                        TagId = ProfilesWriteTestData.AddTagToNotExistingProfile.TagId
                    },
                    "this-profile-does-not-exist"));

            // Ensure nothing was created in the db
            var dbTag = await GetDocumentObjectAsync<FirstLevelProjectionTag>(
                ProfilesWriteTestData.AddTagToNotExistingProfile.TagId);

            Assert.Equal(
                0,
                await GetRelationCountAsync<FirstLevelProjectionTag>(
                    GetEdgeCollection<IFirstLevelProjectionProfile, FirstLevelProjectionTag>(),
                    dbTag.Id));
        }

        [Fact]
        public async Task Add_not_existing_tag_to_user_should_throw()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            await Assert.ThrowsAsync<InstanceNotFoundException>(
                () => repo.AddTagToProfileAsync(
                    new FirstLevelProjectionTagAssignment
                    {
                        IsInheritable = true,
                        TagId = "this-tag-does-not-exist"
                    },
                    ProfilesWriteTestData.AddNotExistingTagToUser.UserId));

            // Ensure nothing was created in the db
            var dbProfile =
                await GetDocumentObjectAsync<IFirstLevelProjectionProfile>(
                    ProfilesWriteTestData.AddNotExistingTagToUser.UserId);

            Assert.NotNull(dbProfile);

            Assert.Equal(
                0,
                await GetRelationCountAsync<FirstLevelProjectionUser>(
                    GetEdgeCollection<FirstLevelProjectionUser, FirstLevelProjectionTag>(),
                    dbProfile.Id));
        }

        [Fact]
        public async Task Add_not_existing_tag_to_group_should_throw()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            await Assert.ThrowsAsync<InstanceNotFoundException>(
                () => repo.AddTagToProfileAsync(
                    new FirstLevelProjectionTagAssignment
                    {
                        IsInheritable = true,
                        TagId = "this-tag-does-not-exist"
                    },
                    ProfilesWriteTestData.AddNotExistingTagToGroup.GroupId));

            // Ensure nothing was created in the db
            var dbProfile =
                await GetDocumentObjectAsync<IFirstLevelProjectionProfile>(
                    ProfilesWriteTestData.AddNotExistingTagToGroup.GroupId);

            Assert.NotNull(dbProfile);

            Assert.Equal(
                0,
                await GetRelationCountAsync<FirstLevelProjectionGroup>(
                    GetEdgeCollection<FirstLevelProjectionGroup, FirstLevelProjectionTag>(),
                    dbProfile.Id));
        }

        [Fact]
        public async Task Add_not_existing_tag_to_organization_should_throw()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            await Assert.ThrowsAsync<InstanceNotFoundException>(
                () => repo.AddTagToProfileAsync(
                    new FirstLevelProjectionTagAssignment
                    {
                        IsInheritable = true,
                        TagId = "this-tag-does-not-exist"
                    },
                    ProfilesWriteTestData.AddNotExistingTagToOrganization.OrganizationId));

            // Ensure nothing was created in the db
            var dbProfile =
                await GetDocumentObjectAsync<IFirstLevelProjectionProfile>(
                    ProfilesWriteTestData.AddNotExistingTagToOrganization.OrganizationId);

            Assert.NotNull(dbProfile);

            Assert.Equal(
                0,
                await GetRelationCountAsync<FirstLevelProjectionOrganization>(
                    GetEdgeCollection<FirstLevelProjectionOrganization, FirstLevelProjectionTag>(),
                    dbProfile.Id));
        }

        [Fact]
        public async Task Remove_tag_from_user_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            await repo.RemoveTagFromProfileAsync(
                ProfilesWriteTestData.RemoveTagFromUser.TagId,
                ProfilesWriteTestData.RemoveTagFromUser.ProfileId);

            var dbProfile =
                await GetDocumentObjectAsync<IFirstLevelProjectionProfile>(
                    ProfilesWriteTestData.RemoveTagFromUser.ProfileId);

            Assert.Equal(
                0,
                await GetRelationCountAsync<IFirstLevelProjectionProfile>(
                    GetEdgeCollection<IFirstLevelProjectionProfile, FirstLevelProjectionTag>(),
                    dbProfile.Id));
        }

        [Fact]
        public async Task Remove_tag_from_group_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            await repo.RemoveTagFromProfileAsync(
                ProfilesWriteTestData.RemoveTagFromGroup.TagId,
                ProfilesWriteTestData.RemoveTagFromGroup.ProfileId);

            var dbProfile =
                await GetDocumentObjectAsync<IFirstLevelProjectionProfile>(
                    ProfilesWriteTestData.RemoveTagFromGroup.ProfileId);

            Assert.Equal(
                0,
                await GetRelationCountAsync<IFirstLevelProjectionProfile>(
                    GetEdgeCollection<IFirstLevelProjectionProfile, FirstLevelProjectionTag>(),
                    dbProfile.Id));
        }

        [Fact]
        public async Task Remove_tag_from_organization_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            await repo.RemoveTagFromProfileAsync(
                ProfilesWriteTestData.RemoveTagFromOrganization.TagId,
                ProfilesWriteTestData.RemoveTagFromOrganization.ProfileId);

            var dbProfile =
                await GetDocumentObjectAsync<IFirstLevelProjectionProfile>(
                    ProfilesWriteTestData.RemoveTagFromOrganization.ProfileId);

            Assert.Equal(
                0,
                await GetRelationCountAsync<IFirstLevelProjectionProfile>(
                    GetEdgeCollection<IFirstLevelProjectionProfile, FirstLevelProjectionTag>(),
                    dbProfile.Id));
        }

        [Fact]
        public async Task Remove_tag_from_not_existing_profile_should_throw()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            await Assert.ThrowsAsync<InstanceNotFoundException>(
                () => repo.RemoveTagFromProfileAsync(
                    ProfilesWriteTestData.RemoveTagFromNotExistingProfile.TagId,
                    "this-profile-does-not-exist"));

            // Ensure nothing was created in the db
            var dbTag = await GetDocumentObjectAsync<FirstLevelProjectionTag>(
                ProfilesWriteTestData.RemoveTagFromNotExistingProfile.TagId);

            Assert.Equal(
                0,
                await GetRelationCountAsync<FirstLevelProjectionTag>(
                    GetEdgeCollection<IFirstLevelProjectionProfile, FirstLevelProjectionTag>(),
                    dbTag.Id));
        }

        [Fact]
        public async Task Remove_not_existing_tag_from_user_should_throw()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            await Assert.ThrowsAsync<InstanceNotFoundException>(
                () => repo.RemoveTagFromProfileAsync(
                    "this-tag-does-not-exist",
                    ProfilesWriteTestData.RemoveNotExistingTagFromUser.UserId));

            // Ensure nothing was created in the db
            var dbRole =
                await GetDocumentObjectAsync<IFirstLevelProjectionProfile>(
                    ProfilesWriteTestData.RemoveNotExistingTagFromUser.UserId);

            Assert.NotNull(dbRole);

            Assert.Equal(
                0,
                await GetRelationCountAsync<IFirstLevelProjectionProfile>(
                    GetEdgeCollection<IFirstLevelProjectionProfile, FirstLevelProjectionTag>(),
                    dbRole.Id));
        }

        [Fact]
        public async Task Remove_not_existing_tag_from_group_should_throw()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            await Assert.ThrowsAsync<InstanceNotFoundException>(
                () => repo.RemoveTagFromProfileAsync(
                    "this-tag-does-not-exist",
                    ProfilesWriteTestData.RemoveNotExistingTagFromGroup.GroupId));

            // Ensure nothing was created in the db
            var dbProfile =
                await GetDocumentObjectAsync<IFirstLevelProjectionProfile>(
                    ProfilesWriteTestData.RemoveNotExistingTagFromGroup.GroupId);

            Assert.NotNull(dbProfile);

            Assert.Equal(
                0,
                await GetRelationCountAsync<IFirstLevelProjectionProfile>(
                    GetEdgeCollection<IFirstLevelProjectionProfile, FirstLevelProjectionTag>(),
                    dbProfile.Id));
        }

        [Fact]
        public async Task Remove_not_existing_tag_from_organization_should_throw()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            await Assert.ThrowsAsync<InstanceNotFoundException>(
                () => repo.RemoveTagFromProfileAsync(
                    "this-tag-does-not-exist",
                    ProfilesWriteTestData.RemoveNotExistingTagFromOrganization.OrganizationId));

            // Ensure nothing was created in the db
            var dbProfile =
                await GetDocumentObjectAsync<IFirstLevelProjectionProfile>(
                    ProfilesWriteTestData.RemoveNotExistingTagFromOrganization.OrganizationId);

            Assert.NotNull(dbProfile);

            Assert.Equal(
                0,
                await GetRelationCountAsync<IFirstLevelProjectionProfile>(
                    GetEdgeCollection<IFirstLevelProjectionProfile, FirstLevelProjectionTag>(),
                    dbProfile.Id));
        }
        /*
        [Fact]
        public async Task Remove_not_existing_tag_assignment_from_role_should_throw()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            await Assert.ThrowsAsync<Exception>(
                () => repo.RemoveTagFromRoleAsync(
                    RoleWriteTestData.RemoveNotAssignedTag.TagId,
                    RoleWriteTestData.RemoveNotAssignedTag.RoleId));

            // Ensure nothing was created in the db
            var dbRole =
                await GetDocumentObjectAsync<FirstLevelProjectionRole>(
                    RoleWriteTestData.RemoveFromNotExistingTag.RoleId);
            Assert.NotNull(dbRole);
            Assert.Equal(
                0,
                await GetRelationCountAsync<FirstLevelProjectionRole>(
                    GetEdgeCollection<FirstLevelProjectionRole, FirstLevelProjectionTag>(),
                    dbRole.Id));
        }
        */

        [Fact]
        public async Task Assign_user_to_group_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            string profileId = ProfilesWriteTestData.AssignUserToGroup.UserId;
            string parentId = ProfilesWriteTestData.AssignUserToGroup.GroupId;
            string edgeName = GetEdgeCollection<IFirstLevelProjectionProfile, FirstLevelProjectionGroup>();

            var conditions = new List<RangeCondition>
            {
                new RangeCondition()
            };

            await repo.CreateProfileAssignmentAsync(
                parentId,
                ContainerType.Group,
                profileId,
                conditions);

            MultiApiResponse<FirstLevelProjectionAssignment> edgeResponse = await GetArangoClient()
                .ExecuteQueryAsync<FirstLevelProjectionAssignment>(
                    $"FOR e IN {edgeName} FILTER e._to LIKE \"%/{parentId}\" AND e._from LIKE \"%/{profileId}\" RETURN e");

            if (edgeResponse.Error)
            {
                throw new Exception();
            }

            IReadOnlyList<FirstLevelProjectionAssignment> edges = edgeResponse.QueryResult;

            Assert.Single(edges);
            edges.Single().Conditions.Should().BeEquivalentTo(conditions);
        }

        [Fact]
        public async Task Assign_user_to_group_temporary_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            string profileId = ProfilesWriteTestData.AssignProfileToGroupTemporarily.UserId;
            string parentId = ProfilesWriteTestData.AssignProfileToGroupTemporarily.GroupId;
            string edgeName = GetEdgeCollection<IFirstLevelProjectionProfile, FirstLevelProjectionGroup>();
            string assignmentCollectionName = GetCollectionName<FirstLevelProjectionTemporaryAssignment>();

            var conditions = new List<RangeCondition>
            {
                new RangeCondition
                {
                    Start = DateTime.Today.ToUniversalTime().AddMonths(-6),
                    End = DateTime.Today.ToUniversalTime().AddYears(1)
                },
                new RangeCondition
                {
                    Start = new DateTime(
                        2050,
                        1,
                        1,
                        8,
                        0,
                        0,
                        DateTimeKind.Utc),
                    End = new DateTime(
                        2070,
                        1,
                        1,
                        8,
                        0,
                        0,
                        DateTimeKind.Utc)
                },
                new RangeCondition
                {
                    Start = new DateTime(
                        2000,
                        12,
                        26,
                        5,
                        0,
                        0,
                        DateTimeKind.Utc),
                    End = new DateTime(
                        2020,
                        1,
                        1,
                        10,
                        30,
                        0,
                        DateTimeKind.Utc)
                }
            };

            List<FirstLevelProjectionTemporaryAssignment> expected = conditions
                .Take(2)
                .Select(
                    (c, i) =>
                        new FirstLevelProjectionTemporaryAssignment
                        {
                            Start = c.Start,
                            End = c.End,
                            ProfileId = profileId,
                            ProfileType = ObjectType.User,
                            TargetId = parentId,
                            TargetType = ObjectType.Group,
                            State = i == 1
                                ? TemporaryAssignmentState
                                    .NotProcessed
                                : TemporaryAssignmentState
                                    .ActiveWithExpiration
                        })
                .ToList();

            await repo.CreateProfileAssignmentAsync(
                parentId,
                ContainerType.Group,
                profileId,
                conditions);

            MultiApiResponse<FirstLevelProjectionAssignment> edgeResponse = await GetArangoClient()
                .ExecuteQueryAsync<FirstLevelProjectionAssignment>(
                    $"FOR e IN {edgeName} FILTER e._to LIKE \"%/{parentId}\" AND e._from LIKE \"%/{profileId}\" RETURN e");

            if (edgeResponse.Error)
            {
                throw new Exception();
            }

            IReadOnlyList<FirstLevelProjectionAssignment> edges = edgeResponse.QueryResult;

            Assert.Single(edges);
            edges.Single().Conditions.Should().BeEquivalentTo(conditions);

            MultiApiResponse<FirstLevelProjectionTemporaryAssignment> assignmentResponse = await GetArangoClient()
                .ExecuteQueryAsync<FirstLevelProjectionTemporaryAssignment>(
                    $"FOR a IN {assignmentCollectionName} FILTER a.ProfileId == \"{profileId}\" RETURN a");

            if (assignmentResponse.Error)
            {
                throw new Exception();
            }

            assignmentResponse.QueryResult
                .Should()
                .BeEquivalentTo(
                    expected,
                    o =>
                        o.Excluding(a => a.Id)
                            .Excluding(a => a.LastModified));
        }

        [Fact]
        public async Task Assign_group_to_group_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            string profileId = ProfilesWriteTestData.AssignGroupToGroup.MemberId;
            string parentId = ProfilesWriteTestData.AssignGroupToGroup.ParentId;
            string edgeName = GetEdgeCollection<IFirstLevelProjectionProfile, FirstLevelProjectionGroup>();

            var conditions = new List<RangeCondition>
            {
                new RangeCondition()
            };

            await repo.CreateProfileAssignmentAsync(
                parentId,
                ContainerType.Group,
                profileId,
                conditions);

            MultiApiResponse<FirstLevelProjectionAssignment> edgeResponse = await GetArangoClient()
                .ExecuteQueryAsync<FirstLevelProjectionAssignment>(
                    $"FOR e IN {edgeName} FILTER e._to LIKE \"%/{parentId}\" AND e._from LIKE \"%/{profileId}\" RETURN e");

            if (edgeResponse.Error)
            {
                throw new Exception();
            }

            IReadOnlyList<FirstLevelProjectionAssignment> edges = edgeResponse.QueryResult;

            Assert.Single(edges);
            edges.Single().Conditions.Should().BeEquivalentTo(conditions);
        }

        [Fact]
        public async Task Assign_not_existing_profile_to_group_should_throw()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            var profileId = "this-profile-does-not-exist";
            string parentId = ProfilesWriteTestData.AssignNotExistingProfileToGroup.ParentId;
            string edgeName = GetEdgeCollection<IFirstLevelProjectionProfile, FirstLevelProjectionGroup>();

            var conditions = new List<RangeCondition>
            {
                new RangeCondition()
            };

            await Assert.ThrowsAsync<InstanceNotFoundException>(
                () => repo.CreateProfileAssignmentAsync(
                    parentId,
                    ContainerType.Group,
                    profileId,
                    conditions));

            MultiApiResponse<FirstLevelProjectionAssignment> edgeResponse = await GetArangoClient()
                .ExecuteQueryAsync<FirstLevelProjectionAssignment>(
                    $"FOR e IN {edgeName} FILTER e._to LIKE \"%/{parentId}\" AND e._from LIKE \"%/{profileId}\" RETURN e");

            if (edgeResponse.Error)
            {
                throw new Exception();
            }

            Assert.Empty(edgeResponse.QueryResult);
        }

        [Fact]
        public async Task Assign_organization_to_organization_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            string profileId = ProfilesWriteTestData.AssignOrgUnitToOrgUnit.MemberId;
            string parentId = ProfilesWriteTestData.AssignOrgUnitToOrgUnit.ParentId;
            string edgeName = GetEdgeCollection<IFirstLevelProjectionProfile, FirstLevelProjectionOrganization>();

            var conditions = new List<RangeCondition>
            {
                new RangeCondition()
            };

            await repo.CreateProfileAssignmentAsync(
                parentId,
                ContainerType.Organization,
                profileId,
                conditions);

            MultiApiResponse<FirstLevelProjectionAssignment> edgeResponse = await GetArangoClient()
                .ExecuteQueryAsync<FirstLevelProjectionAssignment>(
                    $"FOR e IN {edgeName} FILTER e._to LIKE \"%/{parentId}\" AND e._from LIKE \"%/{profileId}\" RETURN e");

            if (edgeResponse.Error)
            {
                throw new Exception();
            }

            IReadOnlyList<FirstLevelProjectionAssignment> edges = edgeResponse.QueryResult;

            Assert.Single(edges);
            edges.Single().Conditions.Should().BeEquivalentTo(conditions);
        }

        [Fact]
        public async Task Assign_user_to_not_existing_profile_should_throw()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            var parentId = "this-profile-does-not-exist";
            string profileId = ProfilesWriteTestData.AssignUserToNotExistingProfile.ParentId;
            string edgeName = GetEdgeCollection<IFirstLevelProjectionProfile, FirstLevelProjectionGroup>();

            var conditions = new List<RangeCondition>
            {
                new RangeCondition()
            };

            await Assert.ThrowsAsync<InstanceNotFoundException>(
                () => repo.CreateProfileAssignmentAsync(
                    parentId,
                    ContainerType.Group,
                    profileId,
                    conditions));

            MultiApiResponse<FirstLevelProjectionAssignment> edgeResponse = await GetArangoClient()
                .ExecuteQueryAsync<FirstLevelProjectionAssignment>(
                    $"FOR e IN {edgeName} FILTER e._to LIKE \"%/{parentId}\" AND e._from LIKE \"%/{profileId}\" RETURN e");

            if (edgeResponse.Error)
            {
                throw new Exception();
            }

            Assert.Empty(edgeResponse.QueryResult);
        }

        [Fact]
        public async Task Assign_group_to_not_existing_profile_should_throw()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            var parentId = "this-profile-does-not-exist";
            string profileId = ProfilesWriteTestData.AssignGroupToNotExistingProfile.ParentId;
            string edgeName = GetEdgeCollection<IFirstLevelProjectionProfile, FirstLevelProjectionGroup>();

            var conditions = new List<RangeCondition>
            {
                new RangeCondition()
            };

            await Assert.ThrowsAsync<InstanceNotFoundException>(
                () => repo.CreateProfileAssignmentAsync(
                    parentId,
                    ContainerType.Group,
                    profileId,
                    conditions));

            MultiApiResponse<FirstLevelProjectionAssignment> edgeResponse = await GetArangoClient()
                .ExecuteQueryAsync<FirstLevelProjectionAssignment>(
                    $"FOR e IN {edgeName} FILTER e._to LIKE \"%/{parentId}\" AND e._from LIKE \"%/{profileId}\" RETURN e");

            if (edgeResponse.Error)
            {
                throw new Exception();
            }

            Assert.Empty(edgeResponse.QueryResult);
        }

        [Fact]
        public async Task Assign_organization_to_not_existing_profile_should_throw()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            var parentId = "this-profile-does-not-exist";
            string profileId = ProfilesWriteTestData.AssignOrganizationToNotExistingProfile.ParentId;
            string edgeName = GetEdgeCollection<IFirstLevelProjectionProfile, FirstLevelProjectionGroup>();

            var conditions = new List<RangeCondition>
            {
                new RangeCondition()
            };

            await Assert.ThrowsAsync<InstanceNotFoundException>(
                () => repo.CreateProfileAssignmentAsync(
                    parentId,
                    ContainerType.Group,
                    profileId,
                    conditions));

            MultiApiResponse<FirstLevelProjectionAssignment> edgeResponse = await GetArangoClient()
                .ExecuteQueryAsync<FirstLevelProjectionAssignment>(
                    $"FOR e IN {edgeName} FILTER e._to LIKE \"%/{parentId}\" AND e._from LIKE \"%/{profileId}\" RETURN e");

            if (edgeResponse.Error)
            {
                throw new Exception();
            }

            Assert.Empty(edgeResponse.QueryResult);
        }

        [Fact]
        public async Task Assign_user_to_role_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            string profileId = ProfilesWriteTestData.AssignUserToRole.UserId;
            string parentId = ProfilesWriteTestData.AssignUserToRole.RoleId;
            string edgeName = GetEdgeCollection<IFirstLevelProjectionProfile, FirstLevelProjectionRole>();

            var conditions = new List<RangeCondition>
            {
                new RangeCondition()
            };

            await repo.CreateProfileAssignmentAsync(
                parentId,
                ContainerType.Role,
                profileId,
                conditions);

            MultiApiResponse<FirstLevelProjectionAssignment> edgeResponse = await GetArangoClient()
                .ExecuteQueryAsync<FirstLevelProjectionAssignment>(
                    $"FOR e IN {edgeName} FILTER e._to LIKE \"%/{parentId}\" AND e._from LIKE \"%/{profileId}\" RETURN e");

            if (edgeResponse.Error)
            {
                throw new Exception();
            }

            IReadOnlyList<FirstLevelProjectionAssignment> edges = edgeResponse.QueryResult;

            Assert.Single(edges);
            edges.Single().Conditions.Should().BeEquivalentTo(conditions);
        }

        [Fact]
        public async Task Assign_group_to_role_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            string profileId = ProfilesWriteTestData.AssignGroupToRole.GroupId;
            string parentId = ProfilesWriteTestData.AssignGroupToRole.RoleId;
            string edgeName = GetEdgeCollection<IFirstLevelProjectionProfile, FirstLevelProjectionRole>();

            var conditions = new List<RangeCondition>
            {
                new RangeCondition()
            };

            await repo.CreateProfileAssignmentAsync(
                parentId,
                ContainerType.Role,
                profileId,
                conditions);

            MultiApiResponse<FirstLevelProjectionAssignment> edgeResponse = await GetArangoClient()
                .ExecuteQueryAsync<FirstLevelProjectionAssignment>(
                    $"FOR e IN {edgeName} FILTER e._to LIKE \"%/{parentId}\" AND e._from LIKE \"%/{profileId}\" RETURN e");

            if (edgeResponse.Error)
            {
                throw new Exception();
            }

            IReadOnlyList<FirstLevelProjectionAssignment> edges = edgeResponse.QueryResult;

            Assert.Single(edges);
            edges.Single().Conditions.Should().BeEquivalentTo(conditions);
        }

        [Fact]
        public async Task Assign_group_to_role_with_condition_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            string profileId = ProfilesWriteTestData.AssignGroupToRoleConditionally.GroupId;
            string parentId = ProfilesWriteTestData.AssignGroupToRoleConditionally.RoleId;
            string edgeName = GetEdgeCollection<IFirstLevelProjectionProfile, FirstLevelProjectionRole>();
            string temporaryAssignmentCollectionName = GetCollectionName<FirstLevelProjectionTemporaryAssignment>();

            var conditions = new List<RangeCondition>
            {
                new RangeCondition
                {
                    Start = DateTime.Today.AddMonths(-2).ToUniversalTime(),
                    End = DateTime.Today.AddDays(25).ToUniversalTime()
                },
                new RangeCondition
                {
                    Start = DateTime.Today.AddMonths(6).ToUniversalTime(),
                    End = DateTime.Today.AddYears(2).ToUniversalTime()
                }
            };

            TemporaryAssignmentTestEntity[] expectedTemporaryAssignments = conditions
                .Select(
                    (c, i) =>
                        new TemporaryAssignmentTestEntity
                        {
                            State = i == 0
                                ? TemporaryAssignmentState
                                    .ActiveWithExpiration
                                : TemporaryAssignmentState
                                    .NotProcessed,
                            Start = c.Start,
                            End = c.End,
                            TargetType = ObjectType.Role,
                            ProfileType = ObjectType.Group,
                            ProfileId = profileId,
                            TargetId = parentId,
                            StoredCompoundKey =
                                CompoundKeyHelpers
                                    .CalculateCompoundKey(
                                        profileId,
                                        parentId,
                                        c.Start,
                                        c.End)
                        })
                .ToArray();

            await repo.CreateProfileAssignmentAsync(
                parentId,
                ContainerType.Role,
                profileId,
                conditions);

            MultiApiResponse<FirstLevelProjectionAssignment> edgeResponse = await GetArangoClient()
                .ExecuteQueryAsync<FirstLevelProjectionAssignment>(
                    $"FOR e IN {edgeName} FILTER e._to LIKE \"%/{parentId}\" AND e._from LIKE \"%/{profileId}\" RETURN e");

            if (edgeResponse.Error)
            {
                throw new Exception();
            }

            IReadOnlyList<FirstLevelProjectionAssignment> edges = edgeResponse.QueryResult;

            Assert.Single(edges);
            edges.Single().Conditions.Should().BeEquivalentTo(conditions);

            IReadOnlyList<TemporaryAssignmentTestEntity> dbAssignments =
                await GetDocumentObjectsAsync<TemporaryAssignmentTestEntity>(
                    $"FOR a IN {temporaryAssignmentCollectionName} "
                    + $"FILTER a.ProfileId==\"{profileId}\""
                    + $"AND a.TargetId==\"{parentId}\""
                    + "RETURN a",
                    true);

            dbAssignments.Should()
                .BeEquivalentTo(
                    expectedTemporaryAssignments,
                    o =>
                        o.Excluding(a => a.Id)
                            .Excluding(a => a.LastErrorMessage)
                            .Excluding(a => a.LastModified)
                            .Excluding(a => a.CompoundKey));
        }

        [Fact]
        public async Task Assign_user_to_not_existing_role_should_throw()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            var parentId = "this-role-does-not-exist";
            string profileId = ProfilesWriteTestData.AssignUserToNotExistingRole.UserId;
            string edgeName = GetEdgeCollection<IFirstLevelProjectionProfile, FirstLevelProjectionRole>();

            var conditions = new List<RangeCondition>
            {
                new RangeCondition()
            };

            await Assert.ThrowsAsync<InstanceNotFoundException>(
                () => repo.CreateProfileAssignmentAsync(
                    parentId,
                    ContainerType.Role,
                    profileId,
                    conditions));

            MultiApiResponse<FirstLevelProjectionAssignment> edgeResponse = await GetArangoClient()
                .ExecuteQueryAsync<FirstLevelProjectionAssignment>(
                    $"FOR e IN {edgeName} FILTER e._to LIKE \"%/{parentId}\" AND e._from LIKE \"%/{profileId}\" RETURN e");

            if (edgeResponse.Error)
            {
                throw new Exception();
            }

            Assert.Empty(edgeResponse.QueryResult);
        }

        [Fact]
        public async Task Assign_group_to_not_existing_role_should_throw()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            var parentId = "this-role-does-not-exist";
            string profileId = ProfilesWriteTestData.AssignGroupToNotExistingRole.GroupId;
            string edgeName = GetEdgeCollection<IFirstLevelProjectionProfile, FirstLevelProjectionRole>();

            var conditions = new List<RangeCondition>
            {
                new RangeCondition()
            };

            await Assert.ThrowsAsync<InstanceNotFoundException>(
                () => repo.CreateProfileAssignmentAsync(
                    parentId,
                    ContainerType.Role,
                    profileId,
                    conditions));

            MultiApiResponse<FirstLevelProjectionAssignment> edgeResponse = await GetArangoClient()
                .ExecuteQueryAsync<FirstLevelProjectionAssignment>(
                    $"FOR e IN {edgeName} FILTER e._to LIKE \"%/{parentId}\" AND e._from LIKE \"%/{profileId}\" RETURN e");

            if (edgeResponse.Error)
            {
                throw new Exception();
            }

            Assert.Empty(edgeResponse.QueryResult);
        }

        [Fact]
        public async Task Assign_user_to_function_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            string profileId = ProfilesWriteTestData.AssignUserToFunction.UserId;
            string parentId = ProfilesWriteTestData.AssignUserToFunction.FunctionId;
            string edgeName = GetEdgeCollection<IFirstLevelProjectionProfile, FirstLevelProjectionRole>();

            var conditions = new List<RangeCondition>
            {
                new RangeCondition()
            };

            string tempAssignCollectionName = GetCollectionName<FirstLevelProjectionTemporaryAssignment>();

            await repo.CreateProfileAssignmentAsync(
                parentId,
                ContainerType.Function,
                profileId,
                conditions);

            MultiApiResponse<FirstLevelProjectionAssignment> edgeResponse = await GetArangoClient()
                .ExecuteQueryAsync<FirstLevelProjectionAssignment>(
                    $"FOR e IN {edgeName} FILTER e._to LIKE \"%/{parentId}\" AND e._from LIKE \"%/{profileId}\" RETURN e");

            if (edgeResponse.Error)
            {
                throw new Exception();
            }

            IReadOnlyList<FirstLevelProjectionAssignment> edges = edgeResponse.QueryResult;

            Assert.Single(edges);
            edges.Single().Conditions.Should().BeEquivalentTo(conditions);

            long temporaryAssignmentAmount = await GetResultCountAsync(
                $"FOR a IN {tempAssignCollectionName} FILTER a.ProfileId==\"{profileId}\" AND a.TargetId==\"{parentId}\" RETURN a");

            Assert.Equal(0, temporaryAssignmentAmount);
        }

        [Fact]
        public async Task Assign_group_to_function_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            string profileId = ProfilesWriteTestData.AssignGroupToFunction.GroupId;
            string parentId = ProfilesWriteTestData.AssignGroupToFunction.FunctionId;
            string edgeName = GetEdgeCollection<IFirstLevelProjectionProfile, FirstLevelProjectionRole>();

            var conditions = new List<RangeCondition>
            {
                new RangeCondition()
            };

            await repo.CreateProfileAssignmentAsync(
                parentId,
                ContainerType.Function,
                profileId,
                conditions);

            MultiApiResponse<FirstLevelProjectionAssignment> edgeResponse = await GetArangoClient()
                .ExecuteQueryAsync<FirstLevelProjectionAssignment>(
                    $"FOR e IN {edgeName} FILTER e._to LIKE \"%/{parentId}\" AND e._from LIKE \"%/{profileId}\" RETURN e");

            if (edgeResponse.Error)
            {
                throw new Exception();
            }

            IReadOnlyList<FirstLevelProjectionAssignment> edges = edgeResponse.QueryResult;

            Assert.Single(edges);
            edges.Single().Conditions.Should().BeEquivalentTo(conditions);
        }

        [Fact]
        public async Task Remove_infinite_user_to_group_assignment_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            string profileId = ProfilesWriteTestData.RemoveInfiniteUserToGroupAssignment.UserId;
            string parentId = ProfilesWriteTestData.RemoveInfiniteUserToGroupAssignment.GroupId;
            string edgeName = GetEdgeCollection<IFirstLevelProjectionProfile, FirstLevelProjectionGroup>();

            await repo.DeleteProfileAssignmentAsync(
                parentId,
                ContainerType.Group,
                profileId,
                new[] { new RangeCondition(null, null) });

            Assert.Equal(
                0,
                await GetRelationCountAsync<IFirstLevelProjectionProfile>(
                    edgeName,
                    profileId));
        }

        [Fact]
        public async Task Remove_infinite_user_to_role_assignment_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            string profileId = ProfilesWriteTestData.RemoveInfiniteUserToRoleAssignment.UserId;
            string parentId = ProfilesWriteTestData.RemoveInfiniteUserToRoleAssignment.RoleId;
            string edgeName = GetEdgeCollection<IFirstLevelProjectionProfile, FirstLevelProjectionRole>();

            await repo.DeleteProfileAssignmentAsync(
                parentId,
                ContainerType.Role,
                profileId,
                new[] { new RangeCondition(null, null) });

            Assert.Equal(
                0,
                await GetRelationCountAsync<IFirstLevelProjectionProfile>(
                    edgeName,
                    profileId));
        }

        [Fact]
        public async Task Remove_infinite_user_to_function_assignment_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            string profileId = ProfilesWriteTestData.RemoveInfiniteUserToFunctionAssignment.UserId;
            string parentId = ProfilesWriteTestData.RemoveInfiniteUserToFunctionAssignment.FunctionId;
            string edgeName = GetEdgeCollection<IFirstLevelProjectionProfile, FirstLevelProjectionFunction>();

            await repo.DeleteProfileAssignmentAsync(
                parentId,
                ContainerType.Function,
                profileId,
                new[] { new RangeCondition(null, null) });

            Assert.Equal(
                0,
                await GetRelationCountAsync<IFirstLevelProjectionProfile>(
                    edgeName,
                    profileId));
        }

        [Fact]
        public async Task Remove_conditional_user_to_group_assignment_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            string profileId = ProfilesWriteTestData.RemoveConditionalUserToGroupAssignment.UserId;
            string parentId = ProfilesWriteTestData.RemoveConditionalUserToGroupAssignment.GroupId;
            string edgeName = GetEdgeCollection<IFirstLevelProjectionProfile, FirstLevelProjectionGroup>();
            string collectionName = GetCollectionName<FirstLevelProjectionTemporaryAssignment>();

            await repo.DeleteProfileAssignmentAsync(
                parentId,
                ContainerType.Group,
                profileId,
                new[]
                {
                    new RangeCondition(
                        DateTime.Today.AddDays(-5).ToUniversalTime(),
                        DateTime.Today.AddDays(5).ToUniversalTime())
                });

            Assert.Equal(
                0,
                await GetRelationCountAsync<IFirstLevelProjectionProfile>(
                    edgeName,
                    profileId));

            long temporaryAssignmentAmount = await GetResultCountAsync(
                $"FOR a IN {collectionName} FILTER a.ProfileId==\"{profileId}\" AND a.TargetId==\"{parentId}\" RETURN a");

            Assert.Equal(0, temporaryAssignmentAmount);
        }

        [Fact]
        public async Task Remove_one_conditional_user_to_group_assignment_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            string profileId = ProfilesWriteTestData.RemoveOneConditionalUserToGroupAssignment.UserId;
            string parentId = ProfilesWriteTestData.RemoveOneConditionalUserToGroupAssignment.GroupId;
            string edgeName = GetEdgeCollection<IFirstLevelProjectionProfile, FirstLevelProjectionGroup>();
            string collectionName = GetCollectionName<FirstLevelProjectionTemporaryAssignment>();

            await repo.DeleteProfileAssignmentAsync(
                parentId,
                ContainerType.Group,
                profileId,
                new[]
                {
                    new RangeCondition(
                        DateTime.Today.AddDays(-10).ToUniversalTime(),
                        DateTime.Today.ToUniversalTime())
                });

            Assert.Equal(
                1,
                await GetRelationCountAsync<IFirstLevelProjectionProfile>(
                    edgeName,
                    profileId));

            string compoundKey = CompoundKeyHelpers.CalculateCompoundKey(
                profileId,
                parentId,
                DateTime.Today.AddDays(-10).ToUniversalTime(),
                DateTime.Today.ToUniversalTime());

            long temporaryAssignmentAmount = await GetResultCountAsync(
                @$"
                        FOR a IN {
                            collectionName
                        } 
                          FILTER a.ProfileId==""{
                              profileId
                          }"" 
                             AND a.TargetId==""{
                                 parentId
                             }""
                             AND a.CompoundKey==""{
                                 compoundKey
                             }""
                          RETURN a");

            Assert.Equal(0, temporaryAssignmentAmount);
        }
    }
}
