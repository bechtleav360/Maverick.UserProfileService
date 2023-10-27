using System.Collections.Generic;
using UserProfileService.EventCollector.Abstractions.Messages;
using UserProfileService.Validation.Abstractions.Configuration;

namespace UserProfileService.Sync.Abstraction.Configurations;

/// <summary>
///     The configuration to synchronize objects from the
///     source system.
/// </summary>
public class SourceConfiguration
{
    /// <summary>
    ///     Defines when a status should be sent out for the already collected responses for a command. Default is 100.
    /// </summary>
    public StatusDispatch Dispatch { get; set; } = new StatusDispatch(100);

    /// <summary>
    ///     The a configuration per item that should be synchronized.
    ///     Also there be one or more implementation for a sync item.
    /// </summary>
    public Dictionary<string, SourceSystemConfiguration> Systems { get; set; } =
        new Dictionary<string, SourceSystemConfiguration>();

    /// <summary>
    ///     The configuration for the validation process.
    /// </summary>
    public ValidationConfiguration Validation { set; get; } = new ValidationConfiguration();
}
