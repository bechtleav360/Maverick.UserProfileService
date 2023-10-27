using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace UserProfileService.Sync.Models.State;

/// <summary>
///     Defines a process of synchronization.
/// </summary>
public class Process
{
    /// <summary>
    ///     Get current step.
    /// </summary>
    [JsonIgnore]
    public Step CurrentStep => System == null || string.IsNullOrWhiteSpace(Step) ? null : CurrentSystem.Steps[Step];

    /// <summary>
    ///     Get current system.
    /// </summary>
    [JsonIgnore]
    public System CurrentSystem => string.IsNullOrWhiteSpace(System) ? null : Systems[System];

    /// <summary>
    ///     Timestamp when synchronization finished.
    /// </summary>
    public DateTime? FinishedAt { get; set; }

    /// <summary>
    ///     Id of the current synchronization process.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     Timestamp when synchronization started.
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    ///     Status of process.
    /// </summary>
    public ProcessStatus Status { get; set; } = ProcessStatus.Initialize;

    /// <summary>
    ///     Defines the current step during synchronizing system.
    /// </summary>
    public string Step { get; set; }

    /// <summary>
    ///     Defines the current system.
    /// </summary>
    public string System { get; set; }

    /// <summary>
    ///     List of all information about system synchronizations.
    /// </summary>
    public IDictionary<string, System> Systems { get; set; } = new Dictionary<string, System>();

    /// <summary>
    ///     Timestamp when synchronization state changed.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
