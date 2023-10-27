using FluentValidation;
using UserProfileService.Events.Payloads.V3;

namespace UserProfileService.Saga.Validation.Fluent.V3;

/// <summary>
///     Defines fluent validation rules for <see cref="FunctionCreatedPayload" />.
/// </summary>
public class FunctionCreatedPayloadValidator : AbstractValidator<FunctionCreatedPayload>
{
    /// <summary>
    ///     Create an instance of <see cref="FunctionCreatedPayloadValidator" />.
    /// </summary>
    public FunctionCreatedPayloadValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.RoleId).NotEmpty();
        RuleFor(x => x.OrganizationId).NotEmpty();
    }
}
