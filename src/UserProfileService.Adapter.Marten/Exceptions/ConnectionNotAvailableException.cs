namespace UserProfileService.Adapter.Marten.Exceptions;

/// <summary>
///     The exception that is thrown when a connection is null or could not be established.
/// </summary>
public class ConnectionNotAvailableException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ConnectionNotAvailableException" /> class with a specified error
    ///     message and
    ///     a reference to the inner exception that is the cause of this exception.
    /// </summary>
    public ConnectionNotAvailableException()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConnectionNotAvailableException" /> class with a specified error
    ///     message and
    ///     a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message string.</param>
    public ConnectionNotAvailableException(string message) : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConnectionNotAvailableException" /> class with a specified error
    ///     message and
    ///     a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message string.</param>
    /// <param name="inner">The inner exception reference.</param>
    public ConnectionNotAvailableException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
