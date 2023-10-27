using System;
using System.Collections.Generic;
using System.Linq;

namespace UserProfileService.Validation.Abstractions;

/// <summary>
///     The exception that is thrown when an instance is not validated successfully.
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    ///     The validation results of the validation exception.
    /// </summary>
    public ICollection<ValidationAttribute> ValidationResults { get; set; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ValidationException" /> class with a list of validation results.
    /// </summary>
    /// <param name="validationResults">A list of validation results.</param>
    public ValidationException(
        params ValidationAttribute[] validationResults) : base("An error occurred while validating message.")
    {
        ValidationResults = validationResults;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ValidationException" /> class with a list of validation results.
    /// </summary>
    /// <param name="validationResults">A list of validation results.</param>
    public ValidationException(
        ICollection<ValidationAttribute> validationResults) : this(validationResults.ToArray())
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ValidationException" /> class with a list of validation results.
    /// </summary>
    /// <param name="message">Error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    /// <param name="validationResults">A list of validation results.</param>
    public ValidationException(
        string message,
        Exception innerException,
        ICollection<ValidationAttribute> validationResults) : base(message, innerException)
    {
        ValidationResults = validationResults;
    }
}
