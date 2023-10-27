using System.Collections.Generic;
using UserProfileService.Sync.Abstraction.Configurations.Implementations;

namespace UserProfileService.Sync.Abstraction.Configurations;

/// <summary>
///     The System configuration that contains the necessary
///     configuration for all specific system.
/// </summary>
public class SourceSystemConfiguration
{
    /// <summary>
    ///     An general system configuration that holds all needed configuration.
    ///     The needed configuration items can be taken out by knowing the interfaces
    ///     for the configuration.
    /// </summary>
    public GeneralSystemConfiguration Configuration { get; set; }

    /// <summary>
    ///     The configuration for the destination system and is configured
    ///     for entities that should be synchronized.
    /// </summary>
    public Dictionary<string, SynchronizationOperations> Destination { set; get; } =
        new Dictionary<string, SynchronizationOperations>();

    /// <summary>
    ///     The configuration for the source system and is configured
    ///     for entities that should be synchronized.
    /// </summary>
    public Dictionary<string, SynchronizationOperations> Source { set; get; } =
        new Dictionary<string, SynchronizationOperations>();
}
