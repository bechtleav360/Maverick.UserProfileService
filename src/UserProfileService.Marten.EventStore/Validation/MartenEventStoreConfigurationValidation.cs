using Microsoft.Extensions.Options;
using UserProfileService.Adapter.Marten.Validation;
using UserProfileService.Marten.EventStore.Options;

namespace UserProfileService.Marten.EventStore.Validation;

/// <summary>
///     A Class used to validate the configuration to connect to Marten Event store.
/// </summary>
public class MartenEventStoreConfigurationValidation : IValidateOptions<MartenEventStoreOptions>
{
    /// <inheritdoc cref="IValidateOptions{TOptions}" />
    public ValidateOptionsResult Validate(string name, MartenEventStoreOptions? options)
    {
        if (options == null)
        {
            return ValidateOptionsResult.Fail(
                $"{nameof(options)} validation error occurred: Options object not provided!");
        }

        // check the base object
        List<string> validationErrors = MartenOptionsValidation.GetValidationErrorMessages(options);

        if (string.IsNullOrWhiteSpace(options.SubscriptionName))
        {
            validationErrors.Add(
                $"Configuration error concerning '{nameof(options.SubscriptionName)}': It should not be null, empty or consist only of white-space characters");
        }

        if (string.IsNullOrWhiteSpace(options.StreamNamePrefix))
        {
            validationErrors.Add(
                $"Configuration error concerning '{nameof(options.StreamNamePrefix)}': It should not be null, empty or consist only of white-space characters");
        }

        if (validationErrors.Any())
        {
            return ValidateOptionsResult.Fail(
                $"{nameof(options)} validation error(s) occurred:{Environment.NewLine}{string.Join(Environment.NewLine, validationErrors.Select((v, i) => $"{i + 1}. {v}"))}");
        }

        return ValidateOptionsResult.Success;
    }
}
