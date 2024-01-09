using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;
using UserProfileService.Adapter.Arango.V2.EntityModels;
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
    public class ProfileTests : ReadTestBase
    {
        public ProfileTests(DatabaseFixture fixture, ITestOutputHelper output)
            : base(fixture, output)
        {
        }

        [Theory]
        [InlineData(RequestedProfileKind.All)]
        [InlineData(RequestedProfileKind.User)]
        [InlineData(RequestedProfileKind.Group)]
        [InlineData(RequestedProfileKind.Organization)]
        [InlineData(RequestedProfileKind.Undefined)]
        public async Task GetProfilesOfSpecifiedKindLimited(RequestedProfileKind kind)
        {
            IReadService service = await Fixture.GetReadServiceAsync();

            IPaginatedList<IProfile> profiles = await service.GetProfilesAsync<UserView, GroupView, OrganizationView>(
                kind,
                new AssignmentQueryObject
                {
                    Limit = 5,
                    Offset = 0
                });

            Assert.NotNull(profiles);

            int expectedAmount = Fixture.GetTestProfiles()
                .Count(
                    p => kind == RequestedProfileKind.All
                        || kind == RequestedProfileKind.Undefined
                        || (kind == RequestedProfileKind.User && p.Kind == ProfileKind.User)
                        || (kind == RequestedProfileKind.Group && p.Kind == ProfileKind.Group)
                        || (kind == RequestedProfileKind.Organization && p.Kind == ProfileKind.Organization));

            Assert.Equal(expectedAmount > 5 ? 5 : expectedAmount, profiles.Count);
            Assert.Equal(expectedAmount, profiles.TotalAmount);
        }

        [Theory]
        [MemberData(nameof(GetProfileTestArguments))]
        public async Task GetProfile(string id, bool isUser)
        {
            IReadService service = await Fixture.GetReadServiceAsync();
            var profile = await service.GetProfileAsync<IProfile>(id, RequestedProfileKind.All);

            if (isUser)
            {
                Assert.True(profile is UserBasic, "Profile should be user, but is not.");

                Assert.Equal(
                    Mapper.Map<UserBasic>(
                        Fixture
                            .GetTestUsers()
                            .FirstOrDefault(p => p.Id == id)),
                    profile as UserBasic,
                    new TestingEqualityComparerForUserBasic(Output));

                return;
            }

            Assert.True(profile is GroupBasic, "Profile should be group, but is not.");

            Assert.Equal(
                Mapper.Map<GroupBasic>(Fixture.GetTestGroups().FirstOrDefault(g => g.Id == id)),
                profile as GroupBasic,
                new TestingEqualityComparerForGroupBasic(Output));
        }

        [Theory]
        [MemberData(nameof(GetAssignedProfilesTestArguments))]
        public async Task GetAssignedProfilesShouldWork(string roleOrFunctionId, QueryObject options)
        {
            IReadService service = await Fixture.GetReadServiceAsync();

            IPaginatedList<Member> profiles = await service.GetAssignedProfiles(roleOrFunctionId, options);

            IPaginatedList<IProfileEntityModel> referenceValues =
                Fixture
                    .GetTestUsers()
                    .Where(p => p.SecurityAssignments?.Any(s => s.Id == roleOrFunctionId) == true)
                    .Cast<IProfileEntityModel>()
                    .Concat(
                        Fixture
                            .GetTestGroups()
                            .Where(p => p.SecurityAssignments?.Any(s => s.Id == roleOrFunctionId) == true))
                    .UsingQueryOptions(
                        options,
                        (profile, search) => profile.MatchProperties(search));

            Assert.Equal(referenceValues.TotalAmount, profiles.TotalAmount);
            Assert.Equal(referenceValues.Count, profiles.Count);

            if (referenceValues.TotalAmount > referenceValues.Count && !string.IsNullOrWhiteSpace(options?.OrderedBy))
            {
                Assert.Equal(
                    referenceValues.Select(Mapper.Map<Member>),
                    profiles,
                    new TestingEqualityComparerForMembers());

                return;
            }

            if (referenceValues.TotalAmount > referenceValues.Count)
            {
                return;
            }

            IEnumerable<Member> checkingReference = string.IsNullOrEmpty(options?.OrderedBy)
                ? referenceValues.Select(Mapper.Map<Member>).OrderBy(v => v.Id)
                : referenceValues.Select(Mapper.Map<Member>);

            IEnumerable<Member> valuesUnderTest = string.IsNullOrEmpty(options?.OrderedBy)
                ? profiles.OrderBy(v => v.Id)
                : profiles;

            Assert.Equal(checkingReference, valuesUnderTest, new TestingEqualityComparerForMembers());
        }

        [Theory]
        [MemberData(nameof(GetProfileTestForExternalId), true)]
        public async Task GetProfileByInternalOrExternalId(string externalId, IProfile expectedProfile)
        {
            IReadService service = await Fixture.GetReadServiceAsync();

            List<IProfile> foundProfile =
                await service.GetProfileByExternalOrInternalIdAsync<User, Group, Organization>(externalId);

            Assert.NotNull(foundProfile);
            Assert.Single(foundProfile);
            foundProfile.First().Should().BeEquivalentTo(expectedProfile, opt => opt.Excluding(t => t.TagUrl));
        }

        [Theory]
        [MemberData(nameof(GetProfileTestForExternalIdWithSourceName))]
        public async Task GetProfileByInternalOrExternalIdWithSourceName(
            string externalId,
            string source,
            IProfile expectedProfile)
        {
            IReadService service = await Fixture.GetReadServiceAsync();

            List<IProfile> foundProfile =
                await service.GetProfileByExternalOrInternalIdAsync<User, Group, Organization>(
                    externalId,
                    true,
                    source);

            Assert.NotNull(foundProfile);
            Assert.Single(foundProfile);

            expectedProfile.Should()
                .BeEquivalentTo(
                    foundProfile.Single(),
                    opt => opt.Excluding(ex => ex.TagUrl));
        }

        [Theory]
        [MemberData(nameof(GetProfileTestForExternalIdWithWrongSourceName))]
        public async Task GetProfileByInternalOrExternalIdWithWrongSourceName(
            string externalId,
            string source)
        {
            IReadService service = await Fixture.GetReadServiceAsync();

            source = $"{source}-wrong";

            List<IProfile> foundProfile =
                await service.GetProfileByExternalOrInternalIdAsync<User, Group, Organization>(
                    externalId,
                    true,
                    source);

            Assert.Empty(foundProfile);
        }

        [Theory]
        [MemberData(nameof(GetProfileTestForExternalId), false)] // profile data won't be used during test
        public async Task GetProfileByInternalOrExternalIdAndAllowExternalIdsSetToFalse(
            string externalId)
        {
            IReadService service = await Fixture.GetReadServiceAsync();

            List<IProfile> foundProfile =
                await service.GetProfileByExternalOrInternalIdAsync<User, Group, Organization>(externalId, false);

            Assert.Empty(foundProfile);
        }

        [Theory]
        [MemberData(nameof(GetProfileTestForExternalId), true)]
        public async Task GetProfileByInternalOrExternalIdAndAllowSearchOnlyWithInternalId(
            string profileId,
            IProfile profile)
        {
            IReadService service = await Fixture.GetReadServiceAsync();

            List<IProfile> foundProfile =
                await service.GetProfileByExternalOrInternalIdAsync<User, Group, Organization>(profileId);

            Assert.NotNull(foundProfile);
            Assert.Single(foundProfile);
            profile.Should().BeEquivalentTo(profile, opt => opt.Excluding(t => t.TagUrl));
        }

        public static IEnumerable<object[]> GetProfileTestArguments()
        {
            return DatabaseFixture.TestUsers
                .Take(2)
                .Select(p => new object[] { p.Id, true })
                .Concat(
                    DatabaseFixture.TestGroups
                        .Take(2)
                        .Select(p => new object[] { p.Id, false }));
        }

        public static IEnumerable<object[]> GetProfileTestForExternalId(bool includeProfile)
        {
            return DatabaseFixture.TestUsers.Take(3)
                .Select(
                    p => includeProfile
                        ? new object[] { p.ExternalIds.First().Id, p }
                        : new object[] { p.ExternalIds.First().Id })
                .Concat(
                    DatabaseFixture.TestGroups
                        .Take(3)
                        .Select(
                            p => includeProfile
                                ? new object[] { p.ExternalIds.First().Id, p }
                                : new object[] { p.ExternalIds.First().Id }));
        }

        public static IEnumerable<object[]> GetProfileTestForExternalIdWithSourceName()
        {
            return DatabaseFixture.TestUsers.Take(3)
                .Select(p => new object[] { p.ExternalIds.First().Id, p.ExternalIds.First().Source, p })
                .Concat(
                    DatabaseFixture.TestGroups
                        .Take(3)
                        .Select(p => new object[] { p.ExternalIds.First().Id, p.ExternalIds.First().Source, p }));
        }

        public static IEnumerable<object[]> GetProfileTestForExternalIdWithWrongSourceName()
        {
            return DatabaseFixture.TestUsers.Take(3)
                .Select(p => new object[] { p.ExternalIds.First().Id, "wrong" })
                .Concat(
                    DatabaseFixture.TestGroups
                        .Take(3)
                        .Select(p => new object[] { p.ExternalIds.First().Id, "wrong" }));
        }

        public static IEnumerable<object[]> GetAssignedProfilesTestArguments()
        {
            IEnumerable<(IQueryObject options, Type expectedExceptionType, int prio)> options =
                GetQueryObjectsAndExceptionTypes<IProfileEntityModel>()
                    .Where(o => o.expectedExceptionType == null);

            IEnumerable<string> ids = DatabaseFixture.TestFunctions
                .Where(f => f.LinkedProfiles == null || f.LinkedProfiles.Count == 0)
                .Take(2)
                .Select(f => f.Id)
                .ConcatSeveralSequences(
                    DatabaseFixture.TestFunctions
                        .Where(f => f.LinkedProfiles is
                        {
                            Count: > 0
                        })
                        .Take(2)
                        .Select(f => f.Id),
                    DatabaseFixture.TestRoles
                        .Where(f => f.LinkedProfiles == null || f.LinkedProfiles.Count == 0)
                        .Take(2)
                        .Select(f => f.Id),
                    DatabaseFixture.TestRoles
                        .Where(f => f.LinkedProfiles is
                        {
                            Count: > 0
                        })
                        .Take(2)
                        .Select(f => f.Id));

            return options
                .Select(info => info.options)
                .SelectMany(
                    option => ids
                        .Select(id => new object[] { id, option }));
        }
    }
}
