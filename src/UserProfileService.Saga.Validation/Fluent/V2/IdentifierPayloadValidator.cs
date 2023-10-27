using FluentValidation;
using UserProfileService.Events.Payloads.V2;

namespace UserProfileService.Saga.Validation.Fluent.V2;

/// <summary>
///     Defines fluent validation rules for <see cref="IdentifierPayload" />.
/// </summary>
public class IdentifierPayloadValidator : AbstractValidator<IdentifierPayload>
{
    /// <summary>
    ///     Create an instance of <see cref="IdentifierPayloadValidator" />.
    /// </summary>
    public IdentifierPayloadValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
