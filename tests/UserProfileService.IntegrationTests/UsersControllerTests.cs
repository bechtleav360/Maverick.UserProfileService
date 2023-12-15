using System.Collections.Generic;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    public class UsersControllerTests
    {
        private readonly ControllerFixture _fixture;
        private readonly IMapper _mapper;

        public UsersControllerTests(ControllerFixture controllerFixture)
        {
            _fixture = controllerFixture;
            _mapper = controllerFixture.Mapper;
        }

        private async Task<ListResponseResult<UserView>> SearchViewUsers(string userId)
        {
            var filter = new Filter
            {
                CombinedBy = BinaryOperator.And,
                Definition = new List<Definitions>
                {
                    new Definitions
                    {
                        FieldName = nameof(User.Id),
                        BinaryOperator = BinaryOperator.And,
                        Operator = FilterOperator.Equals,
                        Values = new string[1] { userId }
                    }
                }
            };

            string filterStr = new FilterUtility().Serialize(filter);

            HttpResponseMessage response = await _fixture.Client.GetAsync($"users/view?filter={filterStr}");

            response.EnsureSuccessStatusCode();

            (bool success, ListResponseResult<UserView> users) =
                await response.TryParseContent<ListResponseResult<UserView>>();

            Assert.True(success);

            return users;
        }

        private async Task<ListResponseResult<UserBasic>> GetUsers(string userId)
        {
            var filter = new Filter
            {
                CombinedBy = BinaryOperator.And,
                Definition = new List<Definitions>
                {
                    new Definitions
                    {
                        FieldName = nameof(User.Id),
                        BinaryOperator = BinaryOperator.And,
                        Operator = FilterOperator.Equals,
                        Values = new string[1] { userId }
                    }
                }
            };

            string filterStr = new FilterUtility().Serialize(filter);

            HttpResponseMessage response = await _fixture.Client.GetAsync($"users?filter={filterStr}");

            response.EnsureSuccessStatusCode();

            (bool success, ListResponseResult<UserBasic> users) =
                await response.TryParseContent<ListResponseResult<UserBasic>>();

            Assert.True(success);

            return users;
        }

        private async Task<UserBasic> SearchUsers(string userId)
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
                            FieldName = nameof(User.Id),
                            BinaryOperator = BinaryOperator.And,
                            Operator = FilterOperator.Equals,
                            Values = new string[1] { userId }
                        }
                    }
                }
            };

            StringContent filterContent = filter.ToJsonContent();

            HttpResponseMessage response = await _fixture.Client.PostAsync("users/search", filterContent);

            response.EnsureSuccessStatusCode();

            (bool success, ListResponseResult<UserBasic> users) =
                await response.TryParseContent<ListResponseResult<UserBasic>>();

            Assert.True(success);

            UserBasic user = Assert.Single(users.Result);

            return user;
        }

        #region Create User

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [Order(1)]
        public async Task A_CreateUserAsync_Success(int index)
        {
            CreateUserRequest userRequest = MockDataGeneratorRequests.CreateUser();

            HttpResponseMessage response = await _fixture.Client.PostAsync("users", userRequest.ToJsonContent());

            (bool success, string userId) = await response.WaitForSuccessAsync(_fixture.Client);

            Assert.True(success);

            _fixture.TestData.Add(TestDataKeyUtility.GenerateKey(ObjectType.User, ObjectOperation.Key, index), userId);

            _fixture.TestData.Add(
                TestDataKeyUtility.GenerateKey(ObjectType.User, ObjectOperation.Create, index),
                userRequest);
        }

        [Fact]
        [Order(2)]
        public async Task B_GetUser_AfterCreateUser_Success()
        {
            var userId =
                _fixture.TestData.Get<string>(TestDataKeyUtility.GenerateKey(ObjectType.User, ObjectOperation.Key));

            var userRequest = _fixture.TestData.Get<CreateUserRequest>(
                TestDataKeyUtility.GenerateKey(ObjectType.User, ObjectOperation.Create));

            User user = await _fixture.Client.GetUser(userId);

            UpsAssert.Equal(userId, userRequest, user);
        }

        [Fact]
        [Order(2)]
        public async Task B_GetUsers_AfterCreateUser_Success()
        {
            var userId =
                _fixture.TestData.Get<string>(TestDataKeyUtility.GenerateKey(ObjectType.User, ObjectOperation.Key));

            var userRequest = _fixture.TestData.Get<CreateUserRequest>(
                TestDataKeyUtility.GenerateKey(ObjectType.User, ObjectOperation.Create));

            ListResponseResult<UserBasic> users = await GetUsers(userId);

            UserBasic user = Assert.Single(users.Result);

            UpsAssert.Equal(userId, userRequest, user);
        }

        [Fact]
        [Order(2)]
        public async Task B_SearchUsers_AfterCreateUser_Success()
        {
            var userId =
                _fixture.TestData.Get<string>(TestDataKeyUtility.GenerateKey(ObjectType.User, ObjectOperation.Key));

            var userRequest = _fixture.TestData.Get<CreateUserRequest>(
                TestDataKeyUtility.GenerateKey(ObjectType.User, ObjectOperation.Create));

            UserBasic user = await SearchUsers(userId);

            UpsAssert.Equal(userId, userRequest, user);
        }

        [Fact]
        [Order(3)]
        public async Task C_SearchViewUsers_AfterCreateUser_Success()
        {
            var userId =
                _fixture.TestData.Get<string>(TestDataKeyUtility.GenerateKey(ObjectType.User, ObjectOperation.Key));

            var userRequest = _fixture.TestData.Get<CreateUserRequest>(
                TestDataKeyUtility.GenerateKey(ObjectType.User, ObjectOperation.Create));

            ListResponseResult<UserView> users = await SearchViewUsers(userId);

            UserView user = Assert.Single(users.Result);

            UpsAssert.Equal(userId, userRequest, user);
        }

        #endregion

        #region Update User

        [Fact]
        [Order(3)]
        public async Task D_UpdateUser_Success()
        {
            var userId =
                _fixture.TestData.Get<string>(TestDataKeyUtility.GenerateKey(ObjectType.User, ObjectOperation.Key));

            var userCreateRequest = _fixture.TestData.Get<CreateUserRequest>(
                TestDataKeyUtility.GenerateKey(ObjectType.User, ObjectOperation.Create));

            var userUpdateRequest = new UserModifiableProperties
            {
                FirstName = $"{userCreateRequest.FirstName} Edit",
                LastName = $"{userCreateRequest.LastName} Edit",
                DisplayName = $"{userCreateRequest.DisplayName} Edit",
                Email = $"{userCreateRequest.Email} Edit",
                UserName = $"{userCreateRequest.UserName} Edit",
                UserStatus = $"{userCreateRequest.UserStatus} Edit"
            };

            HttpResponseMessage response = await _fixture.Client.PutAsync(
                $"users/{userId}",
                userUpdateRequest.ToJsonContent());

            (bool success, _) = await response.WaitForSuccessAsync(_fixture.Client);

            Assert.True(success);

            _fixture.TestData.Add(
                TestDataKeyUtility.GenerateKey(ObjectType.User, ObjectOperation.Update),
                userUpdateRequest);
        }

        [Fact]
        [Order(4)]
        public async Task E_GetUser_AfterUpdateUser_Success()
        {
            var userId =
                _fixture.TestData.Get<string>(TestDataKeyUtility.GenerateKey(ObjectType.User, ObjectOperation.Key));

            var userRequest = _fixture.TestData.Get<UserModifiableProperties>(
                TestDataKeyUtility.GenerateKey(ObjectType.User, ObjectOperation.Update));

            User user = await _fixture.Client.GetUser(userId);

            UpsAssert.Equal(userId, userRequest, user);
        }

        [Fact]
        [Order(4)]
        public async Task E_GetUsers_AfterUpdateUser_Success()
        {
            var userId =
                _fixture.TestData.Get<string>(TestDataKeyUtility.GenerateKey(ObjectType.User, ObjectOperation.Key));

            var userRequest = _fixture.TestData.Get<UserModifiableProperties>(
                TestDataKeyUtility.GenerateKey(ObjectType.User, ObjectOperation.Update));

            ListResponseResult<UserBasic> users = await GetUsers(userId);

            UserBasic user = Assert.Single(users.Result);

            UpsAssert.Equal(userId, userRequest, user);
        }

        [Fact]
        [Order(4)]
        public async Task E_SearchUsers_AfterUpdateUser_Success()
        {
            var userId =
                _fixture.TestData.Get<string>(TestDataKeyUtility.GenerateKey(ObjectType.User, ObjectOperation.Key));

            var userRequest = _fixture.TestData.Get<UserModifiableProperties>(
                TestDataKeyUtility.GenerateKey(ObjectType.User, ObjectOperation.Update));

            UserBasic user = await SearchUsers(userId);

            UpsAssert.Equal(userId, userRequest, user);
        }

        [Fact]
        [Order(4)]
        public async Task E_SearchViewUsers_AfterUpdateUser_Success()
        {
            var userId =
                _fixture.TestData.Get<string>(TestDataKeyUtility.GenerateKey(ObjectType.User, ObjectOperation.Key));

            var userRequest = _fixture.TestData.Get<UserModifiableProperties>(
                TestDataKeyUtility.GenerateKey(ObjectType.User, ObjectOperation.Update));

            ListResponseResult<UserView> users = await SearchViewUsers(userId);

            UserView user = Assert.Single(users.Result);

            UpsAssert.Equal(userId, userRequest, user);
        }

        #endregion

        #region SetUserConfig

        [Fact]
        [Order(3)]
        public async Task F_SetUserConfig_Success()
        {
            var userId =
                _fixture.TestData.Get<string>(TestDataKeyUtility.GenerateKey(ObjectType.User, ObjectOperation.Key));

            string configKey = TestDataKeyUtility.GenerateConfigKey(ObjectType.User, ObjectOperation.Key);

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

            var stringContent = new StringContent(config);

            HttpResponseMessage response = await _fixture.Client.PutAsync(
                $"users/{userId}/config/{configKey}",
                stringContent);

            (bool success, _) = await response.WaitForSuccessAsync(_fixture.Client);

            Assert.True(success);

            _fixture.TestData.Add(configKey, JsonConvert.DeserializeObject<JObject>(config));
        }

        [Fact]
        [Order(4)]
        public async Task G_GetUserConfig_Success()
        {
            string configKey = TestDataKeyUtility.GenerateConfigKey(ObjectType.User, ObjectOperation.Key);

            var userId =
                _fixture.TestData.Get<string>(TestDataKeyUtility.GenerateKey(ObjectType.User, ObjectOperation.Key));

            var configObject = _fixture.TestData.Get<JObject>(configKey);

            HttpResponseMessage response = await _fixture.Client.GetAsync($"users/{userId}/config/{configKey}");

            response.EnsureSuccessStatusCode();

            (bool success, JObject configResult) = await response.TryParseContent<JObject>();

            Assert.True(success);

            configObject.Should().BeEquivalentTo(configResult);
        }

        [Fact]
        [Order(5)]
        public async Task G_DeleteUserConfig_Success()
        {
            string configKey = TestDataKeyUtility.GenerateConfigKey(ObjectType.User, ObjectOperation.Key);

            var userId =
                _fixture.TestData.Get<string>(TestDataKeyUtility.GenerateKey(ObjectType.User, ObjectOperation.Key));

            var configObject = _fixture.TestData.Get<JObject>(configKey);

            HttpResponseMessage response = await _fixture.Client.DeleteAsync($"users/{userId}/config/{configKey}");

            response.EnsureSuccessStatusCode();

            (bool success, _) = await response.WaitForSuccessAsync(_fixture.Client, maxAttempts: 10);

            Assert.True(success);
        }

        [Fact]
        [Order(6)]
        public async Task G_GetUserConfig_AfterDelete_Success()
        {
            string configKey = TestDataKeyUtility.GenerateConfigKey(ObjectType.User, ObjectOperation.Key);

            var userId =
                _fixture.TestData.Get<string>(TestDataKeyUtility.GenerateKey(ObjectType.User, ObjectOperation.Key));

            HttpResponseMessage response = await _fixture.Client.GetAsync($"users/{userId}/config/{configKey}");

            response.EnsureSuccessStatusCode();

            (bool success, JObject configResult) = await response.TryParseContent<JObject>();

            Assert.False(success);
            Assert.Null(configResult);
        }
#endregion

    }
}