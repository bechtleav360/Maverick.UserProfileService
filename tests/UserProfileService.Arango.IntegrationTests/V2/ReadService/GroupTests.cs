using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Arango.IntegrationTests.V2.Abstractions;
using UserProfileService.Arango.IntegrationTests.V2.Comparers;
using UserProfileService.Arango.IntegrationTests.V2.Extensions;
using UserProfileService.Arango.IntegrationTests.V2.Fixtures;
using UserProfileService.Common.Tests.Utilities.Comparers;
using UserProfileService.Common.Tests.Utilities.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Exceptions;
using Xunit;
using Xunit.Abstractions;
using Group = Maverick.UserProfileService.Models.Models.Group;

namespace UserProfileService.Arango.IntegrationTests.V2.ReadService
{
    [Collection(nameof(DatabaseCollection))]
    public class GroupTests : ReadTestBase
    {
        public GroupTests(DatabaseFixture fixture, ITestOutputHelper output)
            : base(fixture, output)
        {
        }

        private IProfile Convert(IProfile profile)
        {
            if (profile is UserBasic ub)
            {
                return ub;
            }

            if (profile is GroupBasic gb)
            {
                return gb;
            }

            return profile.Kind == ProfileKind.Group
                ? Mapper.Map<GroupBasic>(profile)
                : Mapper.Map<UserBasic>(profile);
        }

        [Fact]
        public async Task GetGroupsFilteredByDisplayName()
        {
            List<Group> referenceValues =
                Fixture.GetDefaultTestGroups()
                    .Select(Mapper.Map<Group>)
                    .OrderBy(g => g.CreatedAt)
                    .Take(5)
                    .ToList();

            var queryOptions = new AssignmentQueryObject
            {
                Filter = new Filter
                {
                    Definition = new List<Definitions>
                    {
                        new Definitions
                        {
                            FieldName = nameof(IProfile.DisplayName),
                            Values = referenceValues
                                .Select(v => v.DisplayName)
                                .ToArray()
                        }
                    }
                },
                Limit = 5,
                Offset = 0,
                OrderedBy = nameof(IProfile.CreatedAt),
                SortOrder = SortOrder.Asc
            };

            IReadService service = await Fixture.GetReadServiceAsync();

            IPaginatedList<IProfile> groups = await service.GetProfilesAsync<User, Group, Organization>(
                RequestedProfileKind.Group,
                queryOptions);

            Assert.Equal(5, groups.Count);

            Assert.Equal(
                referenceValues,
                groups.Cast<Group>().ToList(),
                new TestingEqualityComparerForGroupEntities(Output));
        }

        [Theory]
        [MemberData(nameof(GetParentOfGroupTestArguments))]
        public async Task GetParentsOfGroupAndReturnGroupList(string childId)
        {
            AssignmentQueryObject querySettings = GetDefaultAssignmentQueryObject(null, null, "Name");

            IReadService service = await Fixture.GetReadServiceAsync();

            IPaginatedList<IContainerProfile> parents =
                await service.GetParentsOfProfileAsync<GroupView, OrganizationView>(
                    childId,
                    RequestedProfileKind.Group,
                    querySettings);

            IPaginatedList<GroupView> referenceValues = Fixture
                .GetTestGroups()
                .Where(g => g.Members.Any(m => m.Id == childId))
                .Select(Mapper.Map<GroupView>)
                .UsingQueryOptions(querySettings);

            Assert.Equal(referenceValues.TotalAmount, parents.TotalAmount);
            Assert.Equal(referenceValues.Count, parents.Count);
        }

        [Theory]
        [MemberData(nameof(GetParentOfGroupTestArguments))]
        public async Task GetParentsOfGroupAndReturnGroupBasic(string childId)
        {
            AssignmentQueryObject querySettings = GetDefaultAssignmentQueryObject(null, null, "Name");

            IReadService service = await Fixture.GetReadServiceAsync();

            IPaginatedList<IContainerProfile> parents =
                await service.GetParentsOfProfileAsync<GroupBasic, OrganizationBasic>(
                    childId,
                    RequestedProfileKind.Group,
                    querySettings);

            IPaginatedList<IContainerProfile> referenceValues = Fixture
                .GetTestGroups()
                .Where(g => g.Members.Any(m => m.Id == childId))
                .Select(Mapper.Map<GroupBasic>)
                .ToList<IContainerProfile>()
                .UsingQueryOptions(querySettings);

            Assert.Equal(referenceValues.TotalAmount, parents.TotalAmount);
            Assert.Equal(referenceValues.Count, parents.Count);
            Assert.Equal(referenceValues, parents, new TestingEqualityComparerForProfiles(Output));
        }

        [Theory]
        [MemberData(nameof(GetParentOfGroupTestArgumentsFailure))]
        public async Task GetParentsOfGroupWillNotWork(string childId, Type expectedException)
        {
            IReadService service = await Fixture.GetReadServiceAsync();

            await Assert.ThrowsAsync(
                expectedException,
                () => service.GetParentsOfProfileAsync<GroupView, OrganizationView>(
                    childId,
                    RequestedProfileKind.Group));
        }

        [Theory]
        [MemberData(nameof(GetRootGroupsTestArguments))]
        public async Task GetRootGroups(
            AssignmentQueryObject filterObject,
            string expectedFilterString,
            Type expectedExceptionType)
        {
            IReadService service = await Fixture.GetReadServiceAsync();

            if (expectedExceptionType != null)
            {
                await Assert.ThrowsAsync(
                    expectedExceptionType,
                    () => service.GetRootProfilesAsync<GroupView, OrganizationView>(
                        RequestedProfileKind.Group,
                        filterObject));

                return;
            }

            IPaginatedList<IContainerProfile> roots = await service.GetRootProfilesAsync<GroupView, OrganizationView>(
                RequestedProfileKind.Group,
                filterObject);

            List<GroupView> reference = Fixture.GetTestGroups()
                .Where(
                    g => (filterObject?.Filter == null
                            || expectedFilterString == null
                            || Regex.IsMatch(
                                g.Name,
                                expectedFilterString,
                                RegexOptions.IgnoreCase)
                            || Regex.IsMatch(
                                g.DisplayName,
                                expectedFilterString,
                                RegexOptions.IgnoreCase))
                        && (g.MemberOf == null
                            || g.MemberOf.Count(p => p.Kind != ProfileKind.Organization)
                            == 0))
                .SortBy(
                    filterObject?.OrderedBy,
                    filterObject?.SortOrder ?? SortOrder.Asc)
                .Select(Mapper.Map<GroupView>)
                .ToList();

            Assert.Equal(reference.Count, roots.TotalAmount);

            if (reference.Count <= roots.TotalAmount
                && string.IsNullOrEmpty(filterObject?.OrderedBy))
            {
                Assert.Equal(
                    reference.OrderBy(o => o.Name),
                    roots.OrderBy(o => o.Name),
                    new TestingEqualityComparerForProfiles(
                        Output,
                        new TestingEqualityComparerForUserEntities(Output),
                        new TestingEqualityComparerForGroups(Output)));
            }
            else if (!string.IsNullOrEmpty(filterObject?.OrderedBy))
            {
                Assert.Equal(
                    roots,
                    reference
                        .Skip(filterObject.Offset)
                        .Take(filterObject.Limit),
                    new TestingEqualityComparerForProfiles(
                        Output,
                        new TestingEqualityComparerForUserEntities(Output),
                        new TestingEqualityComparerForGroups(Output)));
            }
        }

