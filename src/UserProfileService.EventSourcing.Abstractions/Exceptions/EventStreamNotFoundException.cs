namespace UserProfileService.EventSourcing.Abstractions.Exceptions;

/// <summary>
///     The exception that is thrown when a stream that is not existing is accessed
/// </summary>
public class EventStreamNotFoundException : EventStreamException
{
    /// <summary>
    ///     Creates a new instance of <see cref="EventStreamNotFoundException" />.
    /// </summary>
    /// <param name="streamName">The name of the stream that has not been found.</param>
    public EventStreamNotFoundException(string streamName) : base(
        streamName,
        $"The stream :{streamName} can not be found")
    {
    }

    /// <summary>
    ///     Creates a new instance of <see cref="EventStreamNotFoundException" />.
    /// </summary>
    /// <param name="message">A message explaining the error.</param>
    /// <param name="streamName">The name of the stream that has not been found.</param>
    public EventStreamNotFoundException(string streamName, string message) : base(streamName, message)
    {
    }

    /// <summary>
    ///     Creates a new instance of <see cref="EventStreamNotFoundException" />.
    /// </summary>
    /// <param name="streamName">The name of the stream that has not been found.</param>
    /// <param name="message">A message explaining the error.</param>
    /// <param name="innerException">The exception that caused the current exception.</param>
    public EventStreamNotFoundException(string streamName, string message, Exception innerException) : base(
        streamName,
        message,
        innerException)
    {
    }
}
