using FluentValidation;
using UserProfileService.Events.Payloads.V2;

namespace UserProfileService.Saga.Validation.Fluent.V2;

/// <summary>
///     Defines fluent validation rules for <see cref="ProfileIdentifierPayload" />.
/// </summary>
public class ProfileIdentifierPayloadValidator : AbstractValidator<ProfileIdentifierPayload>
{
    /// <summary>
    ///     Create an instance of <see cref="ProfileIdentifierPayloadValidator" />.
    /// </summary>
    public ProfileIdentifierPayloadValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ProfileKind).IsInEnum();

        RuleFor(x => x.ExternalIds)
            .NotNull()
            .ForEach(
                e =>
                    e.NotNull().SetValidator(new ExternalIdentifierValidator()));
    }
}
