using System;

namespace UserProfileService.Common.V2.Exceptions;

/// <summary>
///     Represents an error that occurs when a validation failed successfully.
/// </summary>
public class NotValidException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="NotValidException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public NotValidException(string message) : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="NotValidException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that caused the validation to fail, or <see langword="null"/> if it does not exist.</param>
    public NotValidException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
