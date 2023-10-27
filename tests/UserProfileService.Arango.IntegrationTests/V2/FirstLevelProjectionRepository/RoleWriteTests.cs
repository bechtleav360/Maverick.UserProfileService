using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Maverick.UserProfileService.Models.EnumModels;
using UserProfileService.Arango.IntegrationTests.V2.Abstractions;
using UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Data;
using UserProfileService.Arango.IntegrationTests.V2.Fixtures;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using Xunit;
using Xunit.Abstractions;

namespace UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository
{
    [Collection(nameof(FirstLevelProjectionCollection))]
    public class RoleWriteTests : ArangoFirstLevelRepoTestBase
    {
        private readonly FirstLevelProjectionFixture _fixture;
        private readonly ITestOutputHelper _output;

        public RoleWriteTests(FirstLevelProjectionFixture fixture, ITestOutputHelper outputHelper)
        {
            _fixture = fixture;
            _output = outputHelper;
        }

        [Fact]
        public async Task Create_role_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();
            FirstLevelProjectionRole role = MockDataGenerator.GenerateRole();

            await repo.CreateRoleAsync(role);

            var dbRole = await GetDocumentObjectAsync<FirstLevelProjectionRole>(role.Id);
            dbRole.Should().BeEquivalentTo(role);
        }

        [Fact]
        public async Task Delete_role_with_time_relevant_assignees_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();
            string collectionName = GetCollectionName<FirstLevelProjectionTemporaryAssignment>();

            await repo.DeleteRoleAsync(RoleWriteTestData.DeleteAnotherRole.TargetRoleId);

            var dbRole =
                await GetDocumentObjectAsync<FirstLevelProjectionRole>(
                    RoleWriteTestData.DeleteAnotherRole.TargetRoleId,
                    false);

            dbRole.Should().BeNull("because the role has been deleted");

            IReadOnlyList<TemporaryAssignmentTestEntity> dbAssignmentsRole =
                await GetDocumentObjectsAsync<TemporaryAssignmentTestEntity>(
                    "FOR a IN @@collection FILTER a.TargetId==@rId RETURN a",
                    true,
                    "rId",
                    RoleWriteTestData.DeleteAnotherRole.TargetRoleId,
                    "@collection",
                    collectionName);

            dbAssignmentsRole.Should()
                .BeEmpty("because the role has been deleted and so its temporary assignments");

            IReadOnlyList<TemporaryAssignmentTestEntity> dbAssignmentUser =
                await GetDocumentObjectsAsync<TemporaryAssignmentTestEntity>(
                    "FOR a IN @@collection FILTER a.ProfileId==@pId RETURN a",
                    true,
                    "pId",
                    RoleWriteTestData.DeleteAnotherRole.UserId,
                    "@collection",
                    collectionName);

            dbAssignmentUser.Should().ContainSingle("because the user still as a temporary assignment to a role");
            Assert.Equal(ObjectType.Role, dbAssignmentUser.Single().TargetType);

