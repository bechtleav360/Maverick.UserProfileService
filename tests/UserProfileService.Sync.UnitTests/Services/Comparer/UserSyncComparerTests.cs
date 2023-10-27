using System.Collections.Generic;
using System.Linq;
using Maverick.UserProfileService.Models.BasicModels;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.V2.Extensions;
using UserProfileService.Sync.Abstraction.Models;
using UserProfileService.Sync.Abstraction.Models.Entities;
using UserProfileService.Sync.Services.Comparer;
using UserProfileService.Sync.UnitTests.MockDataBuilder;
using Xunit;

namespace UserProfileService.Sync.UnitTests.Services.Comparer
{
    public class UserSyncComparerTests
    {
        private readonly UserSyncComparer _comparer;

        public UserSyncComparerTests()
        {
            _comparer = new UserSyncComparer(new LoggerFactory());
        }

        private (UserSync, UserSync) GenerateSourceAndTargetUser()
        {
            UserSync sourceUser = new UserBuilder()
                .GenerateSampleData()
                .BuildSyncModel();

            UserSync targetUser = sourceUser.CloneJson();

            return (sourceUser, targetUser);
        }

        [Fact]
        public void CompareObject_Should_Be_Equal()
        {
            // Arrange
            (UserSync sourceUser, UserSync targetUser) = GenerateSourceAndTargetUser();

            // Act
            bool equal = _comparer.CompareObject(
                sourceUser,
                targetUser,
                out IDictionary<string, object> modifiedProperties);

            // Assert
            Assert.True(equal);
            Assert.Empty(modifiedProperties);
        }

        [Fact]
        public void CompareObject_Should_Be_Equal_IfBothNull()
        {
            // Arrange
            UserSync sourceUser = null;
            UserSync targetUser = null;

            // Act
            bool equal = _comparer.CompareObject(
                sourceUser,
                targetUser,
                out IDictionary<string, object> modifiedProperties);

            // Assert
            Assert.True(equal);
            Assert.Empty(modifiedProperties);
        }

        [Fact]
        public void CompareObject_Should_Be_Equal_If_ExternalIdsDifferentOrder()
        {
            // Arrange
            (UserSync sourceUser, UserSync targetUser) = GenerateSourceAndTargetUser();

            sourceUser.ExternalIds = sourceUser.ExternalIds.OrderBy(o => o.Id).ToList();
            targetUser.ExternalIds = targetUser.ExternalIds.OrderByDescending(o => o.Id).ToList();

            // Act
            bool equal = _comparer.CompareObject(
                sourceUser,
                targetUser,
                out IDictionary<string, object> modifiedProperties);

            // Assert
            Assert.True(equal);
            Assert.Empty(modifiedProperties);
        }

        [Fact]
        public void CompareObject_Should_Be_Equal_IfNotModifiablePropertyChanged()
        {
            // Arrange
            (UserSync sourceUser, UserSync targetUser) = GenerateSourceAndTargetUser();

            targetUser.Source = "4711";

            // Act
            bool equal = _comparer.CompareObject(
                sourceUser,
                targetUser,
                out IDictionary<string, object> modifiedProperties);

            // Assert
            Assert.True(equal);
            Assert.Empty(modifiedProperties);
        }

        [Fact]
        public void CompareObject_Should_Be_NotEqual()
        {
            // Arrange
            UserSync sourceUser = new UserBuilder()
                .GenerateSampleData()
                .BuildSyncModel();

            UserSync targetUser = new UserBuilder()
                .GenerateSampleData()
                .BuildSyncModel();

            // Act
            bool equal = _comparer.CompareObject(
                sourceUser,
                targetUser,
                out IDictionary<string, object> modifiedProperties);

            // Assert
            Assert.False(equal);
            Assert.NotEmpty(modifiedProperties);
        }

        [Fact]
        public void CompareObject_Should_Be_NotEqual_IfSourceIsNull()
        {
            // Arrange
            UserSync sourceUser = null;

            UserSync targetUser = new UserBuilder()
                .GenerateSampleData()
                .BuildSyncModel();

            // Act
            bool equal = _comparer.CompareObject(
                sourceUser,
                targetUser,
                out IDictionary<string, object> modifiedProperties);

            // Assert
            Assert.False(equal);
            Assert.Empty(modifiedProperties);
        }

        [Fact]
        public void CompareObject_Should_Be_NotEqual_IfTargetIsNull()
        {
            // Arrange
            UserSync sourceUser = new UserBuilder()
                .GenerateSampleData()
                .BuildSyncModel();

            UserSync targetUser = null;

            // Act
            bool equal = _comparer.CompareObject(
                sourceUser,
                targetUser,
                out IDictionary<string, object> modifiedProperties);

            // Assert
            Assert.False(equal);
            Assert.Empty(modifiedProperties);
        }

        [Fact]
        public void CompareObject_Should_Be_NotEqual_IfNameIsDifferent()
        {
            // Arrange
            (UserSync sourceUser, UserSync targetUser) = GenerateSourceAndTargetUser();
            sourceUser.FirstName = "New FirstName";
            sourceUser.LastName = "New LastName";

            // Act
            bool equal = _comparer.CompareObject(
                sourceUser,
                targetUser,
                out IDictionary<string, object> modifiedProperties);

            // Assert
            Assert.False(equal);
            Assert.Equal(2, modifiedProperties.Count);
            Assert.Contains(modifiedProperties, t => t.Key == nameof(UserBasic.FirstName));
            Assert.Contains(modifiedProperties, t => t.Key == nameof(UserBasic.LastName));
        }

        [Fact]
        public void CompareObject_Should_Be_NotEqual_If_ExternalIdsDifferent()
        {
            // Arrange
            (UserSync sourceUser, UserSync targetUser) = GenerateSourceAndTargetUser();

            targetUser.ExternalIds.Add(new KeyProperties("4711", "Cologne"));

            // Act
            bool equal = _comparer.CompareObject(
                sourceUser,
                targetUser,
                out IDictionary<string, object> modifiedProperties);

            // Assert
            Assert.False(equal);
            Assert.Single(modifiedProperties);
            Assert.Contains(modifiedProperties, t => t.Key == nameof(UserBasic.ExternalIds));
        }
    }
}
