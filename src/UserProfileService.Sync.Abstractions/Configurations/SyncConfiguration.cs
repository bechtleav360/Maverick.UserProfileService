using UserProfileService.EventCollector.Abstractions.Messages;

namespace UserProfileService.Sync.Abstraction.Configurations;

/// <summary>
///     The systems holds the whole synchronization configuration for the source and the
///     destination system.
/// </summary>
public class SyncConfiguration
{
    /// <summary>
    ///     The expiration time (in minutes) of the lock used to avoid more sync processes to run at same time.
    /// </summary>
    public int LockExpirationTime { get; set; } = 15;

    /// <summary>
    ///     The delay (in minutes) of inaction before a timeout has to be raised during a step of a synchronization process.
    ///     Before setting this value you should take in consideration which value has been set for <see cref="StatusDispatch"/>.
    ///     The higher the <see cref="StatusDispatch"/> value, the higher the timeout value should be set.
    /// </summary>
    public int DelayBeforeTimeoutForStep { get; set; } = 3;

    /// <summary>
    ///     The source configuration that contains all needed configuration to synchronize data from
    ///     the source system.
    /// </summary>
    public SourceConfiguration SourceConfiguration { get; set; } = new SourceConfiguration();
}
