using System;

namespace UserProfileService.Common.V2.Exceptions;

/// <summary>
///     Occurs when the state of a saga is not correct at it's current status.
/// </summary>
public class InvalidStateException : Exception
{
    /// <summary>
    ///     Initializes a new instance of <see cref="InvalidStateException" /> with a specified error message.
    /// </summary>
    /// <param name="message">The regarding message</param>
    public InvalidStateException(string message) : base(message)
    {
    }
}
