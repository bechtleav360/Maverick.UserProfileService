using System;
using FluentValidation;
using UserProfileService.Events.Payloads.V2;

namespace UserProfileService.Saga.Validation.Fluent.V2;

/// <summary>
///     Defines fluent validation rules for <see cref="GroupCreatedPayload" />.
/// </summary>
public class GroupCreatedPayloadValidator : AbstractValidator<GroupCreatedPayload>
{
    /// <summary>
    ///     Create an instance of <see cref="GroupCreatedPayloadValidator" />.
    /// </summary>
    public GroupCreatedPayloadValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.DisplayName).NotEmpty();
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Members)
            .NotNull()
            .ForEach(x => x.NotNull().SetValidator(new ConditionObjectIdentValidator(DateTime.UtcNow)));
    }
}
