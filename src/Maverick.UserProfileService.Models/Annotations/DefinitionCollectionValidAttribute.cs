using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Maverick.UserProfileService.Models.RequestModels;

namespace Maverick.UserProfileService.Models.Annotations
{
    /// <summary>
    ///     Defines that a <see cref="Definitions" /> instance should be validated.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class DefinitionCollectionValidAttribute : ValidationAttribute
    {
        private static bool Validate(
            Definitions definition,
            int increment,
            ValidationContext context,
            StringBuilder errorMessage,
            List<string> invalidMembers)
        {
            var validationResult = new List<ValidationResult>();

            if (definition == null)
            {
                errorMessage.AppendLine($"{context.MemberName}[{increment}]: Instance should not be null.");

                return false;
            }

            bool success =
                Validator.TryValidateObject(definition, new ValidationContext(definition), validationResult, true);

            if (success)
            {
                return true;
            }

            foreach (ValidationResult result in validationResult)
            {
                invalidMembers.AddRange(result.MemberNames.Select(name => $"{context.MemberName}[{increment}].{name}"));
                errorMessage.AppendLine($"{context.MemberName}[{increment}]: {result.ErrorMessage}");
            }

            return false;
        }

        /// <summary>
        ///     Validates the object.
        /// </summary>
        /// <remarks>
        ///     This method returns <c>true</c> if the <paramref name="value" /> is null.
        ///     It is assumed the <see cref="RequiredAttribute" /> is used if the value may not be null.
        /// </remarks>
        /// <param name="value">The value to validate.</param>
        /// <param name="validationContext">
        ///     A <see cref="ValidationContext" /> instance that provides
        ///     context about the validation operation, such as the object and member being validated.
        /// </param>
        /// <returns>
        ///     When validation is valid, <see cref="ValidationResult.Success" />.
        ///     <para>
        ///         When validation is invalid, an instance of <see cref="ValidationResult" />.
        ///     </para>
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     is thrown if the current attribute is malformed or <br />
        ///     if type of <paramref name="value" /> does not equal <see cref="IList{T}" /> of strings.
        /// </exception>
        protected override ValidationResult IsValid(
            object value,
            ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            if (value is not IList<Definitions> definitions)
            {
                throw new InvalidOperationException(
                    "Unsupported type of parameter 'value'. Only Definitions types can be used in this method.");
            }

            if (definitions.Count == 0)
            {
                return new ValidationResult(
                    $"Value of {validationContext.MemberName} invalid! Collection should not be empty.");
            }

            var errorMessageBuilder = new StringBuilder();
            var invalidMembers = new List<string>();

            bool[] final = definitions
                .Select((def, i) => Validate(def, i, validationContext, errorMessageBuilder, invalidMembers))
                .ToArray();

            return final.Any(elem => elem)
                ? ValidationResult.Success
                : new ValidationResult(errorMessageBuilder.ToString(), invalidMembers);
        }
    }
}
