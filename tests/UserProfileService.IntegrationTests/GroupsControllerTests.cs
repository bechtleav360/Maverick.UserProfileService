using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Maverick.UserProfileService.FilterUtility.Implementations;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.Modifiable;
using Maverick.UserProfileService.Models.RequestModels;
using Maverick.UserProfileService.Models.ResponseModels;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UserProfileService.Common.Tests.Utilities.MockDataBuilder;
using UserProfileService.IntegrationTests.Extensions;
using UserProfileService.IntegrationTests.Fixtures;
using UserProfileService.IntegrationTests.Utilities;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.Ordering;

namespace UserProfileService.IntegrationTests
{
    [Collection(nameof(ControllerCollection))]
    [Order(2)]
    public class GroupsControllerTests
    {
        private readonly ControllerFixture _fixture;
        private readonly ITestOutputHelper _output;

        public GroupsControllerTests(ControllerFixture controllerFixture, ITestOutputHelper output)
        {
            _fixture = controllerFixture;
            _output = output;
        }

        private async Task<ListResponseResult<GroupBasic>> GetRootsGroups(string groupId)
        {
            var filter = new Filter
                         {
                             CombinedBy = BinaryOperator.And,
                             Definition = new List<Definitions>
                                          {
                                              new Definitions
                                              {
                                                  FieldName = nameof(Group.Id),
                                                  BinaryOperator = BinaryOperator.And,
                                                  Operator = FilterOperator.Equals,
                                                  Values = new [] { groupId }
                                              }
                                          }
                         };

            string filterStr = new FilterUtility().Serialize(filter);

            HttpResponseMessage response = await _fixture.Client.GetAsync($"groups/roots?filter={filterStr}");

            response.EnsureSuccessStatusCode();

            (bool success, ListResponseResult<GroupBasic> groups) =
                await response.TryParseContent<ListResponseResult<GroupBasic>>();

            Assert.True(success);

            return groups;
        }

        private async Task<ListResponseResult<GroupView>> SearchViewGroups(string groupId)
        {
            var filter = new Filter
                         {
                             CombinedBy = BinaryOperator.And,
                             Definition = new List<Definitions>
                                          {
                                              new Definitions
                                              {
                                                  FieldName = nameof(Group.Id),
                                                  BinaryOperator = BinaryOperator.And,
                                                  Operator = FilterOperator.Equals,
                                                  Values = new [] { groupId }
                                              }
                                          }
                         };

            string filterStr = new FilterUtility().Serialize(filter);

            HttpResponseMessage response = await _fixture.Client.GetAsync($"groups/view?filter={filterStr}");

            response.EnsureSuccessStatusCode();

            (bool success, ListResponseResult<GroupView> groups) =
                await response.TryParseContent<ListResponseResult<GroupView>>();

            Assert.True(success);

            return groups;
        }

        private async Task<ListResponseResult<GroupBasic>> GetGroups(string groupId)
        {
            var filter = new Filter
                         {
                             CombinedBy = BinaryOperator.And,
                             Definition = new List<Definitions>
                                          {
                                              new Definitions
                                              {
                                                  FieldName = nameof(Group.Id),
                                                  BinaryOperator = BinaryOperator.And,
                                                  Operator = FilterOperator.Equals,
                                                  Values = new [] { groupId }
                                              }
                                          }
                         };

            string filterStr = new FilterUtility().Serialize(filter);

            HttpResponseMessage response = await _fixture.Client.GetAsync($"groups?filter={filterStr}");

            response.EnsureSuccessStatusCode();

            (bool success, ListResponseResult<GroupBasic> groups) =
                await response.TryParseContent<ListResponseResult<GroupBasic>>();

            Assert.True(success);

            return groups;
        }

        private async Task<Group> GetGroup(string groupId)
        {
            HttpResponseMessage response = await _fixture.Client.GetAsync($"groups/{groupId}");

            response.EnsureSuccessStatusCode();

            (bool success, Group group) = await response.TryParseContent<Group>();

            Assert.True(success);

            return group;
        }

