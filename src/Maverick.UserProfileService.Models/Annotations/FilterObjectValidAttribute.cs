using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Maverick.UserProfileService.Models.RequestModels;

namespace Maverick.UserProfileService.Models.Annotations
{
    /// <summary>
    ///     Validation attribute to assert a string property, field or parameter is not an empty string or does not only
    ///     contains whitespaces.<br />
    ///     If the value is null, the validation will be bypassed. The check null, use the <see cref="RequiredAttribute" /> as
    ///     well.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class FilterObjectValidAttribute : ValidationAttribute
    {
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
        ///     if type of <paramref name="value" /> does not equal string.
        /// </exception>
        protected override ValidationResult IsValid(
            object value,
            ValidationContext validationContext)
        {
            // Automatically pass if value is null. RequiredAttribute should be used to assert a value is not null.
            if (value == null)
            {
                return ValidationResult.Success;
            }

            if (value is not Filter)
            {
                throw new InvalidOperationException(
                    $"Parameter '{nameof(value)}' is not an instance of type Filter, the only type that is supported by this method.");
            }

            var validationResults = new List<ValidationResult>();

            if (Validator.TryValidateObject(value, new ValidationContext(value, null, null), validationResults, true))
            {
                return ValidationResult.Success;
            }

            string message = string.Join(
                Environment.NewLine,
                validationResults.Select(item => item.ErrorMessage));

            return new ValidationResult(
                $"Error: Value of {validationContext.MemberName} invalid:{Environment.NewLine}{message}");
        }
    }
}
