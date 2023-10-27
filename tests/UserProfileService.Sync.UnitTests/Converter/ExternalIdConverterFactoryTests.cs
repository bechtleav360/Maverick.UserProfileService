using System.Collections.Generic;
using FluentAssertions;
using UserProfileService.Sync.Abstraction.Configurations;
using UserProfileService.Sync.Abstraction.Configurations.Implementations;
using UserProfileService.Sync.Abstraction.Models;
using UserProfileService.Sync.Converter;
using Xunit;

namespace UserProfileService.Sync.UnitTests.Converter
{
    public class ExternalIdConverterFactoryTests
    {
        [Fact]
        public void CreateConverterFactoryTests()
        {
            // Arrange
            var factory = new ExternalIdConverterFactory<ISyncModel>();

            var source = new Dictionary<string, SynchronizationOperations>
            {
                {
                    "users", new SynchronizationOperations
                    {
                        Converter = new ConverterConfiguration
                        {
                            ConverterType = ConverterType.Prefix,
                            ConverterProperties = new Dictionary<string, string>
                            {
                                { "Prefix", "Test_KK" }
                            }
                        }
                    }
                }
            };

            var sourceSystemConfig = new SourceSystemConfiguration
            {
                Source = source
            };

            var currentSagaStep = "users";

            // Act
            var converter = (PrefixConverter<ISyncModel>)factory.CreateConverter(sourceSystemConfig, currentSagaStep);

            // Assert
            converter.Should()
                .NotBeNull()
                .And.BeOfType<PrefixConverter<ISyncModel>>();
        }
    }
}
