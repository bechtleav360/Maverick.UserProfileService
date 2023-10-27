using FluentValidation;
using UserProfileService.Events.Payloads.V2;

namespace UserProfileService.Saga.Validation.Fluent.V2;

/// <summary>
///     Defines fluent validation rules for <see cref="PropertiesUpdatedPayload" />.
/// </summary>
public class PropertiesUpdatedPayloadValidator : AbstractValidator<PropertiesUpdatedPayload>
{
    /// <summary>
    ///     Create an instance of <see cref="PropertiesUpdatedPayloadValidator" />.
    /// </summary>
    public PropertiesUpdatedPayloadValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Properties)
            .NotNull()
            .Must(p => p.Count >= 1)
            .WithMessage("At least one changed property must be specified.");
    }
}
