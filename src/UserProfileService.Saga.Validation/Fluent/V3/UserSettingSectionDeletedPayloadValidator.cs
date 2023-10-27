using FluentValidation;
using UserProfileService.Events.Payloads.V3;

namespace UserProfileService.Saga.Validation.Fluent.V3;

/// <summary>
///     Defines fluent validation rules for <see cref="UserSettingSectionDeletedPayload" />.
/// </summary>
public class UserSettingSectionDeletedPayloadValidator : AbstractValidator<UserSettingSectionDeletedPayload>
{
    /// <summary>
    ///     Create an instance of <see cref="UserSettingSectionDeletedPayloadValidator" />.
    /// </summary>
    public UserSettingSectionDeletedPayloadValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.SectionName).NotEmpty();
    }
}
