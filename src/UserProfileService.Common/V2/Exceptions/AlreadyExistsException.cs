using System;

namespace UserProfileService.Common.V2.Exceptions;

/// <summary>
///     Represents a validation error that occurs when attempting to create an entity that already exists.
/// </summary>
public class AlreadyExistsException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AlreadyExistsException"/> class.
    /// </summary>
    public AlreadyExistsException()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="AlreadyExistsException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public AlreadyExistsException(string message) : base(message)
    {
    }
}
