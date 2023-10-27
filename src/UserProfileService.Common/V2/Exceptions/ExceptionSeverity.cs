namespace UserProfileService.Common.V2.Exceptions;

/// <summary>
///     Severity of an exception in order to classify it.
/// </summary>
public enum ExceptionSeverity
{
    /// <summary>
    ///     The exception is a hint and not necessarily to be handled.
    /// </summary>
    Hint = 0,

    /// <summary>
    ///     The exception is a warning and not necessarily to be handled.
    /// </summary>
    Warning = 1,

    /// <summary>
    ///     The exception is a normal error, such as a timeout issue.
    /// </summary>
    Error = 2,

    /// <summary>
    ///     The exception is a fatal error,
    ///     such as no connection can be established
    ///     because the service account has no access.
    /// </summary>
    Fatal = 3
}
