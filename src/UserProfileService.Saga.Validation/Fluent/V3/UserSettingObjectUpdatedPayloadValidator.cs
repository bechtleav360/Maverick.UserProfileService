using System.Text.Json.Nodes;
using FluentValidation;
using UserProfileService.Events.Payloads.V3;
using UserProfileService.Saga.Validation.Utilities;

namespace UserProfileService.Saga.Validation.Fluent.V3;

/// <summary>
///     Defines fluent validation rules for <see cref="UserSettingObjectUpdatedPayload" />.
/// </summary>
public class UserSettingObjectUpdatedPayloadValidator : AbstractValidator<UserSettingObjectUpdatedPayload>
{
    /// <summary>
    ///     Create an instance of <see cref="UserSettingObjectUpdatedPayloadValidator" />.
    /// </summary>
    public UserSettingObjectUpdatedPayloadValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.SectionName).NotEmpty();
        RuleFor(x => x.ValuesAsJsonString).NotEmpty();
        RuleFor(x => x.SettingObjectId).NotEmpty();
        RuleFor(x => x.ValuesAsJsonString).IsValidJson(setup => setup.UseJsonNodeType<JsonObject>());
    }
}
