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

namespace UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository
{
    [Collection(nameof(FirstLevelProjectionCollection))]
    public class FunctionWriteTests : ArangoFirstLevelRepoTestBase
    {
        private readonly FirstLevelProjectionFixture _fixture;

        public FunctionWriteTests(FirstLevelProjectionFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Create_function_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            var role = await GetDocumentObjectAsync<FirstLevelProjectionRole>(
                FunctionWriteTestData.CreateFunction.RoleId);

            var organization =
                await GetDocumentObjectAsync<FirstLevelProjectionOrganization>(
                    FunctionWriteTestData.CreateFunction.OrganizationId);

            FirstLevelProjectionFunction function = MockDataGenerator.GenerateFunction(
                role: role,
                organization: organization);

            await repo.CreateFunctionAsync(function);

            var dbFunction = await GetDocumentObjectAsync<FirstLevelProjectionFunction>(function.Id);
            dbFunction.Should().BeEquivalentTo(function);

            Assert.Equal(
                2, // expect the created edges
                await GetRelationCountAsync<FirstLevelProjectionFunction>(
                    GetEdgeCollection<FirstLevelProjectionFunction, FirstLevelProjectionRole>(),
                    function.Id));
        }

        [Fact]
        public async Task Delete_function_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            await repo.DeleteFunctionAsync(FunctionWriteTestData.DeleteFunction.TargetFunctionId);

            var dbFunction = await GetDocumentObjectAsync<FirstLevelProjectionFunction>(
                FunctionWriteTestData.DeleteFunction.TargetFunctionId,
                false);

            dbFunction.Should().BeNull("The function was deleted");

            // no connecting edges should exist to roles and oes
            Assert.Equal(
                0,
                await GetRelationCountAsync<FirstLevelProjectionFunction>(
                    GetEdgeCollection<FirstLevelProjectionFunction, FirstLevelProjectionRole>(),
                    FunctionWriteTestData.DeleteFunction.TargetFunctionId));
        }

        [Fact]
        public async Task Delete_function_with_time_relevant_assignees_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();
            string collectionName = GetCollectionName<FirstLevelProjectionTemporaryAssignment>();

            await repo.DeleteFunctionAsync(FunctionWriteTestData.DeleteAnotherFunction.TargetFunctionId);

            var dbFunction = await GetDocumentObjectAsync<FirstLevelProjectionFunction>(
                FunctionWriteTestData.DeleteAnotherFunction.TargetFunctionId,
                false);

            dbFunction.Should().BeNull("The function was deleted");

            // no connecting edges should exist to roles and oes
            Assert.Equal(
                0,
                await GetRelationCountAsync<FirstLevelProjectionFunction>(
                    GetEdgeCollection<FirstLevelProjectionFunction, FirstLevelProjectionRole>(),
                    FunctionWriteTestData.DeleteAnotherFunction.TargetFunctionId));

            IReadOnlyList<TemporaryAssignmentTestEntity> dbAssignmentsFunction =
                await GetDocumentObjectsAsync<TemporaryAssignmentTestEntity>(
                    "FOR a IN @@collection FILTER a.TargetId==@fId RETURN a",
                    true,
                    "fId",
                    FunctionWriteTestData.DeleteAnotherFunction.TargetFunctionId,
                    "@collection",
                    collectionName);

            dbAssignmentsFunction.Should()
                .BeEmpty("because the function has been deleted - so its temporary assignments");

            IReadOnlyList<TemporaryAssignmentTestEntity> dbAssignmentUser =
                await GetDocumentObjectsAsync<TemporaryAssignmentTestEntity>(
                    "FOR a IN @@collection FILTER a.ProfileId==@pId RETURN a",
                    true,
                    "pId",
                    FunctionWriteTestData.DeleteAnotherFunction.UserId,
                    "@collection",
                    collectionName);

            dbAssignmentUser.Should().ContainSingle("because the user still as a temporary assignment to a role");
            Assert.Equal(ObjectType.Role, dbAssignmentUser.Single().TargetType);

