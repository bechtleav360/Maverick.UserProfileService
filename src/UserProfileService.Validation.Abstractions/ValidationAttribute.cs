using System.Collections.Generic;

namespace UserProfileService.Validation.Abstractions;

/// <summary>
///     Validation result for a specific property
/// </summary>
public class ValidationAttribute
{
    /// <summary>
    ///     Further information describing the error.
    ///     Example: Values of a list that are invalid.
    /// </summary>
    public IDictionary<string, object> AdditionalInformation { get; set; }

    /// <summary>
    ///     The message describing the error
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <summary>
    ///     Member the validation result is assigned to.
    /// </summary>
    public string Member { get; set; } = string.Empty;

    /// <summary>
    ///     Create an instance of <see cref="ValidationAttribute" />
    /// </summary>
    public ValidationAttribute()
    {
    }

    /// <summary>
    ///     Create an instance of <see cref="ValidationAttribute" />
    /// </summary>
    /// <param name="member">Member the validation result belongs to.</param>
    /// <param name="errorMessage">Message describing the validation error.</param>
    public ValidationAttribute(string member, string errorMessage)
    {
        Member = member;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    ///     Create an instance of <see cref="ValidationAttribute" />
    /// </summary>
    /// <param name="member">Member the validation result belongs to.</param>
    /// <param name="errorMessage">Message describing the validation error.</param>
    /// <param name="additionalInformation">Further information describing the error</param>
    public ValidationAttribute(
        string member,
        string errorMessage,
        IDictionary<string, object> additionalInformation)
    {
        Member = member;
        ErrorMessage = errorMessage;
        AdditionalInformation = additionalInformation;
    }
}
