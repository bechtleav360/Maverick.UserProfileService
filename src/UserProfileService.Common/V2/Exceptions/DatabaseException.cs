using System;

namespace UserProfileService.Common.V2.Exceptions;

/// <summary>
///     The exception that is thrown when a database exception occurred.
/// </summary>
public class DatabaseException : Exception
{
    /// <summary>
    ///     Severity of the exception.
    /// </summary>
    public ExceptionSeverity Severity { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DatabaseException" /> class with a specified severity.
    /// </summary>
    /// <param name="severity">Severity of the error.</param>
    public DatabaseException(ExceptionSeverity severity) : base("An database exception occurred.")
    {
        Severity = severity;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DatabaseException" /> class with a specified error message and
    ///     severity.
    /// </summary>
    /// <param name="message">Error message that explains the reason for the exception.</param>
    /// <param name="severity">Severity of the error.</param>
    public DatabaseException(string message, ExceptionSeverity severity) : base(message)
    {
        Severity = severity;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DatabaseException" /> class with a specified error message and
    ///     severity.
    /// </summary>
    /// <param name="message">Error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    /// <param name="severity">Severity of the error.</param>
    public DatabaseException(string message, Exception innerException, ExceptionSeverity severity) : base(
        message,
        innerException)
    {
        Severity = severity;
    }
}
