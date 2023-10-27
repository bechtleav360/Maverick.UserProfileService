namespace UserProfileService.Sync.Models.State;

/// <summary>
///     Status of one system during the sync process.
/// </summary>
public enum SystemStatus
{
    /// <summary>
    ///     System process is not yet started.
    /// </summary>
    Initialize,

    /// <summary>
    ///     System process is in progress.
    /// </summary>
    InProgress,

    /// <summary>
    ///     System process is done (without failure).
    /// </summary>
    Success,

    /// <summary>
    ///     System process is done with warnings / errors.
    /// </summary>
    SuccessWithHints,

    /// <summary>
    ///     System process failed.
    /// </summary>
    Failed
}
