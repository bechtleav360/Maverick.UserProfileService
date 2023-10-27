using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using Maverick.UserProfileService.FilterUtility.Implementations;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.Modifiable;
using Maverick.UserProfileService.Models.RequestModels;
using Maverick.UserProfileService.Models.ResponseModels;
using FluentAssertions;
using UserProfileService.Common.Tests.Utilities.MockDataBuilder;
using UserProfileService.IntegrationTests.Extensions;
using UserProfileService.IntegrationTests.Fixtures;
using UserProfileService.IntegrationTests.Utilities;
using Xunit;
using Xunit.Extensions.Ordering;

namespace UserProfileService.IntegrationTests
{
    [Collection(nameof(ControllerCollection))]
    [Order(1)]
    public class RoleControllerTests
    {
        private readonly ControllerFixture _fixture;
        private readonly IMapper _mapper;

        public RoleControllerTests(ControllerFixture controllerFixture)
        {
            _fixture = controllerFixture;
            _mapper = controllerFixture.Mapper;
        }

        private async Task<ListResponseResult<RoleView>> SearchViewRoles(string roleId)
        {
            var filter = new Filter
                         {
                             CombinedBy = BinaryOperator.And,
                             Definition = new List<Definitions>
                                          {
                                              new Definitions
                                              {
                                                  FieldName = nameof(RoleBasic.Id),
                                                  BinaryOperator = BinaryOperator.And,
                                                  Operator = FilterOperator.Equals,
                                                  Values = new string[1] { roleId }
                                              }
                                          }
                         };

            string filterStr = new FilterUtility().Serialize(filter);

            HttpResponseMessage response = await _fixture.Client.GetAsync($"roles/view?filter={filterStr}");

            response.EnsureSuccessStatusCode();

            (bool success, ListResponseResult<RoleView> roles) =
                await response.TryParseContent<ListResponseResult<RoleView>>();

            Assert.True(success);

            return roles;
        }

        private async Task<ListResponseResult<RoleBasic>> GetRoles(string roleId)
        {
            var filter = new Filter
                         {
                             CombinedBy = BinaryOperator.And,
                             Definition = new List<Definitions>
                                          {
                                              new Definitions
                                              {
                                                  FieldName = nameof(RoleBasic.Id),
                                                  BinaryOperator = BinaryOperator.And,
                                                  Operator = FilterOperator.Equals,
                                                  Values = new string[1] { roleId }
                                              }
                                          }
                         };

            string filterStr = new FilterUtility().Serialize(filter);

            HttpResponseMessage response = await _fixture.Client.GetAsync($"roles?filter={filterStr}");

            response.EnsureSuccessStatusCode();

            (bool success, ListResponseResult<RoleBasic> roles) =
                await response.TryParseContent<ListResponseResult<RoleBasic>>();

            Assert.True(success);

            return roles;
        }

        private async Task<RoleBasic> GetRole(string roleId)
        {
            HttpResponseMessage response = await _fixture.Client.GetAsync($"roles/{roleId}");

            response.EnsureSuccessStatusCode();

            (bool success, RoleBasic role) = await response.TryParseContent<RoleBasic>();

            Assert.True(success);

            return role;
        }

        private async Task<RoleBasic> SearchRoles(string roleId)
        {
            var filter = new QueryObject
                         {
                             Filter = new Filter
                                      {
                                          CombinedBy = BinaryOperator.And,
                                          Definition = new List<Definitions>
                                                       {
                                                           new Definitions
                                                           {
                                                               FieldName = nameof(RoleBasic.Id),
                                                               BinaryOperator = BinaryOperator.And,
                                                               Operator = FilterOperator.Equals,
                                                               Values = new string[1] { roleId }
                                                           }
                                                       }
                                      }
                         };

            StringContent filterContent = filter.ToJsonContent();

            HttpResponseMessage response = await _fixture.Client.PostAsync("roles/search", filterContent);

            response.EnsureSuccessStatusCode();

            (bool success, ListResponseResult<RoleBasic> roles) =
                await response.TryParseContent<ListResponseResult<RoleBasic>>();

            Assert.True(success);

            RoleBasic role = Assert.Single(roles.Result);

            return role;
        }

