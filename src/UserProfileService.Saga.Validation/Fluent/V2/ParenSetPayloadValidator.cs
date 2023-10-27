using FluentValidation;
using UserProfileService.Events.Payloads.V2;

namespace UserProfileService.Saga.Validation.Fluent.V2;

/// <summary>
///     Defines fluent validation rules for <see cref="ParentSetPayload" />.
/// </summary>
public class ParenSetPayloadValidator : AbstractValidator<ParentSetPayload>
{
    /// <summary>
    ///     Create an instance of <see cref="ParenSetPayloadValidator" />.
    /// </summary>
    public ParenSetPayloadValidator()
    {
        RuleFor(x => x.ParentId).NotEmpty();

        RuleFor(x => x.SecOIds)
            .Must(x => x != null && x.Length >= 1)
            .WithMessage("At least one security object must be specified.");
    }
}
