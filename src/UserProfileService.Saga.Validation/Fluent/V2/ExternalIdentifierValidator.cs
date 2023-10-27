using FluentValidation;
using Maverick.UserProfileService.Models.Models;

namespace UserProfileService.Saga.Validation.Fluent.V2;

/// <summary>
///     Defines fluent validation rules for <see cref="ExternalIdentifier" />.
/// </summary>
public class ExternalIdentifierValidator : AbstractValidator<ExternalIdentifier>
{
    /// <summary>
    ///     Create an instance of <see cref="ExternalIdentifierValidator" />.
    /// </summary>
    public ExternalIdentifierValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Source).NotEmpty();
    }
}
