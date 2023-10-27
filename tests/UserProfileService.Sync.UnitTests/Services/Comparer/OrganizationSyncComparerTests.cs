using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.V2.Extensions;
using UserProfileService.Sync.Abstraction.Models;
using UserProfileService.Sync.Abstraction.Models.Entities;
using UserProfileService.Sync.Services.Comparer;
using UserProfileService.Sync.UnitTests.MockDataBuilder;
using Xunit;

namespace UserProfileService.Sync.UnitTests.Services.Comparer
{
    public class OrganizationSyncComparerTests
    {
        private readonly OrganizationSyncComparer _comparer;

        public OrganizationSyncComparerTests()
        {
            _comparer = new OrganizationSyncComparer(new LoggerFactory());
        }

        private OrganizationSync GenerateOrganizationSync()
        {
            GroupSync group = new GroupBuilder()
                .GenerateSampleData()
                .BuildSyncModel();

            var mapper = new Mapper(
                new MapperConfiguration(
                    cfg =>
                    {
                        cfg.CreateMap<KeyProperties, ExternalIdentifier>().ReverseMap();

                        cfg.CreateMap<GroupSync, OrganizationSync>();
                    }));

            return mapper.Map<OrganizationSync>(group);
        }

        private (OrganizationSync, OrganizationSync) GenerateSourceAndTargetOrganization()
        {
            OrganizationSync sourceOrganization = GenerateOrganizationSync();

            OrganizationSync targetOrganization = sourceOrganization.CloneJson();

            return (sourceOrganization, targetOrganization);
        }

        [Fact]
        public void CompareObject_Should_Be_Equal()
        {
            // Arrange
            (OrganizationSync sourceOrganization, OrganizationSync targetOrganization) =
                GenerateSourceAndTargetOrganization();

            // Act
            bool equal = _comparer.CompareObject(
                sourceOrganization,
                targetOrganization,
                out IDictionary<string, object> modifiedProperties);

            // Assert
            Assert.True(equal);
            Assert.Empty(modifiedProperties);
        }

        [Fact]
        public void CompareObject_Should_Be_Equal_IfBothNull()
        {
            // Arrange
            OrganizationSync sourceOrganization = null;
            OrganizationSync targetOrganization = null;

            // Act
            bool equal = _comparer.CompareObject(
                sourceOrganization,
                targetOrganization,
                out IDictionary<string, object> modifiedProperties);

            // Assert
            Assert.True(equal);
            Assert.Empty(modifiedProperties);
        }

        [Fact]
        public void CompareObject_Should_Be_Equal_If_ExternalIdsDifferentOrder()
        {
            // Arrange
            (OrganizationSync sourceOrganization, OrganizationSync targetOrganization) =
                GenerateSourceAndTargetOrganization();

            sourceOrganization.ExternalIds = sourceOrganization.ExternalIds.OrderBy(o => o.Id).ToList();
            targetOrganization.ExternalIds = targetOrganization.ExternalIds.OrderByDescending(o => o.Id).ToList();

            // Act
            bool equal = _comparer.CompareObject(
                sourceOrganization,
                targetOrganization,
                out IDictionary<string, object> modifiedProperties);

            // Assert
            Assert.True(equal);
            Assert.Empty(modifiedProperties);
        }

        [Fact]
        public void CompareObject_Should_Be_Equal_IfNotModifiablePropertyChanged()
        {
            // Arrange
            (OrganizationSync sourceOrganization, OrganizationSync targetOrganization) =
                GenerateSourceAndTargetOrganization();

            targetOrganization.Source = "4711";

            // Act
            bool equal = _comparer.CompareObject(
                sourceOrganization,
                targetOrganization,
                out IDictionary<string, object> modifiedProperties);

            // Assert
            Assert.True(equal);
            Assert.Empty(modifiedProperties);
        }

        [Fact]
        public void CompareObject_Should_Be_NotEqual()
        {
            // Arrange
            OrganizationSync sourceOrganization = GenerateOrganizationSync();
            OrganizationSync targetOrganization = GenerateOrganizationSync();

            // Act
            bool equal = _comparer.CompareObject(
                sourceOrganization,
                targetOrganization,
                out IDictionary<string, object> modifiedProperties);

            // Assert
            Assert.False(equal);
            Assert.NotEmpty(modifiedProperties);
        }

        [Fact]
        public void CompareObject_Should_Be_NotEqual_IfSourceIsNull()
        {
            // Arrange
            OrganizationSync sourceOrganization = null;

            OrganizationSync targetOrganization = GenerateOrganizationSync();

            // Act
            bool equal = _comparer.CompareObject(
                sourceOrganization,
                targetOrganization,
                out IDictionary<string, object> modifiedProperties);

            // Assert
            Assert.False(equal);
            Assert.Empty(modifiedProperties);
        }

        [Fact]
        public void CompareObject_Should_Be_NotEqual_IfTargetIsNull()
        {
            // Arrange
            OrganizationSync sourceOrganization = GenerateOrganizationSync();

            OrganizationSync targetOrganization = null;

            // Act
            bool equal = _comparer.CompareObject(
                sourceOrganization,
                targetOrganization,
                out IDictionary<string, object> modifiedProperties);

            // Assert
            Assert.False(equal);
            Assert.Empty(modifiedProperties);
        }

        [Fact]
        public void CompareObject_Should_Be_NotEqual_IfWeightIsDifferent()
        {
            // Arrange
            (OrganizationSync sourceOrganization, OrganizationSync targetOrganization) =
                GenerateSourceAndTargetOrganization();

            sourceOrganization.Weight = targetOrganization.Weight * 5;

            // Act
            bool equal = _comparer.CompareObject(
                sourceOrganization,
                targetOrganization,
                out IDictionary<string, object> modifiedProperties);

            // Assert
            Assert.False(equal);
            Assert.Single(modifiedProperties);
            Assert.Contains(modifiedProperties, t => t.Key == nameof(OrganizationBasic.Weight));
        }

        [Fact]
        public void CompareObject_Should_Be_NotEqual_IfIsSystemIsDifferent()
        {
            // Arrange
            (OrganizationSync sourceOrganization, OrganizationSync targetOrganization) =
                GenerateSourceAndTargetOrganization();

            sourceOrganization.IsSystem = !targetOrganization.IsSystem;

            // Act
            bool equal = _comparer.CompareObject(
                sourceOrganization,
                targetOrganization,
                out IDictionary<string, object> modifiedProperties);

            // Assert
            Assert.False(equal);
            Assert.Single(modifiedProperties);
            Assert.Contains(modifiedProperties, t => t.Key == nameof(OrganizationBasic.IsSystem));
        }

        [Fact]
        public void CompareObject_Should_Be_NotEqual_If_ExternalIdsDifferent()
        {
            // Arrange
            (OrganizationSync sourceOrganization, OrganizationSync targetOrganization) =
                GenerateSourceAndTargetOrganization();

            targetOrganization.ExternalIds.Add(new KeyProperties("4711", "Cologne"));

            // Act
            bool equal = _comparer.CompareObject(
                sourceOrganization,
                targetOrganization,
                out IDictionary<string, object> modifiedProperties);

            // Assert
            Assert.False(equal);
            Assert.Single(modifiedProperties);
            Assert.Contains(modifiedProperties, t => t.Key == nameof(OrganizationBasic.ExternalIds));
        }
    }
}
