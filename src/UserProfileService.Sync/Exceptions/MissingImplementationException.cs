using System;

namespace UserProfileService.Sync.Exceptions;

/// <summary>
///     Exception thrown when no registered implementations are found for a specific type.
/// </summary>
public class MissingImplementationException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MissingImplementationException" /> class.
    /// </summary>
    public MissingImplementationException()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MissingImplementationException" /> class
    ///     with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public MissingImplementationException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MissingImplementationException" /> class
    ///     with a specified error message and a reference to the inner exception that is the cause
    ///     of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or <c>null</c> if none.</param>
    public MissingImplementationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
