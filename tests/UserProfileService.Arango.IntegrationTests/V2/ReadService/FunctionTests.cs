using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Arango.IntegrationTests.V2.Abstractions;
using UserProfileService.Arango.IntegrationTests.V2.Extensions;
using UserProfileService.Arango.IntegrationTests.V2.Fixtures;
using UserProfileService.Arango.IntegrationTests.V2.Helpers;
using UserProfileService.Arango.IntegrationTests.V2.ReadService.TestData;
using UserProfileService.Common.Tests.Utilities.Comparers;
using UserProfileService.Common.Tests.Utilities.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Exceptions;
using Xunit;
using Xunit.Abstractions;

namespace UserProfileService.Arango.IntegrationTests.V2.ReadService;

[Collection(nameof(DatabaseCollection))]
public class FunctionTests : ReadTestBase
{
    public FunctionTests(
        DatabaseFixture fixture,
        ITestOutputHelper output) : base(fixture, output)
    {
    }

    [Theory]
    [MemberData(nameof(GetFunctionsOfProfileRecursiveFalseTestArguments))]
    public async Task GetFunctionsOfProfileRecursiveFalse(
        string profileId,
        AssignmentQueryObject options,
        Type expectedExceptionType)
    {
        IReadService service = await Fixture.GetReadServiceAsync();

        if (expectedExceptionType != null)
        {
            await Assert.ThrowsAsync(
                expectedExceptionType,
                () => service.GetFunctionsOfProfileAsync(profileId, false, options));

            return;
        }

        IPaginatedList<LinkedFunctionObject> functions =
            await service.GetFunctionsOfProfileAsync(profileId, false, options);

        List<FunctionObjectEntityModel> temp = Fixture
            .GetTestFunctions()
            .Where(
                func => func.LinkedProfiles != null
                    && func.LinkedProfiles.Any(lp => lp.Id.EndsWith(profileId)))
            .ToList();

        IPaginatedList<LinkedFunctionObject> referenceValues = temp
            .Select(Mapper.Map<LinkedFunctionObject>)
            .UsingQueryOptions(options);

        Assert.NotNull(functions);
        Assert.Equal(referenceValues.TotalAmount, functions.TotalAmount);
        Assert.Equal(referenceValues.Count, functions.Count);

        // if no valid order options had been set, the default order of ArangoDb and C# memory might be different
        // for this case the comparing of result sets makes no sense, especially if the sets are paginated.
        if (PropertyNameValidFor<FunctionBasic>(options?.OrderedBy))
        {
            Assert.Equal(
                referenceValues,
                functions,
                new TestingEqualityComparerForLinkedFunctionObject(Output));
        }
    }

