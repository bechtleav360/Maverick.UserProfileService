namespace UserProfileService.Sync.Models.State;

/// <summary>
///     Status of the current step state.
/// </summary>
public enum StepStatus
{
    /// <summary>
    ///     State is in progress.
    /// </summary>
    InProgress,

    /// <summary>
    ///     Process of step is fetching entities.
    /// </summary>
    Fetching,

    /// <summary>
    ///     Step is waiting for response for all sending requests.
    /// </summary>
    WaitingForResponse,

    /// <summary>
    ///     Step is successful processed.
    /// </summary>
    Success,

    /// <summary>
    ///     Step is successful processed.
    /// </summary>
    SuccessWithHints,

    /// <summary>
    ///     An fatal error occurred while processing step.
    ///     Current step can not be synchronized.
    /// </summary>
    Failure
}
