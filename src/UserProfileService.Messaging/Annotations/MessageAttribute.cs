namespace UserProfileService.Messaging.Annotations;

/// <summary>
///     Additional metadata for a Message.
///     This will be used to configure the underlying messaging systems.
/// </summary>
public class MessageAttribute : Attribute
{
    /// <summary>
    ///     Overrides the generated name with a pre-defined one.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Send or receive this Message using this Service-Group.
    ///     This is useful when listening to messages from other applications or groups.
    ///     When set to null, the message will only be identified by its <see cref="ServiceName" />.
    /// </summary>
    public string? ServiceGroup { get; set; } = string.Empty;

    /// <summary>
    ///     Send or receive this Message using this Service-Name.
    ///     This is useful when listening to messages from other applications.
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    ///     Indicates the specific version of this message.
    /// </summary>
    public string Version { get; set; } = string.Empty;
}
