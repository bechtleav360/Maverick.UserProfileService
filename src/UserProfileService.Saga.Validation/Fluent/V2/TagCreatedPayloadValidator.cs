using FluentValidation;
using UserProfileService.Events.Payloads.V2;

namespace UserProfileService.Saga.Validation.Fluent.V2;

/// <summary>
///     Defines fluent validation rules for <see cref="TagCreatedPayload" />.
/// </summary>
public class TagCreatedPayloadValidator : AbstractValidator<TagCreatedPayload>
{
    /// <summary>
    ///     Create an instance of <see cref="TagCreatedPayloadValidator" />.
    /// </summary>
    public TagCreatedPayloadValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Source).NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("The name may only be empty if a reference is set.");
    }
}
