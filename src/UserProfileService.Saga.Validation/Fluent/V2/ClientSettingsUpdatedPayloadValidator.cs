using FluentValidation;
using UserProfileService.Events.Payloads.V2;

namespace UserProfileService.Saga.Validation.Fluent.V2;

/// <summary>
///     Defines fluent validation rules for <see cref="ClientSettingsUpdatedPayload" />.
/// </summary>
public class ClientSettingsUpdatedPayloadValidator : AbstractValidator<ClientSettingsUpdatedPayload>
{
    /// <summary>
    ///     Create an instance of <see cref="ClientSettingsUpdatedPayloadValidator" />.
    /// </summary>
    public ClientSettingsUpdatedPayloadValidator()
    {
        RuleFor(x => x.Key).NotEmpty();
        RuleFor(x => x.Resource).NotNull().SetValidator(new ProfileIdentValidator());
        RuleFor(x => x.Settings).NotNull();
    }
}
