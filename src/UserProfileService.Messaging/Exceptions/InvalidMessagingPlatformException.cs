namespace UserProfileService.Messaging.Exceptions;

/// <summary>
///     Thrown when it's attempted to configure an unknown messaging platform.
/// </summary>
public class InvalidMessagingPlatformException : ApplicationException
{
    /// <inheritdoc />
    public InvalidMessagingPlatformException(string brokerType)
        : base($"messaging type '{brokerType}' is invalid/unknown")
    {
    }
}
