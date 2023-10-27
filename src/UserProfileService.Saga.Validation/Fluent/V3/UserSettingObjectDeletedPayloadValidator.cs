using FluentValidation;
using UserProfileService.Events.Payloads.V3;

namespace UserProfileService.Saga.Validation.Fluent.V3;

/// <summary>
///     Defines fluent validation rules for <see cref="UserSettingObjectDeletedPayload" />.
/// </summary>
public class UserSettingObjectDeletedPayloadValidator : AbstractValidator<UserSettingObjectDeletedPayload>
{
    /// <summary>
    ///     Create an instance of <see cref="UserSettingObjectDeletedPayloadValidator" />.
    /// </summary>
    public UserSettingObjectDeletedPayloadValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.SectionName).NotEmpty();
        RuleFor(x => x.SettingObjectId).NotEmpty();
    }
}
