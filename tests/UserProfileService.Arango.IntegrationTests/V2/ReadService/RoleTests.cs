using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;
using UserProfileService.Arango.IntegrationTests.V2.Abstractions;
using UserProfileService.Arango.IntegrationTests.V2.Extensions;
using UserProfileService.Arango.IntegrationTests.V2.Fixtures;
using UserProfileService.Arango.IntegrationTests.V2.Helpers;
using UserProfileService.Common.Tests.Utilities.Comparers;
using UserProfileService.Common.V2.Abstractions;
using Xunit;
using Xunit.Abstractions;

namespace UserProfileService.Arango.IntegrationTests.V2.ReadService
{
    [Collection(nameof(DatabaseCollection))]
    public class RoleTests : ReadTestBase
    {
        public RoleTests(
            DatabaseFixture fixture,
            ITestOutputHelper output) : base(fixture, output)
        {
        }

        [Theory]
        [MemberData(nameof(GetRolesOfProfileTestArguments))]
        public async Task GetRolesOfProfiles(
            string profileId,
            AssignmentQueryObject options,
            Type expectedExceptionType)
        {
            IReadService service = await Fixture.GetReadServiceAsync();

            if (expectedExceptionType != null)
            {
                await Assert.ThrowsAsync(
                    expectedExceptionType,
                    () => service.GetRolesOfProfileAsync(profileId, options));

                return;
            }

            IPaginatedList<LinkedRoleObject> roles = await service.GetRolesOfProfileAsync(profileId, options);

            IPaginatedList<LinkedRoleObject> referenceValues = Fixture
                .GetTestRoles()
                .Where(func => func.LinkedProfiles != null && func.LinkedProfiles.Any(lp => lp.Id.EndsWith(profileId)))
                .Select(Mapper.Map<LinkedRoleObject>)
                .UsingQueryOptions(options);

            Assert.NotNull(roles);
            Assert.Equal(referenceValues.TotalAmount, roles.TotalAmount);
            Assert.Equal(referenceValues.Count, roles.Count);

            // if no valid order options had been set, the default order of ArangoDb and C# memory might be different
            // for this case the comparing of result sets makes no sense, especially if the sets are paginated.
            if (PropertyNameValidFor<RoleBasic>(options?.OrderedBy))
            {
                Assert.Equal(
                    referenceValues,
                    roles,
                    new TestingEqualityComparerForLinkedRoleObject(Output));
            }
        }

        [Theory]
        [MemberData(nameof(GetRolesTestArguments))]
        public async Task GetRolesAsBasic_shouldWork(QueryObject options)
        {
            IReadService service = await Fixture.GetReadServiceAsync();

            IPaginatedList<RoleBasic> roles = await service.GetRolesAsync<RoleBasic>(options);

            IPaginatedList<RoleBasic> referenceValues = Fixture.GetTestRoles()
                .Select(Mapper.Map<RoleBasic>)
                .UsingQueryOptions(
                    options,
                    QueryObjectHelpers.CheckRoleSearchableProperties);

            Assert.Equal(referenceValues.TotalAmount, roles.TotalAmount);
            Assert.Equal(referenceValues.Count, roles.Count);

            // Collections can only be compared directly, if they pre-sorted (backend default sorting methods may differ)
            // If they are not, that should be done before comparing that inside the test.
            // But that will be only possible, if the whole result set is available.
            // Due of pagination settings, it can be different.
            if (string.IsNullOrEmpty(options?.OrderedBy) && roles.TotalAmount == roles.Count)
            {
                Assert.Equal(
                    referenceValues.OrderBy(v => v.Id),
                    roles.OrderBy(r => r.Id),
                    new TestingEqualityComparerForRoleBasic(Output));
            }
            else if (!string.IsNullOrEmpty(options?.OrderedBy))
            {
                Assert.Equal(referenceValues, roles, new TestingEqualityComparerForRoleBasic(Output));
            }
        }

