using System;
using FluentValidation;
using UserProfileService.Events.Payloads.V2;

namespace UserProfileService.Saga.Validation.Fluent.V2;

/// <summary>
///     Defines fluent validation rules for <see cref="OrganizationCreatedPayload" />.
/// </summary>
public class OrganizationCreatedPayloadValidator : AbstractValidator<OrganizationCreatedPayload>
{
    /// <summary>
    ///     Create an instance of <see cref="OrganizationCreatedPayloadValidator" />.
    /// </summary>
    public OrganizationCreatedPayloadValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.DisplayName).NotEmpty();
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Members)
            .NotNull()
            .ForEach(x => x.NotNull().SetValidator(new ConditionObjectIdentValidator(DateTime.UtcNow)));
    }
}
