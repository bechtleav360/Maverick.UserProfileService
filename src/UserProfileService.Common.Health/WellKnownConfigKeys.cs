namespace UserProfileService.Common.Health;

/// <summary>
///     Contains constants related to configuration keys of health services.
/// </summary>
public class WellKnownConfigKeys
{
    /// <summary>
    ///     Contains the absolute key for scheduled health check delay setting.
    /// </summary>
    public const string HealthCheckDelay = "Delays:HealthCheck";

    /// <summary>
    ///     Contains the absolute key for health publisher delay setting.
    /// </summary>
    public const string HealthPushDelay = "Delays:HealthPush";
}