        [Theory]
        [MemberData(nameof(GetGroupMembersTestArguments))]
        public async Task GetGroupMembers(
            string groupId,
            AssignmentQueryObject queryObject,
            Type expectedExceptionType)
        {
            IReadService service = await Fixture.GetReadServiceAsync();

            if (expectedExceptionType != null)
            {
                await Assert.ThrowsAsync(
                    expectedExceptionType,
                    () => service.GetChildrenOfProfileAsync<UserView, GroupBasic, OrganizationBasic>(
                        groupId,
                        ProfileContainerType.Group,
                        RequestedProfileKind.User,
                        queryObject));

                return;
            }

            IPaginatedList<IProfile> children =
                queryObject != null
                    ? await service.GetChildrenOfProfileAsync<UserBasic, GroupBasic, OrganizationBasic>(
                        groupId,
                        ProfileContainerType.Group,
                        RequestedProfileKind.User,
                        new AssignmentQueryObject
                        {
                            Limit = queryObject.Limit,
                            Offset = queryObject.Offset,
                            OrderedBy = queryObject.OrderedBy,
                            SortOrder = queryObject.SortOrder
                        })
                    : await service.GetChildrenOfProfileAsync<UserBasic, GroupBasic, OrganizationBasic>(
                        groupId,
                        ProfileContainerType.Group,
                        RequestedProfileKind.User);

            List<IProfile> referenceValues =
                Fixture.GetTestGroups()
                    .First(g => g.Id == groupId)
                    .Members
                    .Where(m => m.Kind == ProfileKind.User)
                    .Select(m => Fixture.GetTestProfile(m.Id))
                    .Where(p => p != null)
                    .DoFunctionForEachAndReturn(p => p.ExternalIds ??= new List<ExternalIdentifier>())
                    .ToList();

            Assert.Equal(referenceValues.Count, children.TotalAmount);

            if (queryObject?.OrderedBy == null)
            {
                List<IProfile> pagedReferenceValues = referenceValues
                    .OrderBy(p => p.Id)
                    .Skip(queryObject?.Offset ?? 0)
                    .Take(queryObject?.Limit ?? 100)
                    .Select(Convert)
                    .ToList();

                Assert.Equal(
                    pagedReferenceValues,
                    children.OrderBy(p => p.Id),
                    new TestingEqualityComparerForProfiles(
                        Output,
                        new TestingEqualityComparerForUserEntities(Output),
                        new TestingEqualityComparerForGroupEntities(Output)));
            }
            else
            {
                List<IProfile> pagedReferenceValues = referenceValues
                    .SortBy(
                        queryObject.OrderedBy,
                        queryObject.SortOrder) // different sorting
                    .Skip(queryObject.Offset)
                    .Take(queryObject.Limit)
                    .Select(Convert)
                    .ToList();

                Assert.Equal(
                    pagedReferenceValues,
                    children,
                    new TestingEqualityComparerForProfiles(
                        Output,
                        new TestingEqualityComparerForUserEntities(Output),
                        new TestingEqualityComparerForGroupEntities(Output)));
            }
        }

        [Fact]
        public async Task GetAllGroupsThatContainsSpecifiedUsers()
        {
            IReadService service = await Fixture.GetReadServiceAsync();

            GroupEntityModel sampleGroup = Fixture.GetTestGroups()
                .FirstOrDefault(
                    g => g.Members != null
                        && g.Members.Any(m => m.Kind == ProfileKind.User));

            Member userInSampleGroup =
                sampleGroup?.Members.FirstOrDefault(m => m.Kind == ProfileKind.User);

            if (userInSampleGroup == null)
            {
                throw new Exception("Insufficient sample data: Missing a group with a user as member.");
            }

            IPaginatedList<IProfile> foundProfiles =
                await service.GetProfilesAsync<UserBasic, GroupBasic, OrganizationBasic>(
                    RequestedProfileKind.Group,
                    new AssignmentQueryObject
                    {
                        Filter = new Filter
                        {
                            Definition = new List<Definitions>
                            {
                                new Definitions
                                {
                                    FieldName = "Members.Name",
                                    Values = new[] { userInSampleGroup.Name, "&/()=öäÜ /\\?" }
                                }
                            }
                        }
                    });

            Assert.Contains(foundProfiles, p => p is GroupBasic g && g.Id == sampleGroup.Id);
        }

