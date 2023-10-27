using FluentValidation;
using UserProfileService.Events.Payloads.V2;

namespace UserProfileService.Saga.Validation.Fluent.V2;

/// <summary>
///     Defines fluent validation rules for <see cref="ClientSettingsSetPayload" />.
/// </summary>
public class ClientSettingsSetBatchPayloadValidator : AbstractValidator<ClientSettingsSetBatchPayload>
{
    /// <summary>
    ///     Create an instance of <see cref="ClientSettingsSetBatchPayloadValidator" />.
    /// </summary>
    public ClientSettingsSetBatchPayloadValidator()
    {
        RuleFor(x => x.Key).NotEmpty();

        RuleFor(x => x.Resources)
            .NotNull()
            .Must(x => x != null && x.Length >= 1)
            .WithMessage("At least one resource must be specified.")
            .ForEach(x => x.NotNull().SetValidator(new ProfileIdentValidator()));

        RuleFor(x => x.Settings).NotNull();
    }
}
