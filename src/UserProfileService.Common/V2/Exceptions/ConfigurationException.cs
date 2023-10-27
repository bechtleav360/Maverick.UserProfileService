using System;

namespace UserProfileService.Common.V2.Exceptions;

/// <summary>
///     The exception that is thrown when a configuration error has occurred.
/// </summary>
public class ConfigurationException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ConfigurationException" /> class with a specified error message.
    /// </summary>
    /// <param name="message">Error message that explains the reason for the exception.</param>
    public ConfigurationException(string message) : base(message)
    {
    }

    /// <summary>
    ///     Returns the name of this exception and possibly the error message, the name of the inner exception, and the stack
    ///     trace.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        string s = GetType().Name + ": " + Message;

        if (InnerException != null)
        {
            s = s + " ---> " + InnerException;
        }

        if (StackTrace != null)
        {
            s += Environment.NewLine + StackTrace;
        }

        return s;
    }
}
