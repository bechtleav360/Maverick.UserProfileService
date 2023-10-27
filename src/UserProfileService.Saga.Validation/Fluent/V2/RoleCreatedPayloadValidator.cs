using FluentValidation;
using UserProfileService.Events.Payloads.V2;

namespace UserProfileService.Saga.Validation.Fluent.V2;

/// <summary>
///     Defines fluent validation rules for <see cref="RoleCreatedPayload" />.
/// </summary>
public class RoleCreatedPayloadValidator : AbstractValidator<RoleCreatedPayload>
{
    /// <summary>
    ///     Create an instance of <see cref="RoleCreatedPayloadValidator" />.
    /// </summary>
    public RoleCreatedPayloadValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();

        RuleForEach(x => x.Permissions)
            .Must((n, m) => !n.DeniedPermissions.Contains(m))
            .WithMessage("The permission is also defined as denied permission.");
    }
}
