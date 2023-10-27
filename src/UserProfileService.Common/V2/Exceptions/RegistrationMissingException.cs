using System;

namespace UserProfileService.Common.V2.Exceptions;

/// <summary>
///     An exception that will be thrown if a service was not registered in IServiceCollection during startup process.
/// </summary>
public class RegistrationMissingException : Exception
{
    /// <summary>
    ///     Initializes a new instance of <see cref="RegistrationMissingException" /> with a specified error message.
    /// </summary>
    /// <param name="message">The error message</param>
    public RegistrationMissingException(string message)
        : base(message)
    {
    }
}
