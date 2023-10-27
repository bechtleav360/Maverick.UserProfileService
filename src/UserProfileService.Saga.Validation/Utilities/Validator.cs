using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.EnumModels;
using UserProfileService.Validation.Abstractions;

namespace UserProfileService.Saga.Validation.Utilities;

/// <summary>
///     Generic validator for different use cases.
/// </summary>
public static class Validator
{
    /// <summary>
    ///     Contains all validators related to <see cref="string" />.
    /// </summary>
    public static class String
    {
        /// <summary>
        ///     Checks the given value against the given pattern.
        /// </summary>
        /// <param name="str">Value to check with given pattern.</param>
        /// <param name="pattern">Pattern for validation.</param>
        /// <param name="regexOptions">Options for regex to use.</param>
        /// <param name="member">The name of the property to which the <see cref="ValidationAttribute" /> is assigned.</param>
        /// <returns>Results of validation.</returns>
        public static ValidationResult ValidateWithRegex(
            string str,
            string pattern,
            RegexOptions regexOptions = RegexOptions.IgnoreCase,
            string member = null)
        {
            Guard.IsNotNullOrEmpty(pattern, nameof(pattern));

            bool success = string.IsNullOrWhiteSpace(str)
                || Regex.Match(str, pattern, regexOptions).Success;

            if (success)
            {
                return new ValidationResult();
            }

            var validationResult = new ValidationAttribute(
                member,
                $"The given string '{str}' does not match the regex '{pattern}'");

            return new ValidationResult(validationResult);
        }
    }

    /// <summary>
    ///     Contains all validators related to <see cref="GroupBasic" />.
    /// </summary>
    public static class Group
    {
        /// <summary>
        ///     Checks the given names against the given pattern.
        /// </summary>
        /// <param name="name">Name to check with given pattern.</param>
        /// <param name="displayName">Display name to check with given pattern.</param>
        /// <param name="pattern">Pattern for validation.</param>
        /// <param name="nameMember">The name of the name property to which the <see cref="ValidationAttribute" /> is assigned.</param>
        /// <param name="displayMember">
        ///     The display name of the name property to which the <see cref="ValidationAttribute" /> is
        ///     assigned.
        /// </param>
        /// <returns>Results of validation.</returns>
        public static ValidationResult ValidateNames(
            string name,
            string displayName,
            string pattern,
            string nameMember = null,
            string displayMember = null)
        {
            Guard.IsNotNullOrEmpty(pattern, nameof(pattern));

            ValidationResult nameValidationAttribute = String.ValidateWithRegex(
                name,
                pattern,
                member: nameMember);

            ValidationResult displayNameValidationAttribute = String.ValidateWithRegex(
                displayName,
                pattern,
                member: displayMember);

            List<ValidationAttribute> validationResults =
                nameValidationAttribute.Errors.Concat(displayNameValidationAttribute.Errors).ToList();

            return new ValidationResult(validationResults);
        }
    }

    /// <summary>
    ///     Contains all validators related to <see cref="IProfile" />.
    /// </summary>
    public static class Profile
    {
        /// <summary>
        ///     Checks whether the operation can be performed based on the given values.
        /// </summary>
        /// <param name="isSystem">Indicates whether the entity in the operation is a system entity.</param>
        /// <param name="initiatorType">Indicates who triggered the operation.</param>
        /// <param name="member">The name of the property to which the <see cref="ValidationAttribute" /> is assigned.</param>
        /// <returns>Results of validation.</returns>
        public static ValidationResult ValidateOperationAllowed(
            bool isSystem,
            InitiatorType? initiatorType = null,
            string member = null)
        {
            if (isSystem && initiatorType == InitiatorType.User)
            {
                var validationResultSystem =
                    new ValidationAttribute(
                        member,
                        $"The role could not be deleted, because the initiator is a user and the property {member} is true.");

                return new ValidationResult(validationResultSystem);
            }

            return new ValidationResult();
        }
    }
}
