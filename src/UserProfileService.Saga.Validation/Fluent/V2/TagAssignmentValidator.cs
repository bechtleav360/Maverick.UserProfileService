using FluentValidation;
using Maverick.UserProfileService.Models.RequestModels;

namespace UserProfileService.Saga.Validation.Fluent.V2;

/// <summary>
///     Defines fluent validation rules for <see cref="TagAssignment" />.
/// </summary>
public class TagAssignmentValidator : AbstractValidator<TagAssignment>
{
    /// <summary>
    ///     Create an instance of <see cref="TagAssignmentValidator" />.
    /// </summary>
    public TagAssignmentValidator()
    {
        RuleFor(x => x.TagId).NotEmpty();
    }
}
