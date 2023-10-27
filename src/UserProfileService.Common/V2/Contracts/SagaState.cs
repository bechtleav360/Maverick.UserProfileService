namespace UserProfileService.Common.V2.Contracts;

/// <summary>
///     The state that the saga is currently in.
/// </summary>
public enum SagaState
{
    /// <summary>
    ///     Saga has started an is doing tasks.
    /// </summary>
    Pending,

    /// <summary>
    ///     Saga has ended successfully.
    /// </summary>
    Success,

    /// <summary>
    ///     Saga has failed.
    /// </summary>
    Failed
}
