namespace UserProfileService.Common.V2.CommandLineTools.Cleanup;

/// <summary>
///     The type of the clean up target aka system that should be cleaned.
/// </summary>
public enum CleanupTargetType
{
    /// <summary>
    ///     Target is ArangoDb.
    /// </summary>
    Arango,

    /// <summary>
    ///     Target is EventStore.
    /// </summary>
    Eventstore
}