        [Theory]
        [MemberData(nameof(GetRolesTestArguments))]
        public async Task GetRolesAsView_shouldWork(QueryObject options)
        {
            IReadService service = await Fixture.GetReadServiceAsync();

            IPaginatedList<RoleView> roles = await service.GetRolesAsync<RoleView>(options);

            IPaginatedList<RoleView> referenceValues = Fixture.GetTestRoles()
                .Select(Mapper.Map<RoleView>)
                .UsingQueryOptions(options, QueryObjectHelpers.CheckRoleSearchableProperties);

            Assert.Equal(referenceValues.TotalAmount, roles.TotalAmount);
            Assert.Equal(referenceValues.Count, roles.Count);

            // Collections can only be compared directly, if they pre-sorted (backend default sorting methods may differ)
            // If they are not, that should be done before comparing that inside the test.
            // But that will be only possible, if the whole result set is available.
            // Due of pagination settings, it can be different.
            if (string.IsNullOrEmpty(options?.OrderedBy) && roles.TotalAmount == roles.Count)
            {
                Assert.Equal(
                    referenceValues.OrderBy(v => v.Id),
                    roles.OrderBy(r => r.Id),
                    new TestingEqualityComparerForRoleView(Output));
            }
            else if (!string.IsNullOrEmpty(options?.OrderedBy))
            {
                Assert.Equal(referenceValues, roles, new TestingEqualityComparerForRoleView(Output));
            }
        }

        [Theory]
        [MemberData(nameof(GetRoleTestArguments))]
        public async Task GetRole_shouldWork(string roleId)
        {
            IReadService service = await Fixture.GetReadServiceAsync();

            RoleView role = await service.GetRoleAsync(roleId);

            var referenceValue = Mapper.Map<RoleView>(
                Fixture.GetTestRoles()
                    .First(r => r.Id == roleId));

            Assert.NotNull(role);
            Assert.Equal(referenceValue, role, new TestingEqualityComparerForRoleView(Output));
        }

        public static IEnumerable<object[]> GetRolesOfProfileTestArguments()
        {
            return TestArgumentsHelper.GetFunctionAndRoleOfProfileArgumentParts()
                .Concat(GetInvalidIds(0, 1, 3))
                .SelectMany(
                    obj =>
                        GetQueryObjectsAndExceptionTypes<RoleBasic>(obj.ElementAtOrDefault(2) as string)
                            .Select(
                                options =>
                                    new[]
                                    {
                                        obj[0],
                                        options.options,
                                        GetExpectedExceptionType(
                                            obj[1] as Type,
                                            options.expectedExceptionType,
                                            options.prio,
                                            obj.ElementAtOrDefault(3))
                                    }));
        }

        public static IEnumerable<object[]> GetRolesTestArguments()
        {
            return GetQueryObjectsAndExceptionTypes<RoleBasic>(defaultOrderBy: "Description")
                .Where(o => o.expectedExceptionType == null)
                .Select(o => new object[] { o.options });
        }

        public static IEnumerable<object[]> GetRoleTestArguments()
        {
            return DatabaseFixture.TestRoles
                .Take(3)
                .Select(r => new object[] { r.Id });
        }
    }

    public class StringPropertyComparer<TEntity> : IEqualityComparer<TEntity> where TEntity : class
    {
        private readonly Func<TEntity, string> _propertySelector;

        public StringPropertyComparer(Func<TEntity, string> propertySelector)
        {
            _propertySelector = propertySelector;
        }

        public bool Equals(TEntity x, TEntity y)
        {
            return (x == null && y == null)
                || (x != null && y != null && _propertySelector.Invoke(x) == _propertySelector.Invoke(y));
        }

        public int GetHashCode(TEntity obj)
        {
            return _propertySelector.Invoke(obj)?.GetHashCode() ?? 0;
        }
    }
}
