namespace UserProfileService.EventSourcing.Abstractions.Attributes;

/// <summary>
///     Attribute to define the version of an event.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class EventVersionAttribute : Attribute
{
    /// <summary>
    ///     Version information of the related event.
    /// </summary>
    public long VersionInformation { get; set; }

    /// <summary>
    ///     Create an instance of <see cref="EventVersionAttribute" />.
    /// </summary>
    /// <param name="versionInformation">Version of the event.</param>
    public EventVersionAttribute(long versionInformation)
    {
        VersionInformation = versionInformation;
    }
}
