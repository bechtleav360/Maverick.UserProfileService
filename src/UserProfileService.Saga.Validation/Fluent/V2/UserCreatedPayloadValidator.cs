using FluentValidation;
using UserProfileService.Events.Payloads.V2;

namespace UserProfileService.Saga.Validation.Fluent.V2;

/// <summary>
///     Defines fluent validation rules for <see cref="UserCreatedPayload" />.
/// </summary>
public class UserCreatedPayloadValidator : AbstractValidator<UserCreatedPayload>
{
    /// <summary>
    ///     Create an instance of <see cref="UserCreatedPayloadValidator" />.
    /// </summary>
    public UserCreatedPayloadValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.DisplayName).NotEmpty();
        RuleFor(x => x.Source).NotEmpty();

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}
