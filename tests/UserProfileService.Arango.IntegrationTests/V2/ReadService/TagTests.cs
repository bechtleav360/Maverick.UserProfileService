using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Arango.IntegrationTests.V2.Abstractions;
using UserProfileService.Arango.IntegrationTests.V2.Fixtures;
using UserProfileService.Common.Tests.Utilities.Comparers;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Exceptions;
using Xunit;
using Xunit.Abstractions;
using ValidationException = System.ComponentModel.DataAnnotations.ValidationException;

namespace UserProfileService.Arango.IntegrationTests.V2.ReadService
{
    [Collection(nameof(DatabaseCollection))]
    public class TagTests : ReadTestBase
    {
        public TagTests(
            DatabaseFixture fixture,
            ITestOutputHelper output) : base(fixture, output)
        {
        }

        private IProfileEntityModel GetProfileById(string profileId)
        {
            return Fixture
                    .GetTestUsers()
                    .FirstOrDefault(p => p.Id == profileId) as IProfileEntityModel
                ?? Fixture
                    .GetTestGroups()
                    .FirstOrDefault(p => p.Id == profileId);
        }

        private static string GetPartOfName(
            string name,
            int start = 2,
            int endMinus = 1)
        {
            int length = name.Length - start - endMinus;

            if (length < 0)
            {
                return name.Length < start ? name[start..] : name;
            }

            return name.Substring(start, length);
        }

        [Theory]
        [MemberData(nameof(GetTagsTestArguments))]
        public async Task GetTagsOfProfileShouldWork(string profileId)
        {
            IReadService service = await Fixture.GetReadServiceAsync();

            IPaginatedList<CalculatedTag> tags = await service.GetTagsOfProfileAsync(
                profileId,
                RequestedTagType.All);

            IProfileEntityModel profile = GetProfileById(profileId);

            if (profile == null)
            {
                throw new Exception("Unknown profile id during test.");
            }

            Assert.Equal(profile.Tags, tags, new TestingEqualityComparerForCalculatedTags());
        }

        [Theory]
        [MemberData(nameof(GetTagsTestArguments))]
        public async Task GetTagsOfProfileWithCustomTagTypeShouldWork(string profileId)
        {
            IReadService service = await Fixture.GetReadServiceAsync();

            IPaginatedList<CalculatedTag> tags = await service.GetTagsOfProfileAsync(
                profileId,
                RequestedTagType.Custom);

            IProfileEntityModel profile =
                GetProfileById(profileId);

            if (profile == null)
            {
                throw new Exception("Unknown profile id during test.");
            }

            List<CalculatedTag> expectedTags = profile.Tags.Where(t => t.Type == TagType.Custom).ToList();

            Assert.Equal(expectedTags, tags, new TestingEqualityComparerForCalculatedTags());
        }

        [Theory]
        [MemberData(nameof(GetTagsTestArguments))]
        public async Task GetTagsOfProfileWithFarTagTypeShouldWork(string profileId)
        {
            IReadService service = await Fixture.GetReadServiceAsync();

            IPaginatedList<CalculatedTag> tags = await service.GetTagsOfProfileAsync(
                profileId,
                RequestedTagType.FunctionalAccessRights);

            IProfileEntityModel profile =
                GetProfileById(profileId);

            if (profile == null)
            {
                throw new Exception("Unknown profile id during test.");
            }

            List<CalculatedTag> expectedTags =
                profile.Tags.Where(t => t.Type == TagType.FunctionalAccessRights).ToList();

            Assert.Equal(expectedTags, tags, new TestingEqualityComparerForCalculatedTags());
        }

        [Theory]
        [MemberData(nameof(GetTagsTestArguments))]
        public async Task GetTagsOfProfileWithSecurityTagTypeShouldWork(string profileId)
        {
            IReadService service = await Fixture.GetReadServiceAsync();

            IPaginatedList<CalculatedTag> tags = await service.GetTagsOfProfileAsync(
                profileId,
                RequestedTagType.Security);

            IProfileEntityModel profile =
                GetProfileById(profileId);

            if (profile == null)
            {
                throw new Exception("Unknown profile id during test.");
            }

            List<CalculatedTag> expectedTags = profile.Tags.Where(t => t.Type == TagType.Security).ToList();

            Assert.Equal(expectedTags, tags, new TestingEqualityComparerForCalculatedTags());
        }

