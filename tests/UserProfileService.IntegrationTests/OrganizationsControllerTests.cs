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
    public class OrganizationsControllerTests
    {
        private readonly ControllerFixture _fixture;
        private readonly IMapper _mapper;

        public OrganizationsControllerTests(ControllerFixture controllerFixture)
        {
            _fixture = controllerFixture;
            _mapper = controllerFixture.Mapper;
        }

        private async Task<ListResponseResult<OrganizationBasic>> GetRootsOrganizations(string organizationId)
        {
            var filter = new Filter
                         {
                             CombinedBy = BinaryOperator.And,
                             Definition = new List<Definitions>
                                          {
                                              new Definitions
                                              {
                                                  FieldName = nameof(Organization.Id),
                                                  BinaryOperator = BinaryOperator.And,
                                                  Operator = FilterOperator.Equals,
                                                  Values = new string[1] { organizationId }
                                              }
                                          }
                         };

            string filterStr = new FilterUtility().Serialize(filter);

            HttpResponseMessage response = await _fixture.Client.GetAsync($"organizations/roots?filter={filterStr}");

            response.EnsureSuccessStatusCode();

            (bool success, ListResponseResult<OrganizationBasic> organizations) =
                await response.TryParseContent<ListResponseResult<OrganizationBasic>>();

            Assert.True(success);

            return organizations;
        }

        private async Task<ListResponseResult<OrganizationView>> SearchViewOrganizations(string organizationId)
        {
            var filter = new Filter
                         {
                             CombinedBy = BinaryOperator.And,
                             Definition = new List<Definitions>
                                          {
                                              new Definitions
                                              {
                                                  FieldName = nameof(Organization.Id),
                                                  BinaryOperator = BinaryOperator.And,
                                                  Operator = FilterOperator.Equals,
                                                  Values = new string[1] { organizationId }
                                              }
                                          }
                         };

            string filterStr = new FilterUtility().Serialize(filter);

            HttpResponseMessage response = await _fixture.Client.GetAsync($"organizations/view?filter={filterStr}");

            response.EnsureSuccessStatusCode();

            (bool success, ListResponseResult<OrganizationView> organizations) =
                await response.TryParseContent<ListResponseResult<OrganizationView>>();

            Assert.True(success);

            return organizations;
        }

        private async Task<ListResponseResult<OrganizationBasic>> GetOrganizations(string organizationId)
        {
            var filter = new Filter
                         {
                             CombinedBy = BinaryOperator.And,
                             Definition = new List<Definitions>
                                          {
                                              new Definitions
                                              {
                                                  FieldName = nameof(Organization.Id),
                                                  BinaryOperator = BinaryOperator.And,
                                                  Operator = FilterOperator.Equals,
                                                  Values = new string[1] { organizationId }
                                              }
                                          }
                         };

            string filterStr = new FilterUtility().Serialize(filter);

            HttpResponseMessage response = await _fixture.Client.GetAsync($"organizations?filter={filterStr}");

            response.EnsureSuccessStatusCode();

            (bool success, ListResponseResult<OrganizationBasic> organizations) =
                await response.TryParseContent<ListResponseResult<OrganizationBasic>>();

            Assert.True(success);

            return organizations;
        }

        private async Task<Organization> GetOrganization(string organizationId)
        {
            HttpResponseMessage response = await _fixture.Client.GetAsync($"organizations/{organizationId}");

            response.EnsureSuccessStatusCode();

            (bool success, Organization organization) = await response.TryParseContent<Organization>();

            Assert.True(success);

            return organization;
        }

        private async Task<OrganizationBasic> SearchOrganizations(string organizationId)
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
                                                               FieldName = nameof(Organization.Id),
                                                               BinaryOperator = BinaryOperator.And,
                                                               Operator = FilterOperator.Equals,
                                                               Values = new string[1] { organizationId }
                                                           }
                                                       }
                                      }
                         };

            StringContent filterContent = filter.ToJsonContent();

            HttpResponseMessage response = await _fixture.Client.PostAsync("organizations/search", filterContent);

            response.EnsureSuccessStatusCode();

            (bool success, ListResponseResult<OrganizationBasic> organizations) =
                await response.TryParseContent<ListResponseResult<OrganizationBasic>>();

            Assert.True(success);

            OrganizationBasic organization = Assert.Single(organizations.Result);

            return organization;
        }

        private void Equal(string id, CreateOrganizationRequest organizationRequest, Organization organization)
        {
            Equal(id, organizationRequest, (OrganizationBasic)organization);

            Assert.Empty(organization.MemberOf);
            Assert.NotEmpty(organization.CustomPropertyUrl);
        }

        private void Equal(string id, CreateOrganizationRequest organizationRequest, OrganizationBasic organization)
        {
            var organizationUpdateRequest = new OrganizationModifiableProperties
                                            {
                                                DisplayName = organizationRequest.DisplayName,
                                                IsSystem = organizationRequest.IsSystem,
                                                Weight = organizationRequest.Weight
                                            };

            Equal(id, organizationUpdateRequest, organization);

            Assert.Equal(organizationRequest.Name, organization.Name);
        }

        private void Equal(
            string id,
            OrganizationModifiableProperties organizationRequest,
            OrganizationView organization)
        {
            Equal(id, organizationRequest, (OrganizationBasic)organization);

            Assert.Empty(organization.Tags);
        }

        private void Equal(
            string id,
            OrganizationModifiableProperties organizationRequest,
            OrganizationBasic organization)
        {
            Assert.Equal(id, organization.Id);

            Assert.Equal(organizationRequest.DisplayName, organization.DisplayName);
            Assert.Equal(organizationRequest.IsSystem, organization.IsSystem);
            Assert.Equal(organizationRequest.Weight, organization.Weight);

            Assert.Equal("Api", organization.Source);

            Assert.NotEqual(DateTime.MinValue, organization.CreatedAt);
            Assert.NotEqual(DateTime.MinValue, organization.UpdatedAt);

            Assert.NotEmpty(organization.ExternalIds);
            Assert.NotEmpty(organization.TagUrl);
            Assert.NotEmpty(organization.ImageUrl);

            Assert.Null(organization.SynchronizedAt);
        }

