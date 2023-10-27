namespace UserProfileService.Proxy.Sync.Models;

/// <summary>
///     Store the current step of synchronization.
/// </summary>
public class StepView
{
    /// <summary>
    ///     Final Operation to be performed.
    /// </summary>
    public StepOperations Final { get; set; } = new StepOperations();

    /// <summary>
    ///     Handled operations for the current sync step incl. responses from worker.
    /// </summary>
    public StepOperationsHandled Handled { get; set; } = new StepOperationsHandled();

    /// <summary>
    ///     Status of the current step.
    /// </summary>
    public StepStatus? Status { get; set; }

    /// <summary>
    ///     Temporary overview of current step process.
    /// </summary>
    public StepOperationsTemporary Temporary { get; set; } = new StepOperationsTemporary();
}
