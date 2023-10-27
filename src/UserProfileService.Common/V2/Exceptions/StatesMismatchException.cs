using System;

namespace UserProfileService.Common.V2.Exceptions;

/// <summary>
///     Defines an error that will occur, when two states are not compatible and the operation could not been proceeded.
/// </summary>
public class StatesMismatchException : Exception
{
    /// <summary>
    ///     Initiates a new instance of <see cref="StatesMismatchException" /> with a specified error message.
    /// </summary>
    /// <param name="message">A descriptive text related to this exception.</param>
    public StatesMismatchException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initiates a new instance of <see cref="StatesMismatchException" /> without specifying any further property.
    /// </summary>
    public StatesMismatchException()
    {
    }
}
