using FluentValidation;
using UserProfileService.Events.Payloads.V2;

namespace UserProfileService.Saga.Validation.Fluent.V2;

/// <summary>
///     Defines fluent validation rules for <see cref="IdentifierCollectionPayload" />.
/// </summary>
public class IdentifierCollectionPayloadValidator : AbstractValidator<IdentifierCollectionPayload>
{
    /// <summary>
    ///     Create an instance of <see cref="IdentifierCollectionPayloadValidator" />.
    /// </summary>
    public IdentifierCollectionPayloadValidator()
    {
        RuleFor(x => x.Ids)
            .Must(x => x != null && x.Length >= 1)
            .WithMessage("At least one id must be specified.")
            .ForEach(i => i.NotEmpty());
    }
}
