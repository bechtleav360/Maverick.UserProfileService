using FluentAssertions;
using Microsoft.Extensions.Options;
using UserProfileService.Redis.Configuration;
using UserProfileService.Redis.Validation;
using Xunit;

namespace UserProfileService.Redis.UnitTests.Validation
{
    public class RedisConfigurationValidationTests
    {
        [Theory]
        [ClassData(typeof(RedisConfigurationTestData))]
        public void RedisConfigValidation_should_work(
            RedisConfiguration options,
            bool validationSucceeded)
        {
            var validator = new RedisConfigurationValidation();
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
