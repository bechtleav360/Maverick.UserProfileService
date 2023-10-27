using System;
using UserProfileService.Validation.Abstractions;

namespace UserProfileService.Saga.Validation.Utilities;

/// <summary>
///     Contains extension methods related to <see cref="ValidationResult" />.
/// </summary>
public static class ValidationResultExtension
{
    /// <summary>
    ///     Checks the result of the validation and if the validation is invalid, throws an <see cref="ValidationException" />.
    /// </summary>
    /// <param name="validationResult">Validation result to check.</param>
    /// <exception cref="ValidationException">Error thrown on invalid validation.</exception>
    public static void CheckAndThrowException(this ValidationResult validationResult)
    {
        if (validationResult == null)
        {
            throw new ArgumentNullException(nameof(validationResult));
        }

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }
    }
}