        [Theory]
        [MemberData(nameof(GetAllTagsTestArguments))]
        public async Task GetTagsShouldWork(string search, bool includeOptions)
        {
            QueryObject queryObject = includeOptions
                ? new QueryObject
                {
                    Search = search,
                    Limit = 500
                }
                : null;

            IReadService service = await Fixture.GetReadServiceAsync();

            if (includeOptions && search != null && string.IsNullOrWhiteSpace(search))
            {
                await Assert.ThrowsAsync<ValidationException>(() => service.GetTagsAsync(queryObject));

                return;
            }

            IPaginatedList<Tag> tags = await service.GetTagsAsync(queryObject);

            List<Tag> referenceValues =
                DatabaseFixture.TestTagEntities
                    .Where(
                        t => !includeOptions
                            || search == null
                            || t.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            Assert.Equal(referenceValues.Count, tags.TotalAmount);

            Assert.Equal(
                referenceValues.OrderBy(v => v.Id),
                tags.OrderBy(t => t.Id),
                new TestingEqualityComparerAdvancedForTag(Output));
        }

        [Fact]
        public async Task GetExistentTagsShouldWork()
        {
            IReadService service = await Fixture.GetReadServiceAsync();
            var faker = new Faker();

            List<string> referenceValues = faker.PickRandom(DatabaseFixture.TestTagEntities, 5)
                .Select(t => t.Id)
                .ToList();

            IEnumerable<string> tagFilter =
                referenceValues
                    .Concat(faker.Make(10, () => faker.Random.Guid().ToString()));

            List<string> tags = (await service.GetExistentTagsAsync(tagFilter)).ToList();

            Assert.Equal(referenceValues.Count, tags.Count);

            Assert.Equal(
                referenceValues.OrderBy(v => v),
                tags.OrderBy(t => t));
        }

        [Theory]
        [MemberData(nameof(GetTestArgsExistentTagsShouldNotWork))]
        public async Task GetExistentTagsShouldNotWork(IEnumerable<string> tagFilter, Type exceptionType)
        {
            IReadService service = await Fixture.GetReadServiceAsync();

            await Assert.ThrowsAsync(exceptionType, () => service.GetExistentTagsAsync(tagFilter));
        }

        [Theory]
        [MemberData(nameof(GetTestArgsOfGetTag))]
        public async Task GetTagShouldWork(string tagId)
        {
            IReadService service = await Fixture.GetReadServiceAsync();

            Tag referenceValue = Fixture.GetTestTags().FirstOrDefault(t => t.Id == tagId);

            Tag found = await service.GetTagAsync(tagId);

            Assert.Equal(referenceValue, found, new TestingEqualityComparerAdvancedForTag(Output));
        }

        [Theory]
        [MemberData(nameof(GetTestArgsOfGetTagShouldNotWork))]
        public async Task GetTagShouldNotWork(string tagId, Type exceptionType)
        {
            IReadService service = await Fixture.GetReadServiceAsync();

            await Assert.ThrowsAsync(exceptionType, () => service.GetTagAsync(tagId));
        }

        public static IEnumerable<object[]> GetAllTagsTestArguments()
        {
            yield return new object[]
            {
                GetPartOfName(DatabaseFixture.TestTagEntities.First(t => !string.IsNullOrWhiteSpace(t.Name)).Name),
                true
            };

            yield return new object[]
            {
                GetPartOfName(
                    DatabaseFixture.TestTagEntities
                        .Where(t => !string.IsNullOrWhiteSpace(t.Id))
                        .ElementAt(2)
                        .Id),
                true
            };

            yield return new object[] { null, true };
            yield return new object[] { "", true };
            yield return new object[] { "   ", true };
            yield return new object[] { $"{Guid.NewGuid()}#{DateTime.UtcNow:s}", true };
            yield return new object[] { null, false };
        }

        public static IEnumerable<object[]> GetTestArgsOfGetTag()
        {
            return DatabaseFixture.TestTagEntities.Skip(5).Take(2).Select(t => new object[] { t.Id });
        }

        public static IEnumerable<object[]> GetTestArgsOfGetTagShouldNotWork()
        {
            yield return new object[] { null, typeof(ArgumentNullException) };
            yield return new object[] { "", typeof(ArgumentException) };
            yield return new object[] { "   ", typeof(ArgumentException) };
            yield return new object[] { $"{Guid.NewGuid()}_{DateTime.UtcNow:s}", typeof(InstanceNotFoundException) };
        }

        public static IEnumerable<object[]> GetTagsTestArguments()
        {
            return ConcatArguments(
                DatabaseFixture.TestGroups
                    .Where(g => g.Tags == null || g.Tags.Count == 0)
                    .Take(2)
                    .Select(g => new object[] { g.Id }),
                DatabaseFixture.TestGroups
                    .Where(g => g.Tags != null && g.Tags.Count > 0)
                    .Take(2)
                    .Select(g => new object[] { g.Id }),
                DatabaseFixture.TestUsers
                    .Where(g => g.Tags == null || g.Tags.Count == 0)
                    .Take(2)
                    .Select(g => new object[] { g.Id }),
                DatabaseFixture.TestUsers
                    .Where(g => g.Tags != null && g.Tags.Count > 0)
                    .Take(2)
                    .Select(g => new object[] { g.Id }));
        }

        public static IEnumerable<object[]> GetTestArgsExistentTagsShouldNotWork()
        {
            yield return new object[] { null, typeof(ArgumentNullException) };
            yield return new object[] { new[] { " ", "", null, "    " }, typeof(ArgumentException) };
            yield return new object[] { new List<string>(), typeof(ArgumentException) };
        }
    }
}