        private void Equal(string id, CreateRoleRequest roleRequest, RoleBasic role)
        {
            var roleUpdateRequest = new RoleModifiableProperties
                                    {
                                        Description = roleRequest.Description,
                                        IsSystem = roleRequest.IsSystem,
                                        DeniedPermissions = roleRequest.DeniedPermissions,
                                        Permissions = roleRequest.Permissions,
                                        Name = roleRequest.Name
                                    };

            Equal(id, roleUpdateRequest, role);

            roleRequest.ExternalIds.Should().BeEquivalentTo(role.ExternalIds);
        }

        private void Equal(string id, RoleModifiableProperties roleRequest, RoleView role)
        {
            Equal(id, roleRequest, (RoleBasic)role);

            Assert.NotEmpty(role.Permissions);
            Assert.NotEmpty(role.DeniedPermissions);
        }

        private void Equal(string id, RoleModifiableProperties roleRequest, RoleBasic role)
        {
            Assert.Equal(id, role.Id);

            Assert.Equal(roleRequest.Description, role.Description);
            Assert.Equal(roleRequest.IsSystem, role.IsSystem);
            Assert.Equal(roleRequest.Name, role.Name);

            roleRequest.Permissions.Should().BeEquivalentTo(role.Permissions);
            roleRequest.DeniedPermissions.Should().BeEquivalentTo(role.DeniedPermissions);

            Assert.Equal("Api", role.Source);

            Assert.NotEqual(DateTime.MinValue, role.CreatedAt);
            Assert.NotEqual(DateTime.MinValue, role.UpdatedAt);

            Assert.Null(role.SynchronizedAt);
        }

#region Create Role

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [Order(1)]
        public async Task A_CreateRoleAsync_Success(int index)
        {
            CreateRoleRequest roleRequest = MockDataGeneratorRequests.CreateRole();

            HttpResponseMessage response = await _fixture.Client.PostAsync("roles", roleRequest.ToJsonContent());

            (bool success, string roleId) = await response.WaitForSuccessAsync(_fixture.Client);

            Assert.True(success);

            _fixture.TestData.Add(TestDataKeyUtility.GenerateKey(ObjectType.Role, ObjectOperation.Key, index), roleId);
            _fixture.TestData.Add(
                TestDataKeyUtility.GenerateKey(ObjectType.Role, ObjectOperation.Create, index),
                roleRequest);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [Order(2)]
        public async Task B_GetRole_AfterCreateRole_Success(int index)
        {
            var roleId =
                _fixture.TestData.Get<string>(
                    TestDataKeyUtility.GenerateKey(ObjectType.Role, ObjectOperation.Key, index));
            var roleRequest = _fixture.TestData.Get<CreateRoleRequest>(
                TestDataKeyUtility.GenerateKey(ObjectType.Role, ObjectOperation.Create, index));

            RoleBasic role = await GetRole(roleId);

            Equal(roleId, roleRequest, role);

            _fixture.TestData.Add(
                TestDataKeyUtility.GenerateKey(ObjectType.Role, ObjectOperation.Object, index),
                role);
        }

        [Fact]
        [Order(2)]
        public async Task B_GetRoles_AfterCreateRole_Success()
        {
            var roleId =
                _fixture.TestData.Get<string>(TestDataKeyUtility.GenerateKey(ObjectType.Role, ObjectOperation.Key));
            var roleRequest = _fixture.TestData.Get<CreateRoleRequest>(
                TestDataKeyUtility.GenerateKey(ObjectType.Role, ObjectOperation.Create));

            ListResponseResult<RoleBasic> roles = await GetRoles(roleId);

            RoleBasic role = Assert.Single(roles.Result);

            Equal(roleId, roleRequest, role);
        }

        [Fact]
        [Order(3)]
        public async Task C_SearchViewRoles_AfterCreateRole_Success()
        {
            var roleId =
                _fixture.TestData.Get<string>(TestDataKeyUtility.GenerateKey(ObjectType.Role, ObjectOperation.Key));
            var roleRequest = _fixture.TestData.Get<CreateRoleRequest>(
                TestDataKeyUtility.GenerateKey(ObjectType.Role, ObjectOperation.Create));

            ListResponseResult<RoleView> roles = await SearchViewRoles(roleId);

            RoleView role = Assert.Single(roles.Result);

            Equal(roleId, roleRequest, role);
        }

#endregion

#region Update Role

        [Fact]
        [Order(3)]
        public async Task D_UpdateRole_Success()
        {
            var roleId =
                _fixture.TestData.Get<string>(TestDataKeyUtility.GenerateKey(ObjectType.Role, ObjectOperation.Key));
            var roleCreateRequest = _fixture.TestData.Get<CreateRoleRequest>(
                TestDataKeyUtility.GenerateKey(ObjectType.Role, ObjectOperation.Create));

            CreateRoleRequest newRoleValues = MockDataGeneratorRequests.CreateRole();

            var roleUpdateRequest = new RoleModifiableProperties
                                    {
                                        Name = $"{roleCreateRequest.Name} Edit",
                                        DeniedPermissions = newRoleValues.DeniedPermissions,
                                        IsSystem = !roleCreateRequest.IsSystem,
                                        Description = $"{roleCreateRequest.Description} Edit",
                                        Permissions = newRoleValues.Permissions
                                    };

            HttpResponseMessage response = await _fixture.Client.PutAsync(
                $"roles/{roleId}",
                roleUpdateRequest.ToJsonContent());

            (bool success, _) = await response.WaitForSuccessAsync(_fixture.Client);

            Assert.True(success);

            _fixture.TestData.Add(
                TestDataKeyUtility.GenerateKey(ObjectType.Role, ObjectOperation.Update),
                roleUpdateRequest);
        }

        [Fact]
        [Order(4)]
        public async Task E_GetRole_AfterUpdateRole_Success()
        {
            var roleId =
                _fixture.TestData.Get<string>(TestDataKeyUtility.GenerateKey(ObjectType.Role, ObjectOperation.Key));
            var roleRequest = _fixture.TestData.Get<RoleModifiableProperties>(
                TestDataKeyUtility.GenerateKey(ObjectType.Role, ObjectOperation.Update));

            RoleBasic role = await GetRole(roleId);

            Equal(roleId, roleRequest, role);

            _fixture.TestData[TestDataKeyUtility.GenerateKey(ObjectType.Role, ObjectOperation.Object)] = role;
        }

        [Fact]
        [Order(4)]
        public async Task E_GetRoles_AfterUpdateRole_Success()
        {
            var roleId =
                _fixture.TestData.Get<string>(TestDataKeyUtility.GenerateKey(ObjectType.Role, ObjectOperation.Key));
            var roleRequest = _fixture.TestData.Get<RoleModifiableProperties>(
                TestDataKeyUtility.GenerateKey(ObjectType.Role, ObjectOperation.Update));

            ListResponseResult<RoleBasic> roles = await GetRoles(roleId);

            RoleBasic role = Assert.Single(roles.Result);

            Equal(roleId, roleRequest, role);
        }

        [Fact]
        [Order(4)]
        public async Task E_SearchViewRoles_AfterUpdateRole_Success()
        {
            var roleId =
                _fixture.TestData.Get<string>(TestDataKeyUtility.GenerateKey(ObjectType.Role, ObjectOperation.Key));
            var roleRequest = _fixture.TestData.Get<RoleModifiableProperties>(
                TestDataKeyUtility.GenerateKey(ObjectType.Role, ObjectOperation.Update));

            ListResponseResult<RoleView> roles = await SearchViewRoles(roleId);

            RoleView role = Assert.Single(roles.Result);

            Equal(roleId, roleRequest, role);
        }

#endregion
    }
}
