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
    [Order(3)]
    public class FunctionControllerTests
    {
        private readonly ControllerFixture _fixture;
        private readonly IMapper _mapper;

        public FunctionControllerTests(ControllerFixture controllerFixture)
        {
            _fixture = controllerFixture;
            _mapper = controllerFixture.Mapper;
        }

        private async Task<ListResponseResult<FunctionView>> SearchViewFunctions(string functionId)
        {
            var filter = new Filter
                         {
                             CombinedBy = BinaryOperator.And,
                             Definition = new List<Definitions>
                                          {
                                              new Definitions
                                              {
                                                  FieldName = nameof(FunctionBasic.Id),
                                                  BinaryOperator = BinaryOperator.And,
                                                  Operator = FilterOperator.Equals,
                                                  Values = new string[1] { functionId }
                                              }
                                          }
                         };

            string filterStr = new FilterUtility().Serialize(filter);

            HttpResponseMessage response = await _fixture.Client.GetAsync($"functions/view?filter={filterStr}");

            response.EnsureSuccessStatusCode();

            (bool success, ListResponseResult<FunctionView> functions) =
                await response.TryParseContent<ListResponseResult<FunctionView>>();

            Assert.True(success);

            return functions;
        }

        private async Task<ListResponseResult<FunctionBasic>> GetFunctions(string functionId)
        {
            var filter = new Filter
                         {
                             CombinedBy = BinaryOperator.And,
                             Definition = new List<Definitions>
                                          {
                                              new Definitions
                                              {
                                                  FieldName = nameof(FunctionBasic.Id),
                                                  BinaryOperator = BinaryOperator.And,
                                                  Operator = FilterOperator.Equals,
                                                  Values = new string[1] { functionId }
                                              }
                                          }
                         };

            string filterStr = new FilterUtility().Serialize(filter);

            HttpResponseMessage response = await _fixture.Client.GetAsync($"functions?filter={filterStr}");

            response.EnsureSuccessStatusCode();

            (bool success, ListResponseResult<FunctionBasic> functions) =
                await response.TryParseContent<ListResponseResult<FunctionBasic>>();

            Assert.True(success);

            return functions;
        }

        private async Task<FunctionBasic> GetFunction(string functionId)
        {
            HttpResponseMessage response = await _fixture.Client.GetAsync($"functions/{functionId}");

            response.EnsureSuccessStatusCode();

            (bool success, FunctionBasic function) = await response.TryParseContent<FunctionBasic>();

            Assert.True(success);

            return function;
        }

        private void Equal(
            string id,
            CreateFunctionRequest functionRequest,
            FunctionBasic function,
            int roleIndex,
            int organizationIndex)
        {
            var functionUpdateRequest = new FunctionModifiableProperties
                                        {
                                            OrganizationId = functionRequest.OrganizationId,
                                            RoleId = functionRequest.RoleId
                                        };

            Equal(id, functionUpdateRequest, function, roleIndex, organizationIndex);

            functionRequest.ExternalIds.Should().BeEquivalentTo(function.ExternalIds);
        }

        private void Equal(
            string id,
            FunctionModifiableProperties functionRequest,
            FunctionView function,
            int roleIndex,
            int organizationIndex)
        {
            Equal(id, functionRequest, (FunctionBasic)function, roleIndex, organizationIndex);
        }

        private void Equal(
            string id,
            FunctionModifiableProperties functionRequest,
            FunctionBasic function,
            int roleIndex,
            int organizationIndex)
        {
            Assert.Equal(id, function.Id);

            Assert.NotEmpty(function.OrganizationId);
            Assert.Equal(functionRequest.OrganizationId, function.OrganizationId);

            Assert.NotNull(function.Organization);
            Assert.Equal(functionRequest.OrganizationId, function.Organization.Id);
            var organization = _fixture.TestData.Get<Organization>(
                TestDataKeyUtility.GenerateKey(ObjectType.Organization, ObjectOperation.Object, organizationIndex));

            // TODO: Don't ignore
            organization.Should()
                        .BeEquivalentTo(
                            function.Organization,
                            t => t
                                 .Excluding(s => s.ImageUrl)
                                 .Excluding(s => s.UpdatedAt)
                                 .Excluding(s => s.TagUrl));

            Assert.NotEmpty(function.RoleId);
            Assert.Equal(functionRequest.RoleId, function.RoleId);
            var role = _fixture.TestData.Get<RoleBasic>(
                TestDataKeyUtility.GenerateKey(ObjectType.Role, ObjectOperation.Object, roleIndex));

            // TODO: Don't ignore
            role.Should().BeEquivalentTo(function.Role, t => t.Excluding(s => s.UpdatedAt));

            Assert.NotNull(function.Role);
            Assert.Equal(functionRequest.RoleId, function.Role.Id);

            Assert.Equal("Api", function.Source);

            Assert.NotEqual(DateTime.MinValue, function.CreatedAt);
            Assert.NotEqual(DateTime.MinValue, function.UpdatedAt);

            Assert.Null(function.SynchronizedAt);
        }

#region Create Function

        [Fact]
        [Order(1)]
        public async Task A_CreateFunctionAsync_Success()
        {
            var roleId =
                _fixture.TestData.Get<string>(TestDataKeyUtility.GenerateKey(ObjectType.Role, ObjectOperation.Key));
            var organizationId = _fixture.TestData.Get<string>(
                TestDataKeyUtility.GenerateKey(ObjectType.Organization, ObjectOperation.Key));

            CreateFunctionRequest functionRequest = MockDataGeneratorRequests.CreateFunction(roleId, organizationId);

            HttpResponseMessage response = await _fixture.Client.PostAsync(
                "functions",
                functionRequest.ToJsonContent());

            (bool success, string functionId) = await response.WaitForSuccessAsync(_fixture.Client);

            Assert.True(success);

            _fixture.TestData.Add(
                TestDataKeyUtility.GenerateKey(ObjectType.Function, ObjectOperation.Key),
                functionId);
            _fixture.TestData.Add(
                TestDataKeyUtility.GenerateKey(ObjectType.Function, ObjectOperation.Create),
                functionRequest);
        }

        [Fact]
        [Order(2)]
        public async Task B_GetFunction_AfterCreateFunction_Success()
        {
            var functionId = _fixture.TestData.Get<string>(
                TestDataKeyUtility.GenerateKey(ObjectType.Function, ObjectOperation.Key));
            var functionRequest = _fixture.TestData.Get<CreateFunctionRequest>(
                TestDataKeyUtility.GenerateKey(ObjectType.Function, ObjectOperation.Create));

            FunctionBasic function = await GetFunction(functionId);

            Equal(functionId, functionRequest, function, 0, 0);
        }

        [Fact]
        [Order(2)]
        public async Task B_GetFunctions_AfterCreateFunction_Success()
        {
            var functionId = _fixture.TestData.Get<string>(
                TestDataKeyUtility.GenerateKey(ObjectType.Function, ObjectOperation.Key));
            var functionRequest = _fixture.TestData.Get<CreateFunctionRequest>(
                TestDataKeyUtility.GenerateKey(ObjectType.Function, ObjectOperation.Create));

            ListResponseResult<FunctionBasic> functions = await GetFunctions(functionId);

            FunctionBasic function = Assert.Single(functions.Result);

            Equal(functionId, functionRequest, function, 0, 0);
        }

        [Fact]
        [Order(3)]
        public async Task C_SearchViewFunctions_AfterCreateFunction_Success()
        {
            var functionId = _fixture.TestData.Get<string>(
                TestDataKeyUtility.GenerateKey(ObjectType.Function, ObjectOperation.Key));
            var functionRequest = _fixture.TestData.Get<CreateFunctionRequest>(
                TestDataKeyUtility.GenerateKey(ObjectType.Function, ObjectOperation.Create));

            ListResponseResult<FunctionView> functions = await SearchViewFunctions(functionId);

            FunctionView function = Assert.Single(functions.Result);

            Equal(functionId, functionRequest, function, 0, 0);
        }

#endregion
    }
}
