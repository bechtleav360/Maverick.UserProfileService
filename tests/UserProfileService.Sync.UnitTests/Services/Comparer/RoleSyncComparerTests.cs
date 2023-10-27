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
    public class RoleSyncComparerTests
    {
        private readonly RoleSyncComparer _comparer;

        public RoleSyncComparerTests()
        {
            _comparer = new RoleSyncComparer(new LoggerFactory());
        }

        private (RoleSync, RoleSync) GenerateSourceAndTargetRole()
        {
            RoleSync sourceRole = new RoleBuilder()
                .GenerateSampleData()
                .BuildSyncModel();

            RoleSync targetRole = sourceRole.CloneJson();

            return (sourceRole, targetRole);
        }

        [Fact]
        public void CompareObject_Should_Be_Equal()
        {
            // Arrange
            (RoleSync sourceRole, RoleSync targetRole) = GenerateSourceAndTargetRole();

            // Act
            bool equal = _comparer.CompareObject(
                sourceRole,
                targetRole,
                out IDictionary<string, object> modifiedProperties);

            // Assert
            Assert.True(equal);
            Assert.Empty(modifiedProperties);
        }

        [Fact]
        public void CompareObject_Should_Be_Equal_IfBothNull()
        {
            // Arrange
            RoleSync sourceRole = null;
            RoleSync targetRole = null;

            // Act
            bool equal = _comparer.CompareObject(
                sourceRole,
                targetRole,
                out IDictionary<string, object> modifiedProperties);

            // Assert
            Assert.True(equal);
            Assert.Empty(modifiedProperties);
        }

        [Fact]
        public void CompareObject_Should_Be_Equal_If_ExternalIdsDifferentOrder()
        {
            // Arrange
            (RoleSync sourceRole, RoleSync targetRole) = GenerateSourceAndTargetRole();

            sourceRole.ExternalIds = sourceRole.ExternalIds.OrderBy(o => o.Id).ToList();
            targetRole.ExternalIds = targetRole.ExternalIds.OrderByDescending(o => o.Id).ToList();

            // Act
            bool equal = _comparer.CompareObject(
                sourceRole,
                targetRole,
                out IDictionary<string, object> modifiedProperties);

            // Assert
            Assert.True(equal);
            Assert.Empty(modifiedProperties);
        }

        [Fact]
        public void CompareObject_Should_Be_Equal_If_PermissionsHaveDifferentOrder()
        {
            // Arrange
            (RoleSync sourceRole, RoleSync targetRole) = GenerateSourceAndTargetRole();

            sourceRole.Permissions = sourceRole.Permissions.OrderBy(o => o).ToList();
            targetRole.Permissions = targetRole.Permissions.OrderByDescending(o => o).ToList();

            sourceRole.DeniedPermissions = sourceRole.DeniedPermissions.OrderBy(o => o).ToList();
            targetRole.DeniedPermissions = targetRole.DeniedPermissions.OrderByDescending(o => o).ToList();

            // Act
            bool equal = _comparer.CompareObject(
                sourceRole,
                targetRole,
                out IDictionary<string, object> modifiedProperties);

            // Assert
            Assert.True(equal);
            Assert.Empty(modifiedProperties);
        }

        [Fact]
        public void CompareObject_Should_Be_NotEqual()
        {
            // Arrange
            RoleSync sourceRole = new RoleBuilder()
                .GenerateSampleData()
                .BuildSyncModel();

            RoleSync targetRole = new RoleBuilder()
                .GenerateSampleData()
                .BuildSyncModel();

            // Act
            bool equal = _comparer.CompareObject(
                sourceRole,
                targetRole,
                out IDictionary<string, object> modifiedProperties);

            // Assert
            Assert.False(equal);
            Assert.NotEmpty(modifiedProperties);
        }

        [Fact]
        public void CompareObject_Should_Be_NotEqual_IfSourceIsNull()
        {
            // Arrange
            RoleSync sourceRole = null;

            RoleSync targetRole = new RoleBuilder()
                .GenerateSampleData()
                .BuildSyncModel();

            // Act
            bool equal = _comparer.CompareObject(
                sourceRole,
                targetRole,
                out IDictionary<string, object> modifiedProperties);

            // Assert
            Assert.False(equal);
            Assert.Empty(modifiedProperties);
        }

        [Fact]
        public void CompareObject_Should_Be_NotEqual_IfTargetIsNull()
        {
            // Arrange
            RoleSync sourceRole = new RoleBuilder()
                .GenerateSampleData()
                .BuildSyncModel();

            RoleSync targetRole = null;

            // Act
            bool equal = _comparer.CompareObject(
                sourceRole,
                targetRole,
                out IDictionary<string, object> modifiedProperties);

            // Assert
            Assert.False(equal);
            Assert.Empty(modifiedProperties);
        }

        [Fact]
        public void CompareObject_Should_Be_NotEqual_IfIsSystemIsDifferent()
        {
            // Arrange
            (RoleSync sourceRole, RoleSync targetRole) = GenerateSourceAndTargetRole();
            sourceRole.IsSystem = !targetRole.IsSystem;

            // Act
            bool equal = _comparer.CompareObject(
                sourceRole,
                targetRole,
                out IDictionary<string, object> modifiedProperties);

            // Assert
            Assert.False(equal);
            Assert.Single(modifiedProperties);
            Assert.Contains(modifiedProperties, t => t.Key == nameof(RoleBasic.IsSystem));
        }

        [Fact]
        public void CompareObject_Should_Be_NotEqual_If_ListAreDifferent()
        {
            // Arrange
            (RoleSync sourceRole, RoleSync targetRole) = GenerateSourceAndTargetRole();

            targetRole.ExternalIds.Add(new KeyProperties("4711", "Cologne"));
            targetRole.Permissions.Add("New permission 4711");
            targetRole.DeniedPermissions.Add("New denied permission 4711");

            // Act
            bool equal = _comparer.CompareObject(
                sourceRole,
                targetRole,
                out IDictionary<string, object> modifiedProperties);

            // Assert
            Assert.False(equal);
            Assert.Equal(3, modifiedProperties.Count);
            Assert.Contains(modifiedProperties, t => t.Key == nameof(RoleBasic.ExternalIds));
            Assert.Contains(modifiedProperties, t => t.Key == nameof(RoleBasic.Permissions));
            Assert.Contains(modifiedProperties, t => t.Key == nameof(RoleBasic.DeniedPermissions));
        }
    }
}
