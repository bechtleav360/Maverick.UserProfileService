using FluentValidation;
using UserProfileService.Events.Payloads.V2;

namespace UserProfileService.Saga.Validation.Fluent.V2;

/// <summary>
///     Defines fluent validation rules for <see cref="TagsRemovedPayload" />.
/// </summary>
public class TagsRemovedPayloadValidator : AbstractValidator<TagsRemovedPayload>
{
    /// <summary>
    ///     Create an instance of <see cref="TagsRemovedPayloadValidator" />.
    /// </summary>
    public TagsRemovedPayloadValidator()
    {
        RuleFor(x => x.ResourceId).NotEmpty();

        RuleFor(x => x.Tags)
            .Must(x => x != null && x.Length >= 1)
            .WithMessage("At least one tag must be specified to remove.")
            .ForEach(t => t.NotEmpty());
    }
}
