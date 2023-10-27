using UserProfileService.Sync.Models.State;

namespace UserProfileService.Sync.Models.Views;

/// <summary>
///     Store the current step of synchronization.
///     This a view of <see cref="Step" />
/// </summary>
public class StepView
{
    /// <summary>
    ///     Final Operation to be performed.
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global => Get modifier is needed.
    public StepOperations Final { get; set; } = new StepOperations();

    /// <summary>
    ///     Handled operations for the current sync step incl. responses from worker.
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global => Get modifier is needed.
    public StepOperationsHandled Handled { get; set; } = new StepOperationsHandled();

    /// <summary>
    ///     Status of the current step.
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global => Get modifier is needed.
    public StepStatus? Status { get; set; }

    /// <summary>
    ///     Temporary overview of current step process.
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global => Get modifier is needed.
    public StepOperationsTemporary Temporary { get; set; } = new StepOperationsTemporary();
}