    [Fact]
    public async Task GetFunctionsIncludeRecursiveInstanceNotFound()
    {
        IReadService read = await Fixture.GetReadServiceAsync();

        await Assert.ThrowsAsync<InstanceNotFoundException>(
            () => read.GetFunctionsOfProfileAsync(
                Guid.NewGuid().ToString(),
                true));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("  ")]
    public async Task GetFunctionsIncludeRecursiveArgumentExceptions(string profileId)
    {
        IReadService read = await Fixture.GetReadServiceAsync();

        if (profileId == null)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => read.GetFunctionsOfProfileAsync(profileId, true));
        }

        else
        {
            await Assert.ThrowsAsync<ArgumentException>(() => read.GetFunctionsOfProfileAsync(profileId, true));
        }
    }

    [Theory]
    [MemberData(nameof(GetRandomThreeUsersForEmptyResult))]
    public async Task GetFunctionsIncludeRecursiveNothingFoundForUser(string userId)
    {
        IReadService read = await Fixture.GetReadServiceAsync();

        IPaginatedList<LinkedFunctionObject> result = await read.GetFunctionsOfProfileAsync(userId, true);

        Assert.Equal(0, result.TotalAmount);
    }

    [Theory]
    [MemberData(nameof(GetResultForUserIncludeRecursive))]
    public async Task GetFunctionsIncludeRecursiveNothingRightUserWithResult(
        string userId)
    {
        IReadService read = await Fixture.GetReadServiceAsync();
        List<FunctionObjectEntityModel> resultObjects = Fixture.GetFunctionForAssignments();

        var resultLinkedFunctionObjects =
            Mapper.Map<List<LinkedFunctionObject>>(resultObjects);

        IPaginatedList<LinkedFunctionObject> result = await read.GetFunctionsOfProfileAsync(userId, true);

        resultLinkedFunctionObjects.Should()
            .BeEquivalentTo(
                result.Select(r => r).ToList(),
                opt => opt.Excluding(p => p.Conditions).Excluding(p => p.IsActive));
    }

        
    [Theory]
    [MemberData(nameof(GetFunctionsTestArguments))]
    public async Task GetFunctionsAsBasic_shouldWork(AssignmentQueryObject options)
    {
        IReadService service = await Fixture.GetReadServiceAsync();

        IPaginatedList<FunctionBasic> functions = await service.GetFunctionsAsync<FunctionBasic>(options);

        IPaginatedList<FunctionBasic> referenceValues = Fixture.GetTestFunctions()
            .Select(Mapper.Map<FunctionBasic>)
            .UsingQueryOptions(
                options,
                QueryObjectHelpers
                    .CheckFunctionSearchableProperties);

        Assert.Equal(referenceValues.TotalAmount, functions.TotalAmount);
        Assert.Equal(referenceValues.Count, functions.Count);

        // Collections can only be compared directly, if they pre-sorted (backend default sorting methods may differ)
        // If they are not, that should be done before comparing that inside the test.
        // But that will be only possible, if the whole result set is available.
        // Due of pagination settings, it can be different.
        if (string.IsNullOrEmpty(options?.OrderedBy) && functions.TotalAmount == functions.Count)
        {
            Assert.Equal(
                referenceValues.OrderBy(v => v.Id),
                functions.OrderBy(r => r.Id),
                new TestingEqualityComparerForFunctionBasic(Output));
        }
        else if (!string.IsNullOrEmpty(options?.OrderedBy))
        {
            Assert.Equal(referenceValues, functions, new TestingEqualityComparerForFunctionBasic(Output));
        }
    }

    [Theory]
    [MemberData(nameof(GetFunctionsTestArguments))]
    public async Task GetFunctionsAsView_shouldWork(AssignmentQueryObject options)
    {
        IReadService service = await Fixture.GetReadServiceAsync();

        IPaginatedList<FunctionView> functions = await service.GetFunctionsAsync<FunctionView>(options);

        IPaginatedList<FunctionView> referenceValues = Fixture.GetTestFunctions()
            .Select(Mapper.Map<FunctionView>)
            .UsingQueryOptions(
                options,
                QueryObjectHelpers
                    .CheckFunctionSearchableProperties);

        Assert.Equal(referenceValues.TotalAmount, functions.TotalAmount);
        Assert.Equal(referenceValues.Count, functions.Count);

        // Collections can only be compared directly, if they pre-sorted (backend default sorting methods may differ)
        // If they are not, that should be done before comparing that inside the test.
        // But that will be only possible, if the whole result set is available.
        // Due of pagination settings, it can be different.
        if (string.IsNullOrEmpty(options?.OrderedBy) && functions.TotalAmount == functions.Count)
        {
            referenceValues.Should().BeEquivalentTo(functions,
                setup => setup
                    .WithoutStrictOrdering()
                    .TreatEmptyListsAndNullTheSame(f => f.ExternalIds)
                    .ComparingByMembers<OrganizationBasic>());
        }
        else if (!string.IsNullOrEmpty(options?.OrderedBy))
        {
            referenceValues.Should().BeEquivalentTo(functions,
                setup => setup
                    .WithStrictOrdering()
                    .TreatEmptyListsAndNullTheSame(f => f.ExternalIds));
        }
    }

    [Theory]
    [ClassData(typeof(CorrectQueryObjectTestData))]
    public async Task GetFunctionAsView_with_sort_properties_shouldWork(
        AssignmentQueryObject options,
        IPaginatedList<FunctionView> expectedResult)
    {
        IReadService service = await Fixture.GetReadServiceAsync();

        IPaginatedList<FunctionView> functions =
            await service.GetFunctionsAsync<FunctionView>(options);

        Assert.Equal(expectedResult.TotalAmount, functions.TotalAmount);
        Assert.Equal(expectedResult.Count, functions.Count);

        functions.Should()
            .BeEquivalentTo(
                expectedResult,
                setup => setup.TreatEmptyListsAndNullTheSame(f => f.ExternalIds)
                    .ComparingByMembers<OrganizationBasic>());
    }

    [Theory]
    [MemberData(nameof(GetFunctionTestArguments))]
    public async Task GetFunction_shouldWork(string functionId)
    {
        IReadService service = await Fixture.GetReadServiceAsync();

        var function = await service.GetFunctionAsync<FunctionView>(functionId);

        var referenceValue = Mapper.Map<FunctionView>(
            Fixture.GetTestFunctions()
                .First(r => r.Id == functionId));

        Assert.NotNull(function);

        function.Should()
            .BeEquivalentTo(
                referenceValue,
                setup => setup.TreatEmptyListsAndNullTheSame(f => f.ExternalIds)
                    .Excluding(f => f.Organization)
                    .ComparingByMembers<OrganizationBasic>()); // Equals method should be ignored
    }

    public static IEnumerable<object[]> GetFunctionsTestArguments()
    {
        return GetQueryObjectsAndExceptionTypes<RoleBasic>(defaultOrderBy: "Name")
            .Where(o => o.expectedExceptionType == null)
            .Select(o => new object[] { o.options });
    }

    public static IEnumerable<object[]> GetFunctionsOfProfileRecursiveFalseTestArguments()
    {
        return TestArgumentsHelper.GetFunctionAndRoleOfProfileArgumentParts()
            .Concat(GetInvalidIds(0, 1, 3))
            .SelectMany(
                obj =>
                    GetQueryObjectsAndExceptionTypes<FunctionBasic>(
                            obj.ElementAtOrDefault(2) as string)
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

    public static IEnumerable<object[]> GetFunctionTestArguments()
    {
        return DatabaseFixture.TestFunctions
            .Take(3)
            .Select(r => new object[] { r.Id });
    }

    public static IEnumerable<object[]> GetRandomThreeUsersForEmptyResult()
    {
        return DatabaseFixture.TestUsers.Take(3).Select(r => new object[] { r.Id });
    }

    public static IEnumerable<object[]> GetResultForUserIncludeRecursive()
    {
        return new List<object[]>

        {
            new object[]
            {
                DatabaseFixture.TestAssignmentUserIdRecursive
            }
        };
    }
}