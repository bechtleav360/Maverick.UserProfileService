using FluentValidation;
using UserProfileService.Events.Payloads.V2;

namespace UserProfileService.Saga.Validation.Fluent.V2;

/// <summary>
///     Defines fluent validation rules for <see cref="ClientSettingsSetPayload" />.
/// </summary>
public class ClientSettingsSetPayloadValidator : AbstractValidator<ClientSettingsSetPayload>
{
    /// <summary>
    ///     Create an instance of <see cref="ClientSettingsSetPayloadValidator" />.
    /// </summary>
    public ClientSettingsSetPayloadValidator()
    {
        RuleFor(x => x.Key).NotEmpty();
        RuleFor(x => x.Resource).NotNull().SetValidator(new ProfileIdentValidator());
        RuleFor(x => x.Settings).NotNull();
    }
}