        public static IEnumerable<object[]> GetRootGroupsTestArguments()
        {
            Member testUser = DatabaseFixture
                .TestGroups
                .First(
                    g => (g.MemberOf == null || g.MemberOf.Count == 0)
                        && g.Members != null
                        && g.Members.Any(m => m.Kind == ProfileKind.User))
                .Members
                .First(m => m.Kind == ProfileKind.User);

            Member testGroup = DatabaseFixture
                .TestGroups
                .First(
                    g => (g.MemberOf == null || g.MemberOf.Count == 0)
                        && g.Members != null
                        && g.Members.Any(m => m.Kind == ProfileKind.Group))
                .Members
                .First(m => m.Kind == ProfileKind.Group);

            yield return new object[]
            {
                new AssignmentQueryObject
                {
                    Filter = new Filter
                    {
                        Definition = new List<Definitions>
                        {
                            new Definitions
                            {
                                FieldName = nameof(IProfile.Name),
                                Values = new[] { testUser.Name }
                            },
                            new Definitions
                            {
                                FieldName = nameof(IProfile.DisplayName),
                                Values = new[] { testGroup.DisplayName }
                            }
                        },
                        CombinedBy = BinaryOperator.Or
                    },
                    Offset = 0,
                    Limit = 100,
                    OrderedBy = nameof(Group.DisplayName)
                },
                $"^({testUser.Name}|{testGroup.DisplayName})$",
                null
            };

            yield return new object[]
            {
                new AssignmentQueryObject
                {
                    Offset = 0,
                    Limit = 100,
                    OrderedBy = nameof(Group.DisplayName)
                },
                null,
                null
            };

            yield return new object[] { null, null, null };

            yield return new object[]
            {
                new AssignmentQueryObject
                {
                    Filter = new Filter
                    {
                        Definition = new List<Definitions>()
                    }
                },
                null,
                typeof(ValidationException)
            };
        }

        public static IEnumerable<object[]> GetGroupMembersTestArguments()
        {
            return DatabaseFixture
                .TestGroups
                .Where(
                    g => g.Members.Count(m => m.Kind == ProfileKind.Group) > 0
                        && g.Members.Count(m => m.Kind == ProfileKind.User) > 0)
                .Take(3)
                .Select(g => new object[] { g.Id, null, null })
                .Concat(
                    DatabaseFixture
                        .TestGroups
                        .Where(g => g.Members.Count == 0)
                        .Take(3)
                        .Select(g => new object[] { g.Id, null, null }))
                .Concat(
                    DatabaseFixture.TestUsers
                        .Take(2)
                        .Select(u => new object[] { u.Id, null, typeof(InstanceNotFoundException) }))
                .Concat(
                    new List<object[]>
                    {
                        new object[] { "123-123", null, typeof(InstanceNotFoundException) }
                    })
                .SelectMany(
                    args =>
                        GetQueryObjectsAndExceptionTypes<UserBasic>()
                            .Select(
                                o => new[]
                                {
                                    args[0],
                                    o.options,
                                    GetExpectedExceptionType(
                                        args[2] as Type,
                                        o.expectedExceptionType,
                                        o.prio == 2)
                                }))
                .Concat(
                    new List<object[]>
                    {
                        new object[] { "", null, typeof(ArgumentException) },
                        new object[] { "    ", null, typeof(ArgumentException) },
                        new object[] { null, null, typeof(ArgumentNullException) }
                    });
        }

        public static IEnumerable<object[]> GetParentOfGroupTestArguments()
        {
            return DatabaseFixture
                .TestGroups
                .Where(g => g.MemberOf != null && g.MemberOf.Any())
                .Take(2)
                .Select(g => new object[] { g.Id })
                .Concat(
                    DatabaseFixture.TestGroups
                        .Where(g => g.MemberOf == null || !g.MemberOf.Any())
                        .Take(2)
                        .Select(g => new object[] { g.Id }));
        }

        public static IEnumerable<object[]> GetParentOfGroupTestArgumentsFailure()
        {
            yield return new object[]
            {
                // user data is valid, but a group is expected (users and groups are in the same collection)
                DatabaseFixture.TestUsers
                    .First()
                    .Id,
                typeof(InstanceNotFoundException)
            };

            yield return new object[] { "725878295jHJKHJkhakjf8789u14jkl//87", typeof(InstanceNotFoundException) };

            yield return new object[] { "", typeof(ArgumentException) };

            yield return new object[] { "    ", typeof(ArgumentException) };

            yield return new object[] { null, typeof(ArgumentNullException) };
        }
    }
}
