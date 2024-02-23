using System;
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
    ///     The configuration for the destination system and is configured
    ///     for entities that should be synchronized.
    /// </summary>
    public Dictionary<string, SynchronizationOperations> Destination { set; get; } =
        new Dictionary<string, SynchronizationOperations>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     The configuration for the source system and is configured
    ///     for entities that should be synchronized.
    /// </summary>
    public Dictionary<string, SynchronizationOperations> Source { set; get; } =
        new Dictionary<string, SynchronizationOperations>(StringComparer.OrdinalIgnoreCase);


    /// <summary>
    ///     The priority of the current system used to determine the order of the synchronization plan.
    /// </summary>
    public int Priority { get; set; }
}
