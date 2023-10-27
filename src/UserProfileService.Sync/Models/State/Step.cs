using System;
using UserProfileService.Sync.Abstraction.Configurations;

namespace UserProfileService.Sync.Models.State;

/// <summary>
///     Store the current step and the next step
///     the saga has to forward.
/// </summary>
public class Step
{
    /// <summary>
    ///     Collection id of step to collecting response.
    /// </summary>
    public Guid? CollectingId { get; set; }

    /// <summary>
    ///     Final Operation to be performed.
    /// </summary>
    public StepOperations Final { get; set; } = new StepOperations();

    /// <summary>
    ///     Timestamp when synchronization for all entities finished.
    /// </summary>
    public DateTime? FinishedAt { get; set; }

    /// <summary>
    ///     Handled operations for the current sync step incl. responses from worker.
    /// </summary>
    public StepOperationsHandled Handled { get; set; } = new StepOperationsHandled();

    /// <summary>
    ///     Id of current step ("groups", "users", etc.).
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    ///     Id of the next step
    /// </summary>
    public string Next { get; set; }

    /// <summary>
    ///     Operations to perform to during process step.
    /// </summary>
    public SynchronizationOperation Operations { get; set; }

    /// <summary>
    ///     Timestamp when synchronization for all entities started.
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global => Get modifier is needed.
    public DateTime? StartedAt { get; set; }

    /// <summary>
    ///     Status of the current step.
    /// </summary>
    public StepStatus? Status { get; set; }

    /// <summary>
    ///     Temporary overview of current step process.
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global => Set modifier is needed.
    public StepOperationsTemporary Temporary { get; set; } = new StepOperationsTemporary();

    /// <summary>
    ///     Timestamp when synchronization updated by any action.
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global => Get modifier is needed.
    public DateTime? UpdatedAt { get; set; }
}
