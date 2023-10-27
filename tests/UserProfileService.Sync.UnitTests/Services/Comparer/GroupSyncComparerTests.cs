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
    public class GroupSyncComparerTests
    {
        private readonly GroupSyncComparer _comparer;

        public GroupSyncComparerTests()
        {
            _comparer = new GroupSyncComparer(new LoggerFactory());
        }

        private (GroupSync, GroupSync) GenerateSourceAndTargetGroup()
        {
            GroupSync sourceGroup = new GroupBuilder()
                .GenerateSampleData()
                .BuildSyncModel();

            GroupSync targetGroup = sourceGroup.CloneJson();

            return (sourceGroup, targetGroup);
        }

        [Fact]
        public void CompareObject_Should_Be_Equal()
        {
            // Arrange
            (GroupSync sourceGroup, GroupSync targetGroup) = GenerateSourceAndTargetGroup();

            // Act
            bool equal = _comparer.CompareObject(
                sourceGroup,
                targetGroup,
                out IDictionary<string, object> modifiedProperties);

            // Assert
            Assert.True(equal);
            Assert.Empty(modifiedProperties);
        }

        [Fact]
        public void CompareObject_Should_Be_Equal_IfBothNull()
        {
            // Arrange
            GroupSync sourceGroup = null;
            GroupSync targetGroup = null;

            // Act
            bool equal = _comparer.CompareObject(
                sourceGroup,
                targetGroup,
                out IDictionary<string, object> modifiedProperties);

            // Assert
            Assert.True(equal);
            Assert.Empty(modifiedProperties);
        }

        [Fact]
        public void CompareObject_Should_Be_Equal_If_ExternalIdsDifferentOrder()
        {
            // Arrange
            (GroupSync sourceGroup, GroupSync targetGroup) = GenerateSourceAndTargetGroup();

            sourceGroup.ExternalIds = sourceGroup.ExternalIds.OrderBy(o => o.Id).ToList();
            targetGroup.ExternalIds = targetGroup.ExternalIds.OrderByDescending(o => o.Id).ToList();

            // Act
            bool equal = _comparer.CompareObject(
                sourceGroup,
                targetGroup,
                out IDictionary<string, object> modifiedProperties);

            // Assert
            Assert.True(equal);
            Assert.Empty(modifiedProperties);
        }

        [Fact]
        public void CompareObject_Should_Be_Equal_IfNotModifiablePropertyChanged()
        {
            // Arrange
            (GroupSync sourceGroup, GroupSync targetGroup) = GenerateSourceAndTargetGroup();

            targetGroup.Source = "4711";

            // Act
            bool equal = _comparer.CompareObject(
                sourceGroup,
                targetGroup,
                out IDictionary<string, object> modifiedProperties);

            // Assert
            Assert.True(equal);
            Assert.Empty(modifiedProperties);
        }

        [Fact]
        public void CompareObject_Should_Be_NotEqual()
        {
            // Arrange
            GroupSync sourceGroup = new GroupBuilder()
                .GenerateSampleData()
                .BuildSyncModel();

            GroupSync targetGroup = new GroupBuilder()
                .GenerateSampleData()
                .BuildSyncModel();

            // Act
            bool equal = _comparer.CompareObject(
                sourceGroup,
                targetGroup,
                out IDictionary<string, object> modifiedProperties);

            // Assert
            Assert.False(equal);
            Assert.NotEmpty(modifiedProperties);
        }

        [Fact]
        public void CompareObject_Should_Be_NotEqual_IfSourceIsNull()
        {
            // Arrange
            GroupSync sourceGroup = null;

            GroupSync targetGroup = new GroupBuilder()
                .GenerateSampleData()
                .BuildSyncModel();

            // Act
            bool equal = _comparer.CompareObject(
                sourceGroup,
                targetGroup,
                out IDictionary<string, object> modifiedProperties);

            // Assert
            Assert.False(equal);
            Assert.Empty(modifiedProperties);
        }

        [Fact]
        public void CompareObject_Should_Be_NotEqual_IfTargetIsNull()
        {
            // Arrange
            GroupSync sourceGroup = new GroupBuilder()
                .GenerateSampleData()
                .BuildSyncModel();

            GroupSync targetGroup = null;

            // Act
            bool equal = _comparer.CompareObject(
                sourceGroup,
                targetGroup,
                out IDictionary<string, object> modifiedProperties);

            // Assert
            Assert.False(equal);
            Assert.Empty(modifiedProperties);
        }

        [Fact]
        public void CompareObject_Should_Be_NotEqual_IfWeightIsDifferent()
        {
            // Arrange
            (GroupSync sourceGroup, GroupSync targetGroup) = GenerateSourceAndTargetGroup();
            sourceGroup.Weight = targetGroup.Weight * 5;

            // Act
            bool equal = _comparer.CompareObject(
                sourceGroup,
                targetGroup,
                out IDictionary<string, object> modifiedProperties);

            // Assert
            Assert.False(equal);
            Assert.Single(modifiedProperties);
            Assert.Contains(modifiedProperties, t => t.Key == nameof(GroupBasic.Weight));
        }

        [Fact]
        public void CompareObject_Should_Be_NotEqual_IfIsSystemIsDifferent()
        {
            // Arrange
            (GroupSync sourceGroup, GroupSync targetGroup) = GenerateSourceAndTargetGroup();
            sourceGroup.IsSystem = !targetGroup.IsSystem;

            // Act
            bool equal = _comparer.CompareObject(
                sourceGroup,
                targetGroup,
                out IDictionary<string, object> modifiedProperties);

            // Assert
            Assert.False(equal);
            Assert.Single(modifiedProperties);
            Assert.Contains(modifiedProperties, t => t.Key == nameof(GroupBasic.IsSystem));
        }

        [Fact]
        public void CompareObject_Should_Be_NotEqual_If_ExternalIdsDifferent()
        {
            // Arrange
            (GroupSync sourceGroup, GroupSync targetGroup) = GenerateSourceAndTargetGroup();

            targetGroup.ExternalIds.Add(new KeyProperties("4711", "Cologne"));

            // Act
            bool equal = _comparer.CompareObject(
                sourceGroup,
                targetGroup,
                out IDictionary<string, object> modifiedProperties);

            // Assert
            Assert.False(equal);
            Assert.Single(modifiedProperties);
            Assert.Contains(modifiedProperties, t => t.Key == nameof(GroupBasic.ExternalIds));
        }
    }
}
