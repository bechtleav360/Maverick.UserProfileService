namespace UserProfileService.EventSourcing.Abstractions.Exceptions;

/// <summary>
///     Describes an exception that is thrown, when a deleted stream is accessed.
/// </summary>
public class AccessDeletedStreamException : Exception
{
    /// <summary>
    ///     Creates a new instance of <see cref="AccessDeletedStreamException" />.
    /// </summary>
    /// <param name="streamName">The name of the stream that was deleted.</param>
    public AccessDeletedStreamException(string streamName) : base(
        $"The stream {streamName} could not be accessed, because it is deleted/archived.")
    {
        // intentionally blank
    }
}