            Assert.Equal(
                RoleWriteTestData.DeleteAnotherRole.RoleId,
                dbAssignmentUser.Single().TargetId);
        }

        [Fact]
        public async Task Delete_role_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            await repo.DeleteRoleAsync(RoleWriteTestData.DeleteRole.TargetRoleId);

            var dbRole =
                await GetDocumentObjectAsync<FirstLevelProjectionRole>(
                    RoleWriteTestData.DeleteRole.TargetRoleId,
                    false);

            Assert.Null(dbRole);
            //TODO check edges
        }

        [Fact]
        public async Task Delete_not_existing_role_should_throw()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            await Assert.ThrowsAsync<InstanceNotFoundException>(() => repo.DeleteRoleAsync("this-role-does-not-exist"));
        }

        [Fact]
        public async Task Update_role_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();
            FirstLevelProjectionRole newRole = MockDataGenerator.GenerateRole(RoleWriteTestData.UpdateRole.RoleId);

            var previousRole =
                await GetDocumentObjectAsync<FirstLevelProjectionRole>(RoleWriteTestData.UpdateRole.RoleId);

            await repo.UpdateRoleAsync(newRole);

            var dbRole = await GetDocumentObjectAsync<FirstLevelProjectionRole>(RoleWriteTestData.UpdateRole.RoleId);
            Assert.NotNull(dbRole);

            dbRole.Should().NotBeEquivalentTo(previousRole);
            dbRole.Should().BeEquivalentTo(newRole);
        }

        [Fact]
        public async Task Update_not_existing_role_should_throw()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();
            FirstLevelProjectionRole newRole = MockDataGenerator.GenerateRole("this-role-does-not-exist");

            await Assert.ThrowsAsync<InstanceNotFoundException>(() => repo.UpdateRoleAsync(newRole));
        }

        [Fact]
        public async Task Add_tag_to_role_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            await repo.AddTagToRoleAsync(
                new FirstLevelProjectionTagAssignment
                {
                    IsInheritable = true,
                    TagId = RoleWriteTestData.AddTag.TagId
                },
                RoleWriteTestData.AddTag.RoleId);

            var dbRole = await GetDocumentObjectAsync<FirstLevelProjectionRole>(RoleWriteTestData.AddTag.RoleId);

            Assert.Equal(
                1,
                await GetRelationCountAsync<FirstLevelProjectionRole>(
                    GetEdgeCollection<FirstLevelProjectionRole, FirstLevelProjectionTag>(),
                    dbRole.Id));
            //TODO checkEdge
        }

        [Fact]
        public async Task Add_tag_to_not_existing_role_should_throw()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            await Assert.ThrowsAsync<InstanceNotFoundException>(
                () => repo.AddTagToRoleAsync(
                    new FirstLevelProjectionTagAssignment
                    {
                        IsInheritable = true,
                        TagId = RoleWriteTestData.AddTagToNotExistingRole.TagId
                    },
                    "this-role-does-not-exist"));

            // Ensure nothing was created in the db
            var dbTag = await GetDocumentObjectAsync<FirstLevelProjectionTag>(
                RoleWriteTestData.AddTagToNotExistingRole.TagId);

            Assert.Equal(
                0,
                await GetRelationCountAsync<FirstLevelProjectionTag>(
                    GetEdgeCollection<FirstLevelProjectionRole, FirstLevelProjectionTag>(),
                    dbTag.Id));
        }

        [Fact]
        public async Task Add_not_existing_tag_to_role_should_throw()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            await Assert.ThrowsAsync<InstanceNotFoundException>(
                () => repo.AddTagToRoleAsync(
                    new FirstLevelProjectionTagAssignment
                    {
                        IsInheritable = true,
                        TagId = "this-tag-does-not-exist"
                    },
                    RoleWriteTestData.AddNotExistingTag.RoleId));

            // Ensure nothing was created in the db
            var dbRole =
                await GetDocumentObjectAsync<FirstLevelProjectionRole>(RoleWriteTestData.AddNotExistingTag.RoleId);

            Assert.NotNull(dbRole);

            Assert.Equal(
                0,
                await GetRelationCountAsync<FirstLevelProjectionRole>(
                    GetEdgeCollection<FirstLevelProjectionRole, FirstLevelProjectionTag>(),
                    dbRole.Id));
        }

        [Fact]
        public async Task Remove_tag_from_role_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            await repo.RemoveTagFromRoleAsync(
                RoleWriteTestData.RemoveTag.TagId,
                RoleWriteTestData.RemoveTag.RoleId);

            var dbRole = await GetDocumentObjectAsync<FirstLevelProjectionRole>(RoleWriteTestData.RemoveTag.RoleId);

            Assert.Equal(
                0,
                await GetRelationCountAsync<FirstLevelProjectionRole>(
                    GetEdgeCollection<FirstLevelProjectionRole, FirstLevelProjectionTag>(),
                    dbRole.Id));

            //TODO check edge for properties
        }

        [Fact]
        public async Task Remove_tag_from_not_existing_role_should_throw()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            await Assert.ThrowsAsync<InstanceNotFoundException>(
                () => repo.RemoveTagFromRoleAsync(
                    RoleWriteTestData.RemoveTagFromNotExistingRole.TagId,
                    "this-role-does-not-exist"));

            // Ensure nothing was created in the db
            var dbTag = await GetDocumentObjectAsync<FirstLevelProjectionTag>(
                RoleWriteTestData.RemoveTagFromNotExistingRole.TagId);

            Assert.Equal(
                0,
                await GetRelationCountAsync<FirstLevelProjectionTag>(
                    GetEdgeCollection<FirstLevelProjectionRole, FirstLevelProjectionTag>(),
                    dbTag.Id));
        }

        [Fact]
        public async Task Remove_not_existing_tag_from_role_should_throw()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            await Assert.ThrowsAsync<InstanceNotFoundException>(
                () => repo.RemoveTagFromRoleAsync(
                    "this-tag-does-not-exist",
                    RoleWriteTestData.RemoveFromNotExistingTag.RoleId));

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

        [Fact]
        public async Task Remove_not_existing_tag_assignment_from_role_should_throw()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            await Assert.ThrowsAsync<InstanceNotFoundException>(
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
    }
}
