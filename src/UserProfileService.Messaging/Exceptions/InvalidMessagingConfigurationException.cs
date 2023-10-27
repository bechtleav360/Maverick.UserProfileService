namespace UserProfileService.Messaging.Exceptions;

/// <summary>
///     Thrown when messaging-configuration is invalid.
/// </summary>
public class InvalidMessagingConfigurationException : ApplicationException
{
    /// <inheritdoc />
    public InvalidMessagingConfigurationException(string? message)
        : base(message)
    {
    }

    /// <inheritdoc />
    public InvalidMessagingConfigurationException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