        private async Task<GroupBasic> SearchGroups(string groupId)
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
                                                               FieldName = nameof(Group.Id),
                                                               BinaryOperator = BinaryOperator.And,
                                                               Operator = FilterOperator.Equals,
                                                               Values = new [] { groupId }
                                                           }
                                                       }
                                      }
                         };

            StringContent filterContent = filter.ToJsonContent();

            HttpResponseMessage response = await _fixture.Client.PostAsync("groups/search", filterContent);

            response.EnsureSuccessStatusCode();

            (bool success, ListResponseResult<GroupBasic> groups) =
                await response.TryParseContent<ListResponseResult<GroupBasic>>();

            Assert.True(success);

            GroupBasic group = Assert.Single(groups.Result);

            return group;
        }

        private async Task Equal(string id, CreateGroupRequest groupRequest, Group group)
        {
            UpsAssert.Equal(id, groupRequest, group);

            Assert.Empty(group.MemberOf);

            Assert.Equal(groupRequest.Members.Count, group.Members.Count);

            Task<Member>[] userMembers = groupRequest.Members.Select(
                                                         async m =>
                                                         {
                                                             User user = await _fixture.Client.GetUser(m.Id);
                                                             return new Member
                                                                    {
                                                                        Conditions = m.Conditions,
                                                                        DisplayName = user.DisplayName,
                                                                        Id = user.Id,
                                                                        Kind = ProfileKind.User,
                                                                        Name = user.Name,
                                                                        ExternalIds = user.ExternalIds
                                                                    };
                                                         })
                                                     .ToArray();

            Member[] results = await Task.WhenAll(userMembers);

            results.Should().BeEquivalentTo(group.Members);
        }

