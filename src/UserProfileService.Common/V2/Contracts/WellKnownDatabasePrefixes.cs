namespace UserProfileService.Common.V2.Contracts;

/// <summary>
///     Contains well-known prefixes for database related implementations.
/// </summary>
public class WellKnownDatabasePrefixes
{
    /// <summary>
    ///     Used in API/Service.
    /// </summary>
    public const string ApiService = "Service_";

    /// <summary>
    ///     Used in the assignments projection.
    /// </summary>
    public const string AssignmentProjection = "Assignments_";

    /// <summary>
    ///     Used for Projecting v1 models.
    /// </summary>
    public const string Bridge = "V1_";

    /// <summary>
    ///     Used in event collector.
    /// </summary>
    public const string EventCollector = "EventCollector_";

    /// <summary>
    ///     Used in the first level projection.
    /// </summary>
    public const string FirstLevelProjection = "FirstLevel_";

    /// <summary>
    ///     Used in projection worker.
    /// </summary>
    public const string ProjectionWorker = "ProjectionWorker_";

    /// <summary>
    ///     Used in saga worker.
    /// </summary>
    public const string SagaWorker = "SagaWorker_";

    /// <summary>
    ///     Used in sync worker.
    /// </summary>
    public const string Sync = "Sync_";
}
