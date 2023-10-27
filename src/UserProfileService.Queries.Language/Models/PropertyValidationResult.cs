namespace UserProfileService.Queries.Language.Models;

/// <summary>
///     Contains the return values for a validation. Returns whenever
///     the validation succeeded or failed.
/// </summary>
public enum PropertyValidationResult
{
    /// <summary>
    ///     The default value for the variable.The validation failed.
    /// </summary>
    None,

    /// <summary>
    ///     The property is not part of the result object.
    ///     The validation failed.
    /// </summary>
    PropertyNotInResultObject,

    /// <summary>
    ///     The property is part of the result object, but has not the right data type.
    ///     The validation failed.
    /// </summary>
    PropertyNotOfGivenType,

    /// <summary>
    ///     The property is part of the result und has the right data type. The
    ///     validation succeeded.
    /// </summary>
    PropertyValidationSuccess
}
