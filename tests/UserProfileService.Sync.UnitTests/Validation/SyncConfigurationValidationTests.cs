using FluentAssertions;
using Microsoft.Extensions.Options;
using UserProfileService.Sync.Abstraction.Configurations;
using UserProfileService.Sync.UnitTests.Validation.TestData;
using UserProfileService.Sync.Validation;
using Xunit;

namespace UserProfileService.Sync.UnitTests.Validation
{
    public class SyncConfigurationValidationTests
    {
        [Theory]
        [ClassData(typeof(SyncConfigurationCorrectTestData))]
        public void SyncConfigValidationWithCorrectParameter_should_succeed(
            SyncConfiguration options,
            bool validationSucceeded)
        {
            var validator = new SyncConfigurationValidation();
            ValidateOptionsResult result = validator.Validate(string.Empty, options);

            result.Should()
                .BeEquivalentTo(
                    new
                    {
                        Succeeded = validationSucceeded
                    });
        }
    }
}
