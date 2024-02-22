namespace UserProfileService.Common.V2.Contracts;

/// <summary>
///     Contains constants related to worker applications.
/// </summary>
public static class Constants
{
    /// <summary>
    ///     Specifies the name of the Correlation-ID header
    /// </summary>
    public const string HeaderNameCorrelationId = "x-Correlation-Id";

    /// <summary>
    /// Contains well-known constants related to messaging (i.e. endpoint names).
    /// </summary>
    public class Messaging
    {
        /// <summary>
        /// The name of the endpoint that take care of health check messages from/to WebAPI.
        /// </summary>
        public const string HealthCheckApiConsumerEndpoint = "maverick.user-profile.api-health-check-message.consumer";

        /// <summary>
        /// The name of the endpoint that take care of health check messages from/to Sync.
        /// </summary>
        public const string HealthCheckSyncConsumerEndpoint =
            "user-profile.sync-health-check-message.consumer";

        /// <summary>
        /// The name of the endpoint that take care of temporary health check messages.
        /// </summary>
        public const string HealthCheckTempEndpoint = "user-profile.api-health-check-message.temp";

        /// <summary>
        /// The name of the service group that is part of the exchange/queue names.
        /// </summary>
        public const string ServiceGroup = "user-profile";
    }

    /// <summary>
    ///     Defines the possible sources of the entities.
    /// </summary>
    public static class Source
    {
        /// <summary>
        ///     The entities are from the source api.
        /// </summary>
        public const string Api = "Api";
    }

    /// <summary>
    ///     Contains some constants related to event storage
    /// </summary>
    public static class EventStorage
    {
        /// <summary>
        ///     The section of Marten DB configuration
        /// </summary>
        public const string MartenSectionName = "Marten";
    }
}
