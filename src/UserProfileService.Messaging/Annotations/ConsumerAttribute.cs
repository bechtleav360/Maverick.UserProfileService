namespace UserProfileService.Messaging.Annotations;

/// <summary>
///     Additional metadata for a MessageConsumer.
///     This will be used to configure the underlying messaging systems.
/// </summary>
public class ConsumerAttribute : Attribute
{
    /// <summary>
    ///     Overrides the generated name with a pre-defined one.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Indicates the specific version of this message.
    /// </summary>
    public string Version { get; set; } = string.Empty;
}
