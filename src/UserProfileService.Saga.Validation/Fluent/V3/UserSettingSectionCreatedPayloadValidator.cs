using System.Text.Json.Nodes;
using FluentValidation;
using UserProfileService.Events.Payloads.V3;
using UserProfileService.Saga.Validation.Utilities;

namespace UserProfileService.Saga.Validation.Fluent.V3;

/// <summary>
///     Defines fluent validation rules for <see cref="UserSettingSectionCreatedPayload" />.
/// </summary>
public class UserSettingSectionCreatedPayloadValidator : AbstractValidator<UserSettingSectionCreatedPayload>
{
    /// <summary>
    ///     Create an instance of <see cref="UserSettingSectionCreatedPayloadValidator" />.
    /// </summary>
    public UserSettingSectionCreatedPayloadValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.SectionName).NotEmpty();
        RuleFor(x => x.ValuesAsJsonString).NotEmpty();
        RuleFor(x => x.ValuesAsJsonString).IsValidJson(setup => setup.UseJsonNodeType<JsonArray>());
    }
}
