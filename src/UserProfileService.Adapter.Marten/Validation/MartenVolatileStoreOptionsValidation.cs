using Microsoft.Extensions.Options;
using UserProfileService.Adapter.Marten.Options;

namespace UserProfileService.Adapter.Marten.Validation;

/// <summary>
///     Validates <see cref="MartenVolatileStoreOptions" /> options.
/// </summary>
public class MartenVolatileStoreOptionsValidation : IValidateOptions<MartenVolatileStoreOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string name, MartenVolatileStoreOptions options)
    {
        // just return the validation result of the base options class of the current options type
        return new MartenOptionsValidation().Validate(name, options);
    }
}
