using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Maverick.UserProfileService.Models.BasicModels;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Arango.IntegrationTests.V2.Abstractions;
using UserProfileService.Arango.IntegrationTests.V2.Fixtures;
using UserProfileService.Arango.IntegrationTests.V2.Helpers;
using UserProfileService.Common.Tests.Utilities.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Sync.Abstraction.Models.Entities;
using UserProfileService.Sync.Abstraction.Stores;
using Xunit;

namespace UserProfileService.Arango.IntegrationTests.V2
{
    [Collection(nameof(DatabaseCollection))]
    public class ArangoSyncEntityStoreTests : ArangoDbTestBase
    {

        private readonly DatabaseFixture _fixture;

        public ArangoSyncEntityStoreTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        #region Users
        [Fact]
        public async Task GetAllUsers_should_work()
        {
            IEntityStore store = await _fixture.GetSyncRepository();
            IPaginatedList<UserSync> users = await store.GetUsersAsync();
            List<UserSync> repoUsers = users.ToList();
            List<UserSync> savedUsers = SampleDataTestHelper.GetTestUserEntities().Select(x => GetMapper().Map<UserEntityModel, UserSync>(x)).ToList();

            repoUsers.Should().BeEquivalentTo(savedUsers);
        }

        [Fact]
        public async Task GetUserById_should_work()
        {
            IEntityStore store = await _fixture.GetSyncRepository();
            UserEntityModel testUser = SampleDataTestHelper.GetTestUserEntities().PickRandom();

            var user = await store.GetProfileAsync<UserSync>(testUser.Id);
            UserSync repoUser = GetMapper().Map<UserEntityModel, UserSync>(testUser);

            repoUser.Should().BeEquivalentTo(user);
        }

        [Fact]
        public async Task CreateUser_should_work()
        {
            IEntityStore store = await _fixture.GetSyncRepository();
            UserEntityModel testUser = SampleDataTestHelper.GenerateTestUser(Guid.NewGuid().ToString());
            var userToCreate = GetMapper().Map<UserSync>(testUser);
            await store.CreateProfileAsync(userToCreate);

            var createdUser = await store.GetProfileAsync<UserSync>(testUser.Id);

            userToCreate.Should().BeEquivalentTo(createdUser);
        }

        [Fact]
        public async Task UpdateUser_should_work()
        {
            IEntityStore store = await _fixture.GetSyncRepository();
            UserEntityModel testUser = SampleDataTestHelper.GetTestUserEntities().PickRandom();
            testUser.Name = "Winner";
            var convertedTestUser = GetMapper().Map<UserSync>(testUser);
            await store.UpdateProfileAsync(convertedTestUser);

            var repoUser = await store.GetProfileAsync<UserSync>(testUser.Id);

            repoUser.Should().BeEquivalentTo(convertedTestUser);
        }

        [Fact]
        public async Task DeleteUser_should_work()
        {
            IEntityStore store = await _fixture.GetSyncRepository();
            UserEntityModel testUser = SampleDataTestHelper.GetTestUserEntities().PickRandom();

            await store.DeleteProfileAsync<UserSync>(testUser.Id);

            var repoUser = await store.GetProfileAsync<UserSync>(testUser.Id);

            repoUser.Should().BeNull();
        }



        #endregion

        #region Groups

        [Fact]
        public async Task GetAllGroups_should_work()
        {
            IEntityStore store = await _fixture.GetSyncRepository();
            IPaginatedList<GroupSync> groups = await store.GetGroupsAsync();
            List<GroupSync> repoGroups = groups.ToList();
            List<GroupSync> savedGroups = SampleDataTestHelper.GetTestGroupEntities().Select(x => GetMapper().Map<GroupEntityModel, GroupSync>(x)).ToList();

            repoGroups.Should().BeEquivalentTo(savedGroups);
        }

        [Fact]
        public async Task GetGroupById_should_work()
        {
            IEntityStore store = await _fixture.GetSyncRepository();
            GroupEntityModel testGroup = SampleDataTestHelper.GetTestGroupEntities().PickRandom();

            var group = await store.GetProfileAsync<GroupSync>(testGroup.Id);
            GroupSync repoGroup = GetMapper().Map<GroupEntityModel, GroupSync>(testGroup);

            repoGroup.Should().BeEquivalentTo(group);
        }

        [Fact]
        public async Task CreateGroup_should_work()
        {
            IEntityStore store = await _fixture.GetSyncRepository();
            GroupEntityModel testGroup = SampleDataTestHelper.GenerateTestGroup(Guid.NewGuid().ToString());
            var groupToCreate = GetMapper().Map<GroupSync>(testGroup);
            await store.CreateProfileAsync(groupToCreate);

            var createdGroup = await store.GetProfileAsync<GroupSync>(testGroup.Id);

            groupToCreate.Should().BeEquivalentTo(createdGroup);
        }

