namespace UserProfileService.EventSourcing.Abstractions.Exceptions;

/// <summary>
///     Represents an error related to an event stream.
/// </summary>
public abstract class EventStreamException : Exception
{
    /// <summary>
    ///     The name of the stream.
    /// </summary>
    public string StreamName { get; }

    /// <summary>
    ///     Creates a new instance of <see cref="EventStreamException" />.
    /// </summary>
    /// <param name="streamName">The name of the stream that caused the exception.</param>
    /// <param name="message">A message explaining the error.</param>
    protected EventStreamException(string streamName, string message) : base(message)
    {
        StreamName = streamName;
    }

    /// <summary>
    ///     Creates a new instance of <see cref="EventStreamException" />.
    /// </summary>
    /// <param name="streamName">The name of the stream that caused the exception.</param>
    /// <param name="message">A message explaining the error.</param>
    /// <param name="innerException">The exception that caused the current exception.</param>
    protected EventStreamException(string streamName, string message, Exception innerException) : base(
        message,
        innerException)
    {
        StreamName = streamName;
    }

    /// <summary>
    ///     Creates an instance of <see cref="EventStreamException" />
    /// </summary>
    /// <param name="streamName"></param>
    public EventStreamException(string streamName)
    {
        StreamName = streamName;
    }
}
