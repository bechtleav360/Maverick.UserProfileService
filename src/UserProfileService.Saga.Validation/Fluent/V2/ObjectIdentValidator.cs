using FluentValidation;
using Maverick.UserProfileService.Models.Models;

namespace UserProfileService.Saga.Validation.Fluent.V2;

/// <summary>
///     Defines fluent validation rules for <see cref="ObjectIdent" />.
/// </summary>
public class ObjectIdentValidator : AbstractValidator<ObjectIdent>
{
    /// <summary>
    ///     Create an instance of <see cref="ObjectIdentValidator" />.
    /// </summary>
    public ObjectIdentValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Type).IsInEnum();
    }
}
