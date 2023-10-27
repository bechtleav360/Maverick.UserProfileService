using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Arango.IntegrationTests.V2.Abstractions;
using UserProfileService.Arango.IntegrationTests.V2.Extensions;
using UserProfileService.Arango.IntegrationTests.V2.Fixtures;
using UserProfileService.Common.V2.Abstractions;
using Xunit;
using Xunit.Abstractions;

namespace UserProfileService.Arango.IntegrationTests.V2.ReadService
{
    [Collection(nameof(DatabaseCollection))]
    public class UserTests : ReadTestBase
    {
        private readonly Dictionary<string, object> _propertyFunctionMapping = new Dictionary<string, object>
        {
            { nameof(User.Name), (Func<UserBasic, string>)(u => u.Name) },
            { nameof(User.LastName), (Func<UserBasic, string>)(u => u.LastName) },
            { "myTestingProperty", null }
        };

        public UserTests(DatabaseFixture fixture, ITestOutputHelper output)
            : base(fixture, output)
        {
        }

        [Fact]
        public async Task GetAllUsersUnfiltered()
        {
            IReadService service = await Fixture.GetReadServiceAsync();

            var options = new AssignmentQueryObject
            {
                Limit = 100,
                OrderedBy = "Name"
            };

            IPaginatedList<IProfile> users = await service.GetProfilesAsync<User, Group, Organization>(
                RequestedProfileKind.User,
                options);

            IPaginatedList<User> sampleUsers = Fixture
                .GetTestUsers()
                .Select(Mapper.Map<UserEntityModel, User>)
                .UsingQueryOptions(options);

            Assert.Equal(sampleUsers.TotalAmount, users.TotalAmount);
            Assert.Equal(sampleUsers.Count, users.Count);

            Assert.All(
                users,
                u =>
                {
                    if (u is User)
                    {
                        return;
                    }

                    throw new Exception("Wrong type. Must be user!");
                });

            List<string> missing = sampleUsers.Select(u => u.Id).Except(users.Select(u => u.Id)).ToList();
            Assert.Empty(missing);
        }

        [Theory]
        [InlineData("La", nameof(User.Name), SortOrder.Asc, null)]
        [InlineData("La", nameof(User.Name), SortOrder.Desc, null)]
        [InlineData("TOM", nameof(User.LastName), SortOrder.Asc, null)]
        [InlineData("TOM", nameof(User.LastName), SortOrder.Desc, null)]
        [InlineData("fAn", "myTestingProperty", SortOrder.Asc, null)]
        [InlineData("fAn", "myTestingProperty", SortOrder.Desc, null)]
        [InlineData("", "myTestingProperty", SortOrder.Asc, typeof(ValidationException))]
        [InlineData("  ", "myTestingProperty", SortOrder.Asc, typeof(ValidationException))]
        [InlineData(null, nameof(User.Name), SortOrder.Asc, null)]
        [InlineData("a", null, SortOrder.Asc, null)]
        [InlineData("a", null, SortOrder.Desc, null)]
        public async Task GetFilteredAndSortedUserProfiles(
            string searchString,
            string orderProperty,
            SortOrder sortOrder,
            Type expectedExceptionType)
        {
            IReadService service = await Fixture.GetReadServiceAsync();

            var options = new AssignmentQueryObject
            {
                Limit = 15,
                Offset = 0,
                OrderedBy = orderProperty,
                SortOrder = sortOrder,
                Search = searchString
            };

            if (expectedExceptionType != null)
            {
                await Assert.ThrowsAsync(
                    expectedExceptionType,
                    () => service.GetProfilesAsync<User, Group, Organization>(
                        RequestedProfileKind.User,
                        options));

                return;
            }

            Func<UserBasic, string> expectedOrdering =
                !string.IsNullOrWhiteSpace(orderProperty) && _propertyFunctionMapping.TryGetValue(orderProperty, out object value)
                    ? value as Func<UserBasic, string>
                    : null;

            IPaginatedList<IProfile> profiles = await service.GetProfilesAsync<User, GroupBasic, Organization>(
                RequestedProfileKind.User,
                options);

            Assert.NotNull(profiles);

            List<UserBasic> expectedSetTotal = Fixture
                .GetTestUsers()
                .Where(
                    u => string.IsNullOrWhiteSpace(searchString)
                        || u.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                        || u.DisplayName.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                        || u.LastName.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                        || u.FirstName.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                        || u.UserName.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                        || u.Email.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                .Select(Mapper.Map<UserBasic>)
                .OrderByIgnoreCase(expectedOrdering, sortOrder)
                .ToList();

            List<string> expectedSet = expectedSetTotal
                .Take(15)
                .Select(u => string.Concat(u.Name, "#", u.Id, "#", u.FirstName))
                .ToList();

            List<string> foundProfiles = profiles.Select(
                    p => p is User u
                        ? string.Concat(u.Name, "#", u.Id, "#", u.FirstName)
                        : null)
                .ToList();

            Assert.DoesNotContain(foundProfiles, string.IsNullOrWhiteSpace);
            Assert.Equal(expectedSetTotal.Count, profiles.TotalAmount);

            // without a sorting the paginated result set can be different
            if (expectedOrdering != null)
            {
                Assert.True(foundProfiles.SequenceEqual(expectedSet));
            }
            else
            {
                Assert.Equal(expectedSet.Count, foundProfiles.Count);
            }
        }
    }
}