        [Fact]
        public async Task UpdateGroup_should_work()
        {
            IEntityStore store = await _fixture.GetSyncRepository();
            GroupEntityModel testGroup = SampleDataTestHelper.GetTestGroupEntities().PickRandom();
            testGroup.Name = "Winner";
            var convertedGroup = GetMapper().Map<GroupSync>(testGroup);
            await store.UpdateProfileAsync(convertedGroup);

            var repoGroup = await store.GetProfileAsync<GroupSync>(testGroup.Id);

            repoGroup.Should().BeEquivalentTo(convertedGroup);
        }

        [Fact]
        public async Task DeleteGroup_should_work()
        {
            IEntityStore store = await _fixture.GetSyncRepository();
            GroupEntityModel testGroup = SampleDataTestHelper.GetTestGroupEntities().PickRandom();

            await store.DeleteProfileAsync<GroupSync>(testGroup.Id);

            var repoGroup = await store.GetProfileAsync<GroupSync>(testGroup.Id);

            repoGroup.Should().BeNull();
        }

        #endregion

        #region Roles


        [Fact]
        public async Task GetAllRoles_should_work()
        {
            IEntityStore store = await _fixture.GetSyncRepository();
            IPaginatedList<RoleSync> roles = await store.GetRolesAsync<RoleSync>();
            List<RoleSync> repoGroups = roles.ToList();
            List<RoleSync> savedRoles = SampleDataTestHelper.GetTestRoleEntities().Select(x => GetMapper().Map<RoleObjectEntityModel, RoleSync>(x)).ToList();

            repoGroups.Should().BeEquivalentTo(savedRoles);
        }

        [Fact]
        public async Task GetRoleById_should_work()
        {
            IEntityStore store = await _fixture.GetSyncRepository();
            RoleObjectEntityModel testRole = SampleDataTestHelper.GetTestRoleEntities().PickRandom();

            RoleSync role = await store.GetRoleAsync(testRole.Id);
            var repoGroup = GetMapper().Map<RoleSync>(testRole);

            repoGroup.Should().BeEquivalentTo(role);
        }

        [Fact]
        public async Task CreateRole_should_work()
        {
            IEntityStore store = await _fixture.GetSyncRepository();
            RoleObjectEntityModel testRole = SampleDataTestHelper.GenerateTestRole(Guid.NewGuid().ToString());
            var roleToCreate = GetMapper().Map<RoleSync>(testRole);
            await store.CreateRoleAsync(roleToCreate);

            RoleSync createdRole = await store.GetRoleAsync(testRole.Id);

            roleToCreate.Should().BeEquivalentTo(createdRole);
        }

        [Fact]
        public async Task UpdateRole_should_work()
        {
            IEntityStore store = await _fixture.GetSyncRepository();
            RoleObjectEntityModel testRole = SampleDataTestHelper.GetTestRoleEntities().PickRandom();
            testRole.Name = "Winner";
            var convertedRole = GetMapper().Map<RoleSync>(testRole);
            await store.UpdateRoleAsync(convertedRole);

            RoleSync repoRole = await store.GetRoleAsync(testRole.Id);

            repoRole.Should().BeEquivalentTo(convertedRole);
        }

        [Fact]
        public async Task DeleteRole_should_work()
        {
            IEntityStore store = await _fixture.GetSyncRepository();
            RoleObjectEntityModel testRole = SampleDataTestHelper.GetTestRoleEntities().PickRandom();

            await store.DeleteRoleAsync(testRole.Id);

            RoleSync repoRole = await store.GetRoleAsync(testRole.Id);

            repoRole.Should().BeNull();
        }

        #endregion

        #region Organization

        [Fact]
        public async Task GetAllOrganizations_should_work()
        {
            IEntityStore store = await _fixture.GetSyncRepository();
            IPaginatedList<OrganizationSync> organizations = await store.GetOrganizationsAsync();
            List<OrganizationSync> repoOrganizations = organizations.ToList();
            List<OrganizationSync> expectedList = SampleDataTestHelper.GetTestOrganizations().Select(x => GetMapper().Map<OrganizationSync>(x)).ToList();

            repoOrganizations.Should().BeEquivalentTo(expectedList);
        }

        [Fact]
        public async Task GetOrganizationById_should_work()
        {
            IEntityStore store = await _fixture.GetSyncRepository();
            OrganizationEntityModel referenceValue = SampleDataTestHelper.GetTestOrganizations().PickRandom();

            var repositoryOrganization = await store.GetProfileAsync<OrganizationSync>(referenceValue.Id);
            var expectedOrganization = GetMapper().Map<OrganizationSync>(referenceValue);

            repositoryOrganization.Should().BeEquivalentTo(expectedOrganization);
        }

