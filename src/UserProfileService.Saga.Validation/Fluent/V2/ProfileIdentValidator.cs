using FluentValidation;
using Maverick.UserProfileService.Models.Models;

namespace UserProfileService.Saga.Validation.Fluent.V2;

/// <summary>
///     Defines fluent validation rules for <see cref="ProfileIdent" />.
/// </summary>
public class ProfileIdentValidator : AbstractValidator<ProfileIdent>
{
    /// <summary>
    ///     Create an instance of <see cref="ProfileIdentValidator" />.
    /// </summary>
    public ProfileIdentValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ProfileKind).IsInEnum();
    }
}
