using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Maverick.Client.ArangoDb.Public;
using Maverick.Client.ArangoDb.Public.Extensions;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Common.Models;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Arango.IntegrationTests.V2.Abstractions;
using UserProfileService.Arango.IntegrationTests.V2.Fixtures;
using UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Data;
using UserProfileService.Common.Tests.Utilities.MockDataBuilder;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using Xunit;

namespace UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository
{
    [Collection(nameof(SecondLevelProjectionCollection))]
    public class RoleTests : ArangoSecondLevelRepoTestBase
    {
        private readonly SecondLevelProjectionFixture _fixture;

        public static IEnumerable<object[]> RoleData =>
            new List<object[]>
            {
                new object[] { MockDataGenerator.GenerateSecondLevelProjectionRoles().Single() },
                new object[] { MockDataGenerator.GenerateSecondLevelProjectionRoles().Single() },
                new object[] { MockDataGenerator.GenerateSecondLevelProjectionRoles().Single() },
                new object[] { MockDataGenerator.GenerateSecondLevelProjectionRoles().Single() },
                new object[] { MockDataGenerator.GenerateSecondLevelProjectionRoles().Single() }
            };

        public RoleTests(SecondLevelProjectionFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Create_role_should_work()
        {
            ISecondLevelProjectionRepository repo = await _fixture.GetSecondLevelRepository();
            SecondLevelProjectionRole role = MockDataGenerator.GenerateSecondLevelProjectionRoles().Single();
            var referenceValue = GetMapper().Map<RoleBasic>(role);

            await repo.CreateRoleAsync(role);

            RoleBasic dbRole = await GetDocumentObjectAsync<RoleBasic, RoleObjectEntityModel>(role.Id);
            dbRole.Should().BeEquivalentTo(referenceValue);
        }

        [Theory]
        [MemberData(nameof(RoleData))]
        public async Task DeleteRole_should_work(SecondLevelProjectionRole role)
        {
            ISecondLevelProjectionRepository repo = await _fixture.GetSecondLevelRepository();

            await repo.CreateRoleAsync(role);
            var dbFunction = await GetDocumentObjectAsync<RoleObjectEntityModel>(role.Id);
            dbFunction.Should().NotBeNull();
            await repo.DeleteRoleAsync(role.Id);
            await Assert.ThrowsAsync<InstanceNotFoundException>(async () => await repo.GetRoleAsync(role.Id));
        }

        [Fact]
        public async Task UpdateRole_should_work()
        {
            ISecondLevelProjectionRepository repo = await _fixture.GetSecondLevelRepository();

            SecondLevelProjectionRole role = MockDataGenerator.GenerateSecondLevelProjectionRoles(
                    1,
                    false,
                    false)
                .Single();

            IArangoDbClient client = GetArangoClient();
            string roleCollection = GetCollectionName<RoleView>();
            role.Id = RoleTestData.UpdateRole.RoleId;
            role.Source = "Test";
            role.Name = "Emmanuel-Role";
            role.Description = "This is a role of the UpdateRole test!";
            var referenceValue = _fixture.Mapper.Map<RoleBasic>(role);

            await repo.UpdateRoleAsync(role);

            MultiApiResponse<RoleBasic> getRoleResponse = await client.ExecuteQueryAsync<RoleBasic>(
                $"FOR r IN {roleCollection} FILTER r.Id == \"{role.Id}\" RETURN r");

            if (getRoleResponse.Error || getRoleResponse.Responses.Any(r => r.Error))
            {
                throw new Exception();
            }

            RoleBasic updatedRole = getRoleResponse.QueryResult.Single();
            updatedRole.Should().BeEquivalentTo(referenceValue);
        }

        [Theory]
        [MemberData(nameof(RoleData))]
        public async Task AddTagToRole_should_work(SecondLevelProjectionRole role)
        {
            ISecondLevelProjectionRepository repo = await _fixture.GetSecondLevelRepository();
            var tag = GetMapper().Map<Tag>(MockDataGenerator.GenerateCalculatedTags().Single());
            await repo.CreateTagAsync(tag);
            await repo.CreateRoleAsync(role);

            IList<TagAssignment> tags = new List<TagAssignment>();

            tags.Add(
                new TagAssignment
                {
                    IsInheritable = false,
                    TagDetails = tag
                });

            await repo.AddTagToObjectAsync(role.Id, role.Id, ObjectType.Role, tags);

            string collectionName = _fixture.GetCollectionName<SecondLevelProjectionRole>();

            var aqlQuery = @$" FOR x in {
                collectionName
            }  
                   FILTER x.Id == ""{
                       role.Id
                   }""
                   RETURN x.Tags";

            MultiApiResponse<IList<TagAssignment>> response =
                await GetArangoClient().ExecuteQueryAsync<IList<TagAssignment>>(aqlQuery);

            response.QueryResult.FirstOrDefault()
                ?.FirstOrDefault()
                ?
                .Should()
                .BeEquivalentTo(tags.FirstOrDefault());
        }
    }
}