#region Create Group

        [Fact]
        [Order(1)]
        public async Task A_CreateGroupAsync_Success()
        {
            CreateGroupRequest groupRequest = MockDataGeneratorRequests.CreateGroup();

            HttpResponseMessage response = await _fixture.Client.PostAsync("groups", groupRequest.ToJsonContent());

            (bool success, string groupId) = await response.WaitForSuccessAsync(_fixture.Client, _output);

            Assert.True(success);

            _fixture.TestData.Add(TestDataKeyUtility.GenerateKey(ObjectType.Group, ObjectOperation.Key), groupId);
            _fixture.TestData.Add(
                TestDataKeyUtility.GenerateKey(ObjectType.Group, ObjectOperation.Create),
                groupRequest);
        }

        [Fact]
        [Order(1)]
        public async Task A_CreateGroupAsync_WithUsers_Success()
        {
            var userId1 =
                _fixture.TestData.Get<string>(TestDataKeyUtility.GenerateKey(ObjectType.User, ObjectOperation.Key));
            var userId2 = _fixture.TestData.Get<string>(
                TestDataKeyUtility.GenerateKey(ObjectType.User, ObjectOperation.Key, 1));

            var members = new List<ConditionAssignment>
                          {
                              new ConditionAssignment
                              {
                                  Id = userId1
                              },
                              new ConditionAssignment
                              {
                                  Id = userId2
                              }
                          };

            CreateGroupRequest groupRequest = MockDataGeneratorRequests.CreateGroup(members);

            HttpResponseMessage response = await _fixture.Client.PostAsync("groups", groupRequest.ToJsonContent());

            (bool success, string groupId) = await response.WaitForSuccessAsync(_fixture.Client, _output);

            Assert.True(success);

            _fixture.TestData.Add(TestDataKeyUtility.GenerateKey(ObjectType.Group, ObjectOperation.Key, 1), groupId);
            _fixture.TestData.Add(
                TestDataKeyUtility.GenerateKey(ObjectType.Group, ObjectOperation.Create, 1),
                groupRequest);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [Order(2)]
        public async Task B_GetGroup_AfterCreateGroup_Success(int index)
        {
            var groupId =
                _fixture.TestData.Get<string>(
                    TestDataKeyUtility.GenerateKey(ObjectType.Group, ObjectOperation.Key, index));
            var groupRequest = _fixture.TestData.Get<CreateGroupRequest>(
                TestDataKeyUtility.GenerateKey(ObjectType.Group, ObjectOperation.Create, index));

            Group group = await GetGroup(groupId);

            await Equal(groupId, groupRequest, group);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [Order(2)]
        public async Task B_GetGroups_AfterCreateGroup_Success(int index)
        {
            var groupId =
                _fixture.TestData.Get<string>(
                    TestDataKeyUtility.GenerateKey(ObjectType.Group, ObjectOperation.Key, index));
            var groupRequest = _fixture.TestData.Get<CreateGroupRequest>(
                TestDataKeyUtility.GenerateKey(ObjectType.Group, ObjectOperation.Create, index));

            ListResponseResult<GroupBasic> groups = await GetGroups(groupId);

            GroupBasic group = Assert.Single(groups.Result);

            UpsAssert.Equal(groupId, groupRequest, group);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [Order(2)]
        public async Task B_GetRootGroups_AfterCreateGroup_Success(int index)
        {
            var groupId =
                _fixture.TestData.Get<string>(
                    TestDataKeyUtility.GenerateKey(ObjectType.Group, ObjectOperation.Key, index));
            var groupRequest = _fixture.TestData.Get<CreateGroupRequest>(
                TestDataKeyUtility.GenerateKey(ObjectType.Group, ObjectOperation.Create, index));

            ListResponseResult<GroupBasic> groups = await GetRootsGroups(groupId);

            GroupBasic group = Assert.Single(groups.Result);

            UpsAssert.Equal(groupId, groupRequest, group);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [Order(2)]
        public async Task B_SearchGroups_AfterCreateGroup_Success(int index)
        {
            var groupId =
                _fixture.TestData.Get<string>(
                    TestDataKeyUtility.GenerateKey(ObjectType.Group, ObjectOperation.Key, index));
            var groupRequest = _fixture.TestData.Get<CreateGroupRequest>(
                TestDataKeyUtility.GenerateKey(ObjectType.Group, ObjectOperation.Create, index));

            GroupBasic group = await SearchGroups(groupId);

            UpsAssert.Equal(groupId, groupRequest, group);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [Order(3)]
        public async Task C_SearchViewGroups_AfterCreateGroup_Success(int index)
        {
            var groupId =
                _fixture.TestData.Get<string>(
                    TestDataKeyUtility.GenerateKey(ObjectType.Group, ObjectOperation.Key, index));
            var groupRequest = _fixture.TestData.Get<CreateGroupRequest>(
                TestDataKeyUtility.GenerateKey(ObjectType.Group, ObjectOperation.Create, index));

            ListResponseResult<GroupView> groups = await SearchViewGroups(groupId);

            GroupView group = Assert.Single(groups.Result);

            UpsAssert.Equal(groupId, groupRequest, group);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 2)] // Currently not working and will be corrected in the future. 
        [Order(2)]
        public async Task GetChildren_AfterCreateGroup_Success(int index, int expectedChildren)
        {
            var groupId =
                _fixture.TestData.Get<string>(
                    TestDataKeyUtility.GenerateKey(ObjectType.Group, ObjectOperation.Key, index));

            HttpResponseMessage response = await _fixture.Client.GetAsync($"groups/{groupId}/children");

            response.EnsureSuccessStatusCode();

            (bool success, ListResponseResult<ConditionalUser> users) =
                await response.TryParseContent<ListResponseResult<ConditionalUser>>();

            Assert.True(success);

            Assert.Equal(expectedChildren, users.Response.Count);

            foreach (ConditionalUser conditionalUser in users.Result)
            {
                User user = await _fixture.Client.GetUser(conditionalUser.Id);

                UpsAssert.Equal(conditionalUser.Id, user, conditionalUser);
            }
        }

#endregion

#region Update Group

        [Fact]
        [Order(3)]
        public async Task D_UpdateGroup_Success()
        {
            var groupId =
                _fixture.TestData.Get<string>(TestDataKeyUtility.GenerateKey(ObjectType.Group, ObjectOperation.Key));
            var groupCreateRequest = _fixture.TestData.Get<CreateGroupRequest>(
                TestDataKeyUtility.GenerateKey(ObjectType.Group, ObjectOperation.Create));

            var groupUpdateRequest = new GroupModifiableProperties
                                     {
                                         IsSystem = !groupCreateRequest.IsSystem,
                                         Weight = groupCreateRequest.Weight * 2,
                                         DisplayName = $"{groupCreateRequest.DisplayName} Edit"
                                     };

            HttpResponseMessage response = await _fixture.Client.PutAsync(
                $"groups/{groupId}",
                groupUpdateRequest.ToJsonContent());

            (bool success, _) = await response.WaitForSuccessAsync(_fixture.Client, _output);

            Assert.True(success);

            _fixture.TestData.Add(
                TestDataKeyUtility.GenerateKey(ObjectType.Group, ObjectOperation.Update),
                groupUpdateRequest);
        }

        [Fact]
        [Order(4)]
        public async Task E_GetGroup_AfterUpdateGroup_Success()
        {
            var groupId =
                _fixture.TestData.Get<string>(TestDataKeyUtility.GenerateKey(ObjectType.Group, ObjectOperation.Key));
            var groupRequest = _fixture.TestData.Get<GroupModifiableProperties>(
                TestDataKeyUtility.GenerateKey(ObjectType.Group, ObjectOperation.Update));

            Group group = await GetGroup(groupId);

            UpsAssert.Equal(groupId, groupRequest, group);
        }

        [Fact]
        [Order(4)]
        public async Task E_GetGroups_AfterUpdateGroup_Success()
        {
            var groupId =
                _fixture.TestData.Get<string>(TestDataKeyUtility.GenerateKey(ObjectType.Group, ObjectOperation.Key));
            var groupRequest = _fixture.TestData.Get<GroupModifiableProperties>(
                TestDataKeyUtility.GenerateKey(ObjectType.Group, ObjectOperation.Update));

            ListResponseResult<GroupBasic> groups = await GetGroups(groupId);

            GroupBasic group = Assert.Single(groups.Result);

            UpsAssert.Equal(groupId, groupRequest, group);
        }

        [Fact]
        [Order(4)]
        public async Task E_SearchGroups_AfterUpdateGroup_Success()
        {
            var groupId =
                _fixture.TestData.Get<string>(TestDataKeyUtility.GenerateKey(ObjectType.Group, ObjectOperation.Key));
            var groupRequest = _fixture.TestData.Get<GroupModifiableProperties>(
                TestDataKeyUtility.GenerateKey(ObjectType.Group, ObjectOperation.Update));

            GroupBasic group = await SearchGroups(groupId);

            UpsAssert.Equal(groupId, groupRequest, group);
        }

        [Fact]
        [Order(4)]
        public async Task E_SearchViewGroups_AfterUpdateGroup_Success()
        {
            var groupId =
                _fixture.TestData.Get<string>(TestDataKeyUtility.GenerateKey(ObjectType.Group, ObjectOperation.Key));
            var groupRequest = _fixture.TestData.Get<GroupModifiableProperties>(
                TestDataKeyUtility.GenerateKey(ObjectType.Group, ObjectOperation.Update));

            ListResponseResult<GroupView> groups = await SearchViewGroups(groupId);

            GroupView group = Assert.Single(groups.Result);

            UpsAssert.Equal(groupId, groupRequest, group);
        }

        [Fact]
        [Order(4)]
        public async Task E_GetRootGroups_AfterUpdateGroup_Success()
        {
            var groupId =
                _fixture.TestData.Get<string>(TestDataKeyUtility.GenerateKey(ObjectType.Group, ObjectOperation.Key));
            var groupRequest = _fixture.TestData.Get<GroupModifiableProperties>(
                TestDataKeyUtility.GenerateKey(ObjectType.Group, ObjectOperation.Update));

            ListResponseResult<GroupBasic> groups = await GetRootsGroups(groupId);

            GroupBasic group = Assert.Single(groups.Result);

            UpsAssert.Equal(groupId, groupRequest, group);
        }

#endregion

#region SetGroupConfig

        [Fact]
        [Order(3)]
        public async Task F_SetGroupConfig_Success()
        {
            var groupId =
                _fixture.TestData.Get<string>(TestDataKeyUtility.GenerateKey(ObjectType.Group, ObjectOperation.Key));
            string configKey = TestDataKeyUtility.GenerateConfigKey(ObjectType.Group, ObjectOperation.Key);

            var config = @"
                        {
                            'FeatureToggling': {
                                'Features': {
                                    'Module_1c311c78 -c4b5-4865-884b-e9190f6d4083': {
                                        'IsOn': false
                                    },
                                    'Module_4e56d3df-542d-4884-b94e-c84b3f233a3d': {
                                        'IsOn': true
                                    },
                                    'Module_7785ac48-b049-4782-90d4-ce3c3fc288a5': {
                                        'IsOn': true
                                    },
                                    'Module_6287b953-31fd-4bd6-8d95-abfbfbe7affc': {
                                        'IsOn': false
                                    },
                                    'Module_ad8c3761-5514-4f49-a054-96bcae88f3d6': {
                                        'IsOn': false
                                    },
                                    'Module_401aa0e6-3375-4db6-8ebb-026d21a3a6a8': {
                                        'IsOn': false
                                    },
                                    'Module_ec80bfc0-e6a4-48d1-9375-73df60ce3f6c': {
                                        'IsOn': false
                                    },
                                    'MaverickActivated': {
                                        'IsOn': true
                                    }
                                }
                            }
                        }";

            var stringContent = new StringContent(config, Encoding.UTF8, MediaTypeNames.Text.Plain);

            HttpResponseMessage response = await _fixture.Client.PutAsync(
                $"groups/{groupId}/config/{configKey}",
                stringContent);

            (bool success, _) = await response.WaitForSuccessAsync(_fixture.Client, _output);

            Assert.True(success);

            _fixture.TestData.Add(configKey, JsonConvert.DeserializeObject<JObject>(config));
        }

        [Fact]
        [Order(4)]
        public async Task G_GetGroupConfig_Success()
        {
            string configKey = TestDataKeyUtility.GenerateConfigKey(ObjectType.Group, ObjectOperation.Key);
            var groupId =
                _fixture.TestData.Get<string>(TestDataKeyUtility.GenerateKey(ObjectType.Group, ObjectOperation.Key));
            var configObject = _fixture.TestData.Get<JObject>(configKey);

            HttpResponseMessage response = await _fixture.Client.GetAsync($"groups/{groupId}/config/{configKey}");

            response.EnsureSuccessStatusCode();

            (bool success, JObject configResult) = await response.TryParseContent<JObject>();

            Assert.True(success);

            configObject.Should().BeEquivalentTo(configResult);
        }

        [Fact]
        [Order(5)]
        public async Task G_DeleteGroupConfig_Success()
        {
            string configKey = TestDataKeyUtility.GenerateConfigKey(ObjectType.Group, ObjectOperation.Key);
            var groupId =
                _fixture.TestData.Get<string>(TestDataKeyUtility.GenerateKey(ObjectType.Group, ObjectOperation.Key));

            HttpResponseMessage response = await _fixture.Client.DeleteAsync($"groups/{groupId}/config/{configKey}");

            response.EnsureSuccessStatusCode();

            (bool success, _) = await response.WaitForSuccessAsync(_fixture.Client, maxAttempts: 10);

            Assert.False(success);
        }

        [Fact]
        [Order(6)]
        public async Task H_GetGroupConfig_AfterDelete_Success()
        {
            string configKey = TestDataKeyUtility.GenerateConfigKey(ObjectType.Group, ObjectOperation.Key);
            var groupId =
                _fixture.TestData.Get<string>(TestDataKeyUtility.GenerateKey(ObjectType.Group, ObjectOperation.Key));

            HttpResponseMessage response = await _fixture.Client.GetAsync($"groups/{groupId}/config/{configKey}");

            response.EnsureSuccessStatusCode();

            (bool success, JObject configResult) = await response.TryParseContent<JObject>();

            Assert.False(success);
            Assert.Null(configResult);
        }

#endregion
    }
}
