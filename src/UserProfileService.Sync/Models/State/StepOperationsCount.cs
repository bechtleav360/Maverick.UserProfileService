namespace UserProfileService.Sync.Models.State;

/// <summary>
///     Count of step operations.
/// </summary>
public class StepOperationsCount
{
    /// <summary>
    ///     Total number for failure operations.
    /// </summary>
    public int Failure { get; set; }

    /// <summary>
    ///     Total number for success operations.
    /// </summary>
    public int Success { get; set; }

    /// <summary>
    ///     Total number of operations.
    /// </summary>
    public int Total => Success + Failure;
}
