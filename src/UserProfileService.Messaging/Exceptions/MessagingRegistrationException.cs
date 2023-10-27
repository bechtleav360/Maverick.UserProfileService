namespace UserProfileService.Messaging.Exceptions;

/// <summary>
///     Thrown when there is an error while messaging is being setup.
/// </summary>
public class MessagingRegistrationException : ApplicationException
{
    /// <inheritdoc />
    public MessagingRegistrationException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
