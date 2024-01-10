using System;

namespace UserProfileService.Sync.Abstraction.Annotations;

/// <summary>
///     Specifies the source name of the sync configuration provider.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class SynchConfigurationProviderAttribute : Attribute
{
    /// <summary>
    ///     The name of the system configuration that should be synced from.
    /// </summary>
    public string SyncConfigName { get; set; }

    /// <summary>
    ///     Initializes a new instance of <see cref="SynchConfigurationProviderAttribute" />.
    /// </summary>
    /// <param name="syncConfigName">The name of the source the related sync configuration provider is using.</param>
    public SynchConfigurationProviderAttribute(string syncConfigName)
    {
        SyncConfigName = syncConfigName;
    }
}
