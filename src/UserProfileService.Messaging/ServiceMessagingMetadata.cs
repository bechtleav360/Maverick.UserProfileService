namespace UserProfileService.Messaging;

/// <summary>
///     Metadata for an application.
///     Used to configure messaging.
/// </summary>
public class ServiceMessagingMetadata
{
    /// <summary>
    ///     Group that the application belongs to.
    ///     A app-group consists of multiple apps working together for a single 'feature'.
    /// </summary>
    public string ServiceGroup { get; }

    /// <summary>
    ///     Technical name of the application.
    ///     This is used to differentiate multiple apps that might share names of internal components.
    ///     This should not reflect any namespace.
    /// </summary>
    public string ServiceName { get; }

    /// <summary>
    ///     Context of the message that is being sent out.
    ///     This should uniquely identify a single component or group.
    /// </summary>
    public Uri Source { get; }

    /// <summary>
    ///     Create a new instance of <see cref="ServiceMessagingMetadata" />
    /// </summary>
    /// <param name="serviceName">name of the app</param>
    /// <param name="serviceGroup">optional group of the app</param>
    /// <param name="source">source identifying this app</param>
    public ServiceMessagingMetadata(
        string serviceName,
        string serviceGroup,
        Uri source)
    {
        ServiceName = serviceName;
        ServiceGroup = serviceGroup;
        Source = source;
    }
}
