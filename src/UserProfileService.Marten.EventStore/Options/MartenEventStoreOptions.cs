using UserProfileService.Adapter.Marten.Options;

namespace UserProfileService.Marten.EventStore.Options;

/// <summary>
///     Models the options that configure Marten EventStore and it's connection to PostrgreSql
/// </summary>
public class MartenEventStoreOptions : MartenConnectionOptions
{
    /// <summary>
    ///     Prefix used for the stream of the second level projection
    /// </summary>
    public string? StreamNamePrefix { get; set; } = string.Empty;

    /// <summary>
    ///     Used to identify the first level projection />.
    /// </summary>
    public string? SubscriptionName { get; set; }
}
