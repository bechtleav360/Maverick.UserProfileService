using FluentValidation;
using UserProfileService.Events.Payloads.V2;

namespace UserProfileService.Saga.Validation.Fluent.V2;

/// <summary>
///     Defines fluent validation rules for <see cref="ClientSettingsDeletedPayload" />.
/// </summary>
public class ClientSettingsDeletedPayloadValidator : AbstractValidator<ClientSettingsDeletedPayload>
{
    /// <summary>
    ///     Create an instance of <see cref="ClientSettingsDeletedPayloadValidator" />.
    /// </summary>
    public ClientSettingsDeletedPayloadValidator()
    {
        RuleFor(x => x.Key).NotEmpty();
        RuleFor(x => x.Resource).NotNull().SetValidator(new ProfileIdentValidator());
    }
}
