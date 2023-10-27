namespace UserProfileService.Projection.Common.Utilities;

/// <summary>
///     Contains daemon lock Id used for the registered projection by marten event store.
/// </summary>
/// <remarks>
///     The daemon lock Id is used to establish a global lock id for the async daemon and should
///     be unique for any applications that target the same database.
/// </remarks>
public enum ProjectionDaemonLockId
{
    /// <summary>
    ///     Daemon lock Id used by the first level projection.
    ///     The number was chosen randomly to avoid in-house
    ///     conflicts with database.
    /// </summary>
    FirstLevelProjectionDaemonLockId = 2000,

    /// <summary>
    ///     Daemon lock Id used by the first level facade projection.
    /// </summary>
    FirstLevelFacadeDaemonLockId,

    /// <summary>
    ///     Daemon lock Id used by the second level Api projection.
    /// </summary>
    SecondLevelApiDaemonLockId,

    /// <summary>
    ///     Daemon lock Id used by the second level assignment projection.
    /// </summary>
    SecondLevelAssignmentsDaemonLockId,

    /// <summary>
    ///     Daemon lock Id used by the second level opa projection.
    /// </summary>
    SecondLevelOpaDaemonLockId,

    /// <summary>
    ///     Daemon lock Id used by the second level volatile data projection.
    /// </summary>
    SecondLevelVolatileDataProjectionLockId
}
