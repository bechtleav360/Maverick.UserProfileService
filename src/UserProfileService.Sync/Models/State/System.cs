using System;
using System.Collections.Generic;

namespace UserProfileService.Sync.Models.State;

/// <summary>
///     Defines a system step during synchronization process
/// </summary>
public class System
{
    /// <summary>
    ///     Timestamp when synchronization of system finished.
    /// </summary>
    public DateTime? FinishedAt { get; set; }

    /// <summary>
    ///     The name of the system to synchronize.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    ///     Indicates whether the system was synchronized.
    /// </summary>
    public bool IsCompleted => FinishedAt != null;

    /// <summary>
    ///     Id of the next system.
    /// </summary>
    public string Next { get; set; }

    /// <summary>
    ///     Timestamp when synchronization of system started.
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global => Get modifier is needed.
    public DateTime? StartedAt { get; set; }

    /// <summary>
    ///     Status of current system
    /// </summary>
    public SystemStatus? Status { get; set; }

    /// <summary>
    ///     Steps of system.
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global => Set modifier is needed.
    public IDictionary<string, Step> Steps { get; set; } = new Dictionary<string, Step>();

    /// <summary>
    ///     Timestamp when synchronization of system updated by action.
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global => Get modifier is needed.
    public DateTime? UpdatedAt { get; set; }
}
