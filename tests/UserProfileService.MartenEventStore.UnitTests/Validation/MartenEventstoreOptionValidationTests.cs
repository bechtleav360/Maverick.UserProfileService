using FluentAssertions;
using Microsoft.Extensions.Options;
using UserProfileService.Marten.EventStore.Options;
using UserProfileService.Marten.EventStore.Validation;
using UserProfileService.MartenEventStore.UnitTests.TestData;

namespace UserProfileService.MartenEventStore.UnitTests.Validation;

public class MartenEventStoreOptionValidationTests
{
    [Theory]
    [ClassData(typeof(MartenEventStoreOptionsTestData))]
    public void Marten_Configuration_Validation_should_work(
        MartenEventStoreOptions options,
        bool validationSucceeded)
    {
        var validator = new MartenEventStoreConfigurationValidation();
        ValidateOptionsResult result = validator.Validate(string.Empty, options);

        result.Should()
            .BeEquivalentTo(
                new
                {
                    Succeeded = validationSucceeded
                });
    }
}
