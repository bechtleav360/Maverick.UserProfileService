// Will be used as response in the API
// ReSharper disable UnusedMember.Global
namespace UserProfileService.Proxy.Sync.Models;

/// <summary>
///     Status of the sync process.
/// </summary>
public enum ProcessStatus
{
    /// <summary>
    ///     Sync process is not yet started.
    /// </summary>
    Initialize,

    /// <summary>
    ///     Sync process is in progress.
    /// </summary>
    InProgress,

    /// <summary>
    ///     Sync process is initializing next saga step.
    /// </summary>
    InitializeStep,

    /// <summary>
    ///     Sync process is done (without failure).
    /// </summary>
    Success,

    /// <summary>
    ///     Sync process is done with warnings / errors.
    /// </summary>
    SuccessWithHints,

    /// <summary>
    ///     Sync process failed.
    /// </summary>
    Failed,

    /// <summary>
    ///     Sync process aborted.
    /// </summary>
    Aborted
}
