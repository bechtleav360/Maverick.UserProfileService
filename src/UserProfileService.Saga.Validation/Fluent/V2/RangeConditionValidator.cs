using System;
using FluentValidation;
using Maverick.UserProfileService.Models.Models;

namespace UserProfileService.Saga.Validation.Fluent.V2;

/// <summary>
///     Defines fluent validation rules for <see cref="RangeCondition" />.
/// </summary>
public class RangeConditionValidator : AbstractValidator<RangeCondition>
{
    /// <summary>
    ///     Create an instance of <see cref="RangeConditionValidator" />.
    /// </summary>
    /// <param name="validationTime">Time to validate the end date.</param>
    public RangeConditionValidator(DateTime? validationTime = null)
    {
        RuleFor(x => x.End)
            .GreaterThan(x => x.Start)
            .GreaterThan(x => validationTime ?? DateTime.MinValue)
            .When(x => x.End != null && x.Start != null);
    }
}
