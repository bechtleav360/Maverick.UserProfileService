namespace UserProfileService.Common.Health;

/// <summary>
///     Well known health check tags
/// </summary>
public static class HealthCheckTags
{
    /// <summary>
    ///     Tag for Liveness-Checks
    /// </summary>
    public const string Liveness = "Liveness";

    /// <summary>
    ///     Tag for Readiness-Checks
    /// </summary>
    public const string Readiness = "Readiness";

    /// <summary>
    ///     Tag for State-Checks
    /// </summary>
    public const string State = "State";

    /// <summary>
    ///     Tag for checks run by a scheduler
    /// </summary>
    public const string Scheduled = "scheduled";
}
