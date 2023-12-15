using System;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.Modifiable;
using Maverick.UserProfileService.Models.RequestModels;
using FluentAssertions;
using Xunit;

namespace UserProfileService.IntegrationTests.Utilities
{
    internal static class UpsAssert
    {
        internal static void Equal(string id, CreateUserRequest expected, User actual)
        {
            Equal(id, expected, (UserBasic)actual);

            Assert.Empty(actual.MemberOf);
            Assert.NotEmpty(actual.CustomPropertyUrl);
        }

        internal static void Equal(string id, CreateUserRequest expected, UserBasic actual)
        {
            var userUpdateRequest = new UserModifiableProperties
            {
                DisplayName = expected.DisplayName,
                Email = expected.Email,
                UserName = expected.UserName,
                UserStatus = expected.UserStatus,
                FirstName = expected.FirstName,
                LastName = expected.LastName
            };

            Equal(id, userUpdateRequest, actual);

            Assert.Equal(expected.Name, actual.Name);
        }

        internal static void Equal(string id, UserModifiableProperties expected, UserView actual)
        {
            Equal(id, expected, (UserBasic)actual);

            Assert.Empty(actual.MemberOf);
            Assert.Empty(actual.Functions);
        }

        internal static void Equal(string id, UserModifiableProperties expected, UserBasic actual)
        {
            Assert.Equal(id, actual.Id);

            Assert.Equal(expected.FirstName, actual.FirstName);
            Assert.Equal(expected.LastName, actual.LastName);
            Assert.Equal(expected.Email, actual.Email, StringComparer.OrdinalIgnoreCase);
            Assert.Equal(expected.UserName, actual.UserName);
            Assert.Equal(expected.UserStatus, actual.UserStatus);

            Assert.Equal("Api", actual.Source);

            Assert.NotEqual(DateTime.MinValue, actual.CreatedAt);
            Assert.NotEqual(DateTime.MinValue, actual.UpdatedAt);

            Assert.NotEmpty(actual.ExternalIds);
            Assert.NotEmpty(actual.TagUrl);
            Assert.NotEmpty(actual.ImageUrl);

            Assert.Null(actual.SynchronizedAt);
        }

        internal static void Equal(string id, User expected, UserView actual)
        {
            Assert.Equal(id, actual.Id);

            Assert.NotEqual(DateTime.MinValue, actual.CreatedAt);
            Assert.NotEqual(DateTime.MinValue, actual.UpdatedAt);
            Assert.Equal("Api", actual.Source);

            expected.Should().BeEquivalentTo(actual);
        }

        internal static void Equal(string id, CreateGroupRequest expected, GroupView actual)
        {
            Equal(id, expected, (GroupBasic)actual);

            Assert.Equal(expected.Members.Count, actual.ChildrenCount);
            Assert.Equal(expected.Members.Count != 0, actual.HasChildren);
        }

        internal static void Equal(string id, CreateGroupRequest expected, GroupBasic actual)
        {
            var groupUpdateRequest = new GroupModifiableProperties
            {
                DisplayName = expected.DisplayName,
                IsSystem = expected.IsSystem,
                Weight = expected.Weight
            };

            Equal(id, groupUpdateRequest, actual);

            Assert.Equal(expected.Name, actual.Name);
        }

        internal static void Equal(string id, GroupModifiableProperties expected, GroupView actual)
        {
            Equal(id, expected, (GroupBasic)actual);

            Assert.Empty(actual.Tags);
        }

        internal static void Equal(string id, GroupModifiableProperties expected, GroupBasic actual)
        {
            Assert.Equal(id, actual.Id);

            Assert.Equal(expected.DisplayName, actual.DisplayName);
            Assert.Equal(expected.IsSystem, actual.IsSystem);
            Assert.Equal(expected.Weight, actual.Weight);

            Assert.Equal("Api", actual.Source);

            Assert.NotEqual(DateTime.MinValue, actual.CreatedAt);
            Assert.NotEqual(DateTime.MinValue, actual.UpdatedAt);

            Assert.NotEmpty(actual.ExternalIds);
            Assert.NotEmpty(actual.TagUrl);
            Assert.NotEmpty(actual.ImageUrl);

            Assert.Null(actual.SynchronizedAt);
        }
    }
}
