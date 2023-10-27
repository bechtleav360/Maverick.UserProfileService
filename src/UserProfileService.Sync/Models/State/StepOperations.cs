namespace UserProfileService.Sync.Models.State;

/// <summary>
///     Overview about operations
/// </summary>
public class StepOperations
{
    /// <summary>
    ///     Total number for create operations.
    /// </summary>
    public int Create { get; set; }

    /// <summary>
    ///     Total number for delete operations.
    /// </summary>
    public int Delete { get; set; }

    /// <summary>
    ///     Total operation of all operations.
    /// </summary>
    public int Total => Create + Update + Delete;

    /// <summary>
    ///     Total number for update operations.
    /// </summary>
    public int Update { get; set; }
}
