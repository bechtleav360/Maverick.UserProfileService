using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Maverick.UserProfileService.Models.Annotations
{
    /// <summary>
    ///     Defines that the underlying array or list of strings should at least contain one string, that <br />
    ///     is not empty or null and that contains other characters that just whitespaces.<br />
    ///     If the field itself is null, the validation will be skipped. Therefore <see cref="RequiredAttribute" /> should be
    ///     used.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class AtLeastOneValidStringAttribute : ValidationAttribute
    {
        private static bool TryConvert(
            object value,
            out IList<string> stringCollection)
        {
            if (value is IList<string> generic)
            {
                stringCollection = generic;

                return true;
            }

            stringCollection = default;

            return false;
        }

        /// <summary>
        ///     Determines whether a specified object is valid.
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

            if (!TryConvert(value, out IList<string> converted))
            {
                throw new InvalidOperationException(
                    "Unsupported type of parameter 'value': Only IList<string> can be checked by this method.");
            }

            return converted.Any(val => !string.IsNullOrWhiteSpace(val))
                ? ValidationResult.Success
                : new ValidationResult(
                    $"Value of {validationContext.MemberName} invalid. It should contain at least one valid string.");
        }
    }
}