        [Fact]
        public async Task CreateOrganization_should_work()
        {
            IEntityStore store = await _fixture.GetSyncRepository();
            OrganizationEntityModel organizationToCreate = SampleDataTestHelper.GetTestOrganizations().PickRandom();
            organizationToCreate.Id = Guid.NewGuid().ToString();

            var convertedOrganization = GetMapper().Map<OrganizationSync>(organizationToCreate);

            await store.CreateProfileAsync(convertedOrganization);

            var createdOrganization = await store.GetProfileAsync<OrganizationSync>(organizationToCreate.Id);

            convertedOrganization.Should().BeEquivalentTo(createdOrganization);
        }

        [Fact]
        public async Task UpdateOrganization_should_work()
        {
            IEntityStore store = await _fixture.GetSyncRepository();
            OrganizationEntityModel testOrganization = SampleDataTestHelper.GetTestOrganizations().PickRandom();
            testOrganization.Name = "Winner";
            var convertedOrganization = GetMapper().Map<OrganizationSync>(testOrganization);
            await store.UpdateProfileAsync(convertedOrganization);

            var repoOrganization = await store.GetProfileAsync<OrganizationSync>(testOrganization.Id);

            repoOrganization.Should().BeEquivalentTo(convertedOrganization);
        }

        [Fact]
        public async Task DeleteOrganization_should_work()
        {
            IEntityStore store = await _fixture.GetSyncRepository();
            OrganizationEntityModel testOrganization = SampleDataTestHelper.GetTestOrganizations().PickRandom();
            await store.DeleteProfileAsync<OrganizationSync>(testOrganization.Id);

            var repoOrganization = await store.GetProfileAsync<OrganizationSync>(testOrganization.Id);

            repoOrganization.Should().BeNull();
        }

        #endregion

        #region Functions

        [Fact]
        public async Task GetAllFunctions_should_work()
        {
            IEntityStore store = await _fixture.GetSyncRepository();
            IPaginatedList<FunctionSync> functions = await store.GetFunctionsAsync<FunctionSync>();
            List<FunctionSync> repoFunctions = functions.ToList();
            List<FunctionSync> savedFunctions = SampleDataTestHelper.GetTestFunctionEntities().Select(x => GetMapper().Map<FunctionSync>(x)).ToList();

            repoFunctions.Should().BeEquivalentTo(savedFunctions, setup => setup.ComparingByMembers<OrganizationBasic>());
        }

        [Fact]
        public async Task GetFunctionsById_should_work()
        {
            IEntityStore store = await _fixture.GetSyncRepository();
            FunctionObjectEntityModel testFunction = SampleDataTestHelper.GetTestFunctionEntities().PickRandom();

            FunctionSync repoFunction = await store.GetFunctionAsync(testFunction.Id);
            var expectedFunction = GetMapper().Map<FunctionSync>(testFunction);

            expectedFunction.Should().BeEquivalentTo(repoFunction);
        }

        [Fact]
        public async Task CreateFunction_should_work()
        {
            IEntityStore store = await _fixture.GetSyncRepository();
            var functionToCreate = GetMapper().Map<FunctionSync>(SampleDataTestHelper.GetTestFunctionEntities().PickRandom());
            functionToCreate.Id = Guid.NewGuid().ToString();

            await store.CreateFunctionAsync(functionToCreate);

            FunctionSync createdFunction = await store.GetFunctionAsync(functionToCreate.Id);

            functionToCreate.Should().BeEquivalentTo(createdFunction);
        }

        [Fact]
        public async Task UpdateFunction_should_work()
        {
            IEntityStore store = await _fixture.GetSyncRepository();
            FunctionObjectEntityModel testFunction = SampleDataTestHelper.GetTestFunctionEntities().PickRandom();
            testFunction.Name = "Winner";
            var convertedFunction = GetMapper().Map<FunctionSync>(testFunction);
            await store.UpdateFunctionAsync(convertedFunction);

            FunctionSync repoFunction = await store.GetFunctionAsync(testFunction.Id);

            repoFunction.Should().BeEquivalentTo(convertedFunction);
        }

        [Fact]
        public async Task DeleteFunction_should_work()
        {
            IEntityStore store = await _fixture.GetSyncRepository();
            FunctionObjectEntityModel testFunction = SampleDataTestHelper.GetTestFunctionEntities().PickRandom();

            await store.DeleteFunctionAsync(testFunction.Id);

            FunctionSync repoFunction = await store.GetFunctionAsync(testFunction.Id);

            repoFunction.Should().BeNull();
        }

        #endregion


    }
}
