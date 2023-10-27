using System.Security;
using UserProfileService.EventSourcing.Abstractions.Models;

namespace UserProfileService.EventSourcing.Abstractions.Exceptions;

/// <summary>
///     Represents errors that will occur if an <see cref="StreamedEventHeader" /> is not valid.
/// </summary>
public class InvalidHeaderException : Exception
{
    /// <summary>
    ///     The regarding header that caused the error.
    /// </summary>
    public StreamedEventHeader Header { get; }

    /// <summary>
    ///     Initializes a new instance of <see cref="InvalidHeaderException" /> without specifying any message or further
    ///     data regarding the exception or it's cause.
    /// </summary>
    public InvalidHeaderException()
    {
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="InvalidHeaderException" /> with a specified error
    ///     <paramref name="message" />.
    /// </summary>
    /// <param name="message">A message containing information about the error or it's cause.</param>
    public InvalidHeaderException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="InvalidHeaderException" /> with a specified error
    ///     <see cref="message" /> and a reference to an inner exception that caused the error.
    /// </summary>
    /// <param name="message">A message containing information about the error or it's cause.</param>
    /// <param name="innerException">A reference to the exception that caused the error.</param>
    public InvalidHeaderException(
        string message,
        Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="InvalidHeaderException" /> with a specified <see cref="message" />
    ///     containing information about the error and the <paramref name="header" /> that caused the error.
    /// </summary>
    /// <param name="message">A message containing information about the error or it's cause.</param>
    /// <param name="header">The regarding header that caused the error.</param>
    public InvalidHeaderException(
        string message,
        StreamedEventHeader header)
        : base(message)
    {
        Header = header;
    }

    /// <inheritdoc />
    [SecurityCritical]
    public override string ToString()
    {
        if (Header == null)
        {
            return base.ToString();
        }

        string headerDetails = $"event stream id: {Header.EventStreamId}{Environment.NewLine}"
            + $"event number: {Header.EventNumberVersion}{Environment.NewLine}"
            + $"event type: {Header.EventType}";

        return
            $"{base.ToString()}{Environment.NewLine}Related header data: {headerDetails}";
    }
}
