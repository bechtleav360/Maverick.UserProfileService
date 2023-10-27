namespace UserProfileService.Sync.Models.State;

/// <summary>
///     Overview about handled operations
/// </summary>
public class StepOperationsHandled
{
    /// <summary>
    ///     Total number for create operations.
    /// </summary>
    public StepOperationsCount Create { get; set; } = new StepOperationsCount();

    /// <summary>
    ///     Total number for delete operations.
    /// </summary>
    public StepOperationsCount Delete { get; set; } = new StepOperationsCount();

    /// <summary>
    ///     Total operation of all operations.
    /// </summary>
    public int Total => Create.Total + Update.Total + Delete.Total;

    /// <summary>
    ///     Total number for update operations.
    /// </summary>
    public StepOperationsCount Update { get; set; } = new StepOperationsCount();
}
