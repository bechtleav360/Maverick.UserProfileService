namespace UserProfileService.Common.V2.Contracts;

public class WellKnownConfigurationKeys
{
    public const string ConnectionsConfigKey = "Connections";
    public const string CoreProvider = "Core";
    public const string EventBus = "EventBusConnection";
    public const string EventBusImplementation = "EventBusImplementation";

    /// <summary>
    ///     Contains the configuration section of event notifier implementations.
    /// </summary>
    public const string EventNotifierConfiguration = "EventNotifier";

    public const string EventStore = "EventStoreConfiguration";
    public const string IdentitySettings = "IdentitySettings";
    public const string LoggingConfiguration = "LoggingConfiguration";
    public const string MartenEventStore = "MartenEventStoreConfiguration";

    public const string MartenSettings = "Marten";
    public const string MessageBroker = "MessageBroker";
    public const string OpaConfigKey = "OPA";
    public const string ProfileDeputyConfiguration = "Deputy";
    public const string ProfileStorage = "ProfileStorage";
    public const string ProjectionConfiguration = "Projection";
    public const string RedisSettings = "Redis";
    public const string TicketStore = "TicketStore";
    public const string UseForwardedHeaders = "UseForwardedHeaders";
    public const string WebsocketConfigKey = "MaverickWebSocket";
}