#region Create Organization

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [Order(1)]
        public async Task A_CreateOrganizationAsync_Success(int index)
        {
            CreateOrganizationRequest organizationRequest = MockDataGeneratorRequests.CreateOrganization();

            HttpResponseMessage response = await _fixture.Client.PostAsync(
                "organizations",
                organizationRequest.ToJsonContent());

            (bool success, string organizationId) = await response.WaitForSuccessAsync(_fixture.Client);

            Assert.True(success);

            _fixture.TestData.Add(
                TestDataKeyUtility.GenerateKey(ObjectType.Organization, ObjectOperation.Key, index),
                organizationId);
            _fixture.TestData.Add(
                TestDataKeyUtility.GenerateKey(ObjectType.Organization, ObjectOperation.Create, index),
                organizationRequest);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [Order(2)]
        public async Task B_GetOrganization_AfterCreateOrganization_Success(int index)
        {
            var organizationId = _fixture.TestData.Get<string>(
                TestDataKeyUtility.GenerateKey(ObjectType.Organization, ObjectOperation.Key));
            var organizationRequest = _fixture.TestData.Get<CreateOrganizationRequest>(
                TestDataKeyUtility.GenerateKey(ObjectType.Organization, ObjectOperation.Create));

            Organization organization = await GetOrganization(organizationId);

            Equal(organizationId, organizationRequest, organization);

            _fixture.TestData.Add(
                TestDataKeyUtility.GenerateKey(ObjectType.Organization, ObjectOperation.Object, index),
                organization);
        }

        [Fact]
        [Order(2)]
        public async Task B_GetOrganizations_AfterCreateOrganization_Success()
        {
            var organizationId = _fixture.TestData.Get<string>(
                TestDataKeyUtility.GenerateKey(ObjectType.Organization, ObjectOperation.Key));
            var organizationRequest = _fixture.TestData.Get<CreateOrganizationRequest>(
                TestDataKeyUtility.GenerateKey(ObjectType.Organization, ObjectOperation.Create));

            ListResponseResult<OrganizationBasic> organizations = await GetOrganizations(organizationId);

            OrganizationBasic organization = Assert.Single(organizations.Result);

            Equal(organizationId, organizationRequest, organization);
        }

        [Fact]
        [Order(2)]
        public async Task B_GetRootOrganizations_AfterCreateOrganization_Success()
        {
            var organizationId = _fixture.TestData.Get<string>(
                TestDataKeyUtility.GenerateKey(ObjectType.Organization, ObjectOperation.Key));
            var organizationRequest = _fixture.TestData.Get<CreateOrganizationRequest>(
                TestDataKeyUtility.GenerateKey(ObjectType.Organization, ObjectOperation.Create));

            ListResponseResult<OrganizationBasic> organizations = await GetRootsOrganizations(organizationId);

            OrganizationBasic organization = Assert.Single(organizations.Result);

            Equal(organizationId, organizationRequest, organization);
        }

        [Fact]
        [Order(2)]
        public async Task B_SearchOrganizations_AfterCreateOrganization_Success()
        {
            var organizationId = _fixture.TestData.Get<string>(
                TestDataKeyUtility.GenerateKey(ObjectType.Organization, ObjectOperation.Key));
            var organizationRequest = _fixture.TestData.Get<CreateOrganizationRequest>(
                TestDataKeyUtility.GenerateKey(ObjectType.Organization, ObjectOperation.Create));

            OrganizationBasic organization = await SearchOrganizations(organizationId);

            Equal(organizationId, organizationRequest, organization);
        }

        [Fact]
        [Order(2)]
        public async Task C_SearchViewOrganizations_AfterCreateOrganization_Success()
        {
            var organizationId = _fixture.TestData.Get<string>(
                TestDataKeyUtility.GenerateKey(ObjectType.Organization, ObjectOperation.Key));
            var organizationRequest = _fixture.TestData.Get<CreateOrganizationRequest>(
                TestDataKeyUtility.GenerateKey(ObjectType.Organization, ObjectOperation.Create));

            ListResponseResult<OrganizationView> organizations = await SearchViewOrganizations(organizationId);

            OrganizationView organization = Assert.Single(organizations.Result);

            Equal(organizationId, organizationRequest, organization);
        }

#endregion

#region Update Organization

        [Fact]
        [Order(3)]
        public async Task D_UpdateOrganization_Success()
        {
            var organizationId = _fixture.TestData.Get<string>(
                TestDataKeyUtility.GenerateKey(ObjectType.Organization, ObjectOperation.Key));
            var organizationCreateRequest =
                _fixture.TestData.Get<CreateOrganizationRequest>(
                    TestDataKeyUtility.GenerateKey(ObjectType.Organization, ObjectOperation.Create));

            var organizationUpdateRequest = new OrganizationModifiableProperties
                                            {
                                                IsSystem = !organizationCreateRequest.IsSystem,
                                                Weight = organizationCreateRequest.Weight * 2,
                                                DisplayName = $"{organizationCreateRequest.DisplayName} Edit"
                                            };

            HttpResponseMessage response = await _fixture.Client.PutAsync(
                $"organizations/{organizationId}",
                organizationUpdateRequest.ToJsonContent());

            (bool success, _) = await response.WaitForSuccessAsync(_fixture.Client);

            Assert.True(success);

            _fixture.TestData.Add(
                TestDataKeyUtility.GenerateKey(ObjectType.Organization, ObjectOperation.Update),
                organizationUpdateRequest);
        }

        [Fact]
        [Order(4)]
        public async Task E_GetOrganization_AfterUpdateOrganization_Success()
        {
            var organizationId = _fixture.TestData.Get<string>(
                TestDataKeyUtility.GenerateKey(ObjectType.Organization, ObjectOperation.Key));
            var organizationRequest =
                _fixture.TestData.Get<OrganizationModifiableProperties>(
                    TestDataKeyUtility.GenerateKey(ObjectType.Organization, ObjectOperation.Update));

            Organization organization = await GetOrganization(organizationId);

            Equal(organizationId, organizationRequest, organization);

            _fixture.TestData[TestDataKeyUtility.GenerateKey(ObjectType.Organization, ObjectOperation.Object)] =
                organization;
        }

        [Fact]
        [Order(4)]
        public async Task E_GetOrganizations_AfterUpdateOrganization_Success()
        {
            var organizationId = _fixture.TestData.Get<string>(
                TestDataKeyUtility.GenerateKey(ObjectType.Organization, ObjectOperation.Key));
            var organizationRequest =
                _fixture.TestData.Get<OrganizationModifiableProperties>(
                    TestDataKeyUtility.GenerateKey(ObjectType.Organization, ObjectOperation.Update));

            ListResponseResult<OrganizationBasic> organizations = await GetOrganizations(organizationId);

            OrganizationBasic organization = Assert.Single(organizations.Result);

            Equal(organizationId, organizationRequest, organization);
        }

        [Fact]
        [Order(4)]
        public async Task E_SearchOrganizations_AfterUpdateOrganization_Success()
        {
            var organizationId = _fixture.TestData.Get<string>(
                TestDataKeyUtility.GenerateKey(ObjectType.Organization, ObjectOperation.Key));
            var organizationRequest =
                _fixture.TestData.Get<OrganizationModifiableProperties>(
                    TestDataKeyUtility.GenerateKey(ObjectType.Organization, ObjectOperation.Update));

            OrganizationBasic organization = await SearchOrganizations(organizationId);

            Equal(organizationId, organizationRequest, organization);
        }

        [Fact]
        [Order(4)]
        public async Task E_SearchViewOrganizations_AfterUpdateOrganization_Success()
        {
            var organizationId = _fixture.TestData.Get<string>(
                TestDataKeyUtility.GenerateKey(ObjectType.Organization, ObjectOperation.Key));
            var organizationRequest =
                _fixture.TestData.Get<OrganizationModifiableProperties>(
                    TestDataKeyUtility.GenerateKey(ObjectType.Organization, ObjectOperation.Update));

            ListResponseResult<OrganizationView> organizations = await SearchViewOrganizations(organizationId);

            OrganizationView organization = Assert.Single(organizations.Result);

            Equal(organizationId, organizationRequest, organization);
        }

        [Fact]
        [Order(4)]
        public async Task E_GetRootOrganizations_AfterUpdateOrganization_Success()
        {
            var organizationId = _fixture.TestData.Get<string>(
                TestDataKeyUtility.GenerateKey(ObjectType.Organization, ObjectOperation.Key));
            var organizationRequest =
                _fixture.TestData.Get<OrganizationModifiableProperties>(
                    TestDataKeyUtility.GenerateKey(ObjectType.Organization, ObjectOperation.Update));

            ListResponseResult<OrganizationBasic> organizations = await GetRootsOrganizations(organizationId);

            OrganizationBasic organization = Assert.Single(organizations.Result);

            Equal(organizationId, organizationRequest, organization);
        }

#endregion
    }
}
