using System;
using FluentValidation;
using Maverick.UserProfileService.Models.Models;

namespace UserProfileService.Saga.Validation.Fluent.V2;

/// <summary>
///     Defines fluent validation rules for <see cref="ObjectIdent" />.
/// </summary>
public class ConditionObjectIdentValidator : AbstractValidator<ConditionObjectIdent>
{
    /// <summary>
    ///     Create an instance of <see cref="ConditionObjectIdentValidator" />.
    /// </summary>
    /// <param name="rangeConditionDateTime">Time against which the condition is checked.</param>
    public ConditionObjectIdentValidator(DateTime? rangeConditionDateTime = null)
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Type).IsInEnum();

        RuleFor(x => x.Conditions)
            .NotNull()
            .ForEach(
                x =>
                    x.NotNull().SetValidator(new RangeConditionValidator(rangeConditionDateTime)));
    }
}
