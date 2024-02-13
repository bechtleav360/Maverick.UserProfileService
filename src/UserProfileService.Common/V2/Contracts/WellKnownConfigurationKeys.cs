namespace UserProfileService.Common.V2.Contracts;

/// <summary>
///     Utility class containing well-known configuration key constants.
/// </summary>
public static class WellKnownConfigurationKeys
{
    /// <summary>
    ///     Contains the connections configuration section.
    /// </summary>
    public const string ConnectionsConfigKey = "Connections";
    /// <summary>
    ///     Contains the core provider configuration section.
    /// </summary>
    public const string CoreProvider = "Core";
    /// <summary>
    ///     Contains the event bus configuration section.
    /// </summary>
    public const string EventBus = "EventBusConnection";

    /// <summary>
    ///     Contains the configuration section of event notifier implementations.
    /// </summary>
    public const string EventNotifierConfiguration = "EventNotifier";

    /// <summary>
    ///     Contains the event store configuration section.
    /// </summary>
    public const string EventStore = "EventStoreConfiguration";

    /// <summary>
    ///     Contains the identity settings configuration section.
    /// </summary>
    public const string IdentitySettings = "IdentitySettings";

    /// <summary>
    ///     Contains the logging configuration section.
    /// </summary>
    public const string LoggingConfiguration = "LoggingConfiguration";

    /// <summary>
    ///     Contains the Marten settings configuration section.
    /// </summary>
    public const string MartenSettings = "Marten";
    /// <summary>
    ///     Contains the profile deputy configuration section.
    /// </summary>
    public const string ProfileDeputyConfiguration = "Deputy";
    /// <summary>
    ///     Contains the profile storage configuration section.
    /// </summary>
    public const string ProfileStorage = "ProfileStorage";
    /// <summary>
    ///     Contains the projection configuration section.
    /// </summary>
    public const string ProjectionConfiguration = "Projection";
    /// <summary>
    ///     Contains the Redis configuration section.
    /// </summary>
    public const string RedisSettings = "Redis";
    /// <summary>
    ///     Contains the ticket store configuration section.
    /// </summary>
    public const string TicketStore = "TicketStore";
}