            Assert.Equal(
                FunctionWriteTestData.DeleteAnotherFunction.RoleId,
                dbAssignmentUser.Single().TargetId);
        }

        [Fact]
        public async Task Delete_not_existing_function_should_throw()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            await Assert.ThrowsAsync<InstanceNotFoundException>(
                () => repo.DeleteFunctionAsync("this-function-does-not-exist"));
        }

        [Fact]
        public async Task Update_function_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            var role = await GetDocumentObjectAsync<FirstLevelProjectionRole>(
                FunctionWriteTestData.UpdateFunction.RoleId);

            var organization =
                await GetDocumentObjectAsync<FirstLevelProjectionOrganization>(
                    FunctionWriteTestData.UpdateFunction.OrganizationId);

            var previousFunction =
                await GetDocumentObjectAsync<FirstLevelProjectionFunction>(
                    FunctionWriteTestData.UpdateFunction.FunctionId);

            FirstLevelProjectionFunction function = MockDataGenerator.GenerateFunction(
                FunctionWriteTestData.UpdateFunction.FunctionId,
                role,
                organization);

            await repo.UpdateFunctionAsync(function);

            var dbFunction = await GetDocumentObjectAsync<FirstLevelProjectionFunction>(function.Id);
            dbFunction.Should().BeEquivalentTo(function);
            dbFunction.Should().NotBeEquivalentTo(previousFunction);
        }

        [Fact]
        public async Task Update_not_existing_function_should_throw()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            FirstLevelProjectionFunction function = MockDataGenerator.GenerateFunction();

            await Assert.ThrowsAsync<InstanceNotFoundException>(() => repo.UpdateFunctionAsync(function));
        }

        [Fact]
        public async Task Add_tag_to_function_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            await repo.AddTagToFunctionAsync(
                new FirstLevelProjectionTagAssignment
                {
                    IsInheritable = true,
                    TagId = FunctionWriteTestData.AddTag.TagId
                },
                FunctionWriteTestData.AddTag.FunctionId);

            //TODO compare
        }

        [Fact]
        public async Task Add_tag_to_not_existing_function_should_throw()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            await Assert.ThrowsAsync<InstanceNotFoundException>(
                () => repo.AddTagToFunctionAsync(
                    new FirstLevelProjectionTagAssignment
                    {
                        IsInheritable = true,
                        TagId = FunctionWriteTestData.AddTagToNotExistingFunction.TagId
                    },
                    "this-function-does-not-exist"));
        }

        [Fact]
        public async Task Add_not_existing_tag_to_function_should_throw()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            await Assert.ThrowsAsync<InstanceNotFoundException>(
                () => repo.AddTagToFunctionAsync(
                    new FirstLevelProjectionTagAssignment
                    {
                        IsInheritable = true,
                        TagId = "this-tag-does-not-exist"
                    },
                    FunctionWriteTestData.AddNotExistingTag.FunctionId));

            Assert.Equal(
                0,
                await GetRelationCountAsync<FirstLevelProjectionFunction>(
                    GetEdgeCollection<FirstLevelProjectionFunction, FirstLevelProjectionTag>(),
                    FunctionWriteTestData.AddNotExistingTag.FunctionId));
        }

        [Fact]
        public async Task Remove_tag_from_not_existing_function_should_throw()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            await Assert.ThrowsAsync<InstanceNotFoundException>(
                () => repo.RemoveTagFromRoleAsync(
                    FunctionWriteTestData.RemoveTagFromNotExistingFunction.TagId,
                    "this-function-does-not-exist"));
        }

        [Fact]
        public async Task Remove_not_existing_tag_from_function_should_throw()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            await Assert.ThrowsAsync<InstanceNotFoundException>(
                () => repo.RemoveTagFromRoleAsync(
                    "this-tag-does-not-exist",
                    FunctionWriteTestData.RemoveNotExistingTag.FunctionId));
        }

        [Fact]
        public async Task Remove_not_existing_tag_assignment_from_function_should_throw()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            await Assert.ThrowsAsync<InstanceNotFoundException>(
                () => repo.RemoveTagFromRoleAsync(
                    FunctionWriteTestData.RemoveNotAssignedTag.TagId,
                    FunctionWriteTestData.RemoveNotAssignedTag.FunctionId));
        }
    }
}
