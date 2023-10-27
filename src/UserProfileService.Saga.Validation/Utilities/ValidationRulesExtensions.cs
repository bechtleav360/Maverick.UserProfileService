using System;
using FluentValidation;

namespace UserProfileService.Saga.Validation.Utilities;

/// <summary>
///     Contains validation rule extensions related to JSON objects.
/// </summary>
public static class ValidationRulesExtensions
{
    /// <summary>
    ///     Defines a 'null' validator on the current rule builder.
    ///     Validation will fail if the property is not null.
    /// </summary>
    /// <typeparam name="T">Type of object being validated</typeparam>
    /// <param name="ruleBuilder">The rule builder on which the validator should be defined</param>
    /// <param name="setup">The validation setup that can be configured.</param>
    /// <returns>The <see cref="IRuleBuilder{T,TProperty}"/> that can be configured.</returns>
    public static IRuleBuilderOptions<T, string> IsValidJson<T>(
        this IRuleBuilder<T, string> ruleBuilder,
        Action<ValidatorBuilder> setup)
    {
        var builder = new ValidatorBuilder();
        setup.Invoke(builder);

        return ruleBuilder.SetValidator(new JsonDocumentValidator<T>(builder.JsonNodeType));
    }
}
