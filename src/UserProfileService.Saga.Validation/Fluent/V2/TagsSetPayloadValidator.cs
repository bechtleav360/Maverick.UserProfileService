using FluentValidation;
using UserProfileService.Events.Payloads.V2;

namespace UserProfileService.Saga.Validation.Fluent.V2;

/// <summary>
///     Defines fluent validation rules for <see cref="TagsSetPayload" />.
/// </summary>
public class TagsSetPayloadValidator : AbstractValidator<TagsSetPayload>
{
    /// <summary>
    ///     Create an instance of <see cref="TagsSetPayloadValidator" />.
    /// </summary>
    public TagsSetPayloadValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Tags)
            .Must(x => x != null && x.Length >= 1)
            .WithMessage("At least one tag must be specified.")
            .ForEach(
                t =>
                    t.NotNull().SetValidator(new TagAssignmentValidator()));
    }
}
