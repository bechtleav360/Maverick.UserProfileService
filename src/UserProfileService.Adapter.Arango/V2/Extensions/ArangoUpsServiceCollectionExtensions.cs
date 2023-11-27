using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.Protocol.Extensions;
using Maverick.Client.ArangoDb.Public.Configuration;
using Maverick.Client.ArangoDb.Public.Extensions;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UserProfileService.Adapter.Arango.V2.Abstractions;
using UserProfileService.Adapter.Arango.V2.Configuration;
using UserProfileService.Adapter.Arango.V2.Contracts;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Adapter.Arango.V2.Helpers;
using UserProfileService.Adapter.Arango.V2.Implementations;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Contracts;
using UserProfileService.Common.V2.TicketStore.Abstractions;
using UserProfileService.Common.V2.Utilities;
using UserProfileService.EventCollector.Abstractions;

namespace UserProfileService.Adapter.Arango.V2.Extensions;

/// <summary>
///     Extension methods to easy register ArangoDB read and write services to a service collection.
/// </summary>
public static class ArangoUpsServiceCollectionExtensions
{
    private static ArangoConfiguration GetConfiguration(IServiceProvider serviceProvider)
    {
        ArangoConfiguration config = serviceProvider.GetRequiredService<IOptions<ArangoConfiguration>>().Value;

        config.ExceptionConfiguration.ExceptionHandler = exception =>
        {
            var globalHealthCheck =
                serviceProvider
                    .GetService<ArangoGlobalHealthCheck>();

            if (globalHealthCheck == null
                || !exception.IsAFatalError())
            {
                return Task.CompletedTask;
            }

            globalHealthCheck.Exception = exception;

            HealthStatus newHealthStatus =
                globalHealthCheck.Status switch
                {
                    HealthStatus.Healthy => HealthStatus.Degraded,
                    HealthStatus.Degraded =>
                        HealthStatus.Unhealthy,
                    _ => globalHealthCheck.Status
                };

            if (globalHealthCheck.Status == HealthStatus.Healthy
                || (globalHealthCheck.GetSecondsAfterLastUpdate()
                    > 200
                    && globalHealthCheck.Status != newHealthStatus))
            {
                globalHealthCheck.Status = newHealthStatus;
            }

            return Task.CompletedTask;
        };

        return config;
    }

    private static void ConfigureArangoConfiguration(
        IServiceCollection services,
        IConfigurationSection configurationSection,
        ILogger logger = null)
    {
        logger.EnterMethod();

        services.AddSingleton<ArangoGlobalHealthCheck>();
        services.Configure<ArangoConfiguration>(configurationSection);

        // custom validation object
        services.TryAddEnumerable(
            ServiceDescriptor
                .Singleton<IValidateOptions<ArangoConfiguration>, ArangoConfigurationValidation>());

        logger.ExitMethod();
    }

    /// <summary>
    ///     Adds all dependent services regarding user profile storage excluding <see cref="IReadService" /> and
    ///     <see cref="IProjectionWriteService" /> to the specified service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> where the services will be registered. </param>
    /// <param name="configurationSection">
    ///     The <inheritdoc cref="IConfigurationSection" /> where the configuration for the
    ///     event id store can be found.
    /// </param>
    /// <param name="logger">Logger instance that will be used to log incoming messages of the registration process.</param>
    /// <param name="arangoClientName">The name of the arangoClient that is used to create request to the database.</param>
    /// <param name="serializerSettings">The serializer settings that are used to modify the json converters.</param>
    /// <returns>The original service object.</returns>
    // TODO: It is somehow hacky and against our coding guidelines, but to refactor the hole
    // TODO: registration arango methods are just not enough time. Will be done, when there will be more time.
    // TODO: The class has to be refactored.
    public static void AddCommonDependenciesForArangoProfileRepositories(
        this IServiceCollection services,
        IConfigurationSection configurationSection,
        ILogger logger = null,
        string arangoClientName = null,
        JsonSerializerSettings serializerSettings = null)
    {
        logger.EnterMethod();

        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (configurationSection == null)
        {
            throw new ArgumentNullException(nameof(configurationSection));
        }


        logger.LogInfoMessage(
            "Starting to register all ArangoDB dependencies for profile storage.",
            LogHelpers.Arguments());

        ConfigureArangoConfiguration(services, configurationSection, logger);

        logger.LogInfoMessage("Registered ArangoDB configuration.", LogHelpers.Arguments());

        services
            .AddArangoClientFactory(p => p.GetRequiredService<ILogger<ArangoDbClient>>())
            .AddArangoClient(
                arangoClientName
                ?? ArangoConstants.DatabaseClientNameUserProfileStorage,
                GetConfiguration,
                defaultSerializerSettings: serializerSettings
                ?? new JsonSerializerSettings
                {
                    Converters = WellKnownJsonConverters
                        .GetDefaultProfileConverters()
                        .ToList(),
                    ContractResolver = new DefaultContractResolver()
                })
            .AddArangoClient(
                "HealthCheck",
                GetConfiguration);

        logger.LogInfoMessage("Registered client for ArangoDB rest calls.", LogHelpers.Arguments());

        services.TryAddSingleton<IDbInitializer, ArangoDbInitializer>();
        logger.LogInfoMessage("Registered ArangoDB initializer service.", LogHelpers.Arguments());

        logger.ExitMethod();
    }

    /// <summary>
    ///     Adds all dependent services regarding user profile storage including <see cref="IReadService" /> to the specified
    ///     service collection using the specified configuration section as ArangoNestedQueryEnumerable configuration.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> where the services will be registered. </param>
    /// <param name="configurationSection">
    ///     The <inheritdoc cref="IConfigurationSection" /> where the configuration for the
    ///     event id store can be found.
    /// </param>
    /// <param name="prefix">The prefix string of each collection name.</param>
    /// <param name="logger">Logger instance that will be used to log incoming messages of the registration process.</param>
    /// <returns>The original service object.</returns>
    public static IServiceCollection AddArangoRepositoriesToReadFromProfileStorage(
        this IServiceCollection services,
        IConfigurationSection configurationSection,
        string prefix,
        ILogger logger = null)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (configurationSection == null)
        {
            throw new ArgumentNullException(nameof(configurationSection));
        }

        if (string.IsNullOrWhiteSpace(prefix))
        {
            throw new ArgumentNullException(nameof(prefix));
        }

        // common dependencies that will be shared from read AND projection write service.
        services.AddCommonDependenciesForArangoProfileRepositories(configurationSection, logger);

        services.AddScoped<IAdminReadService, ArangoDbAdminReadService>(
            p => new ArangoDbAdminReadService(
                p.GetRequiredService<IDbInitializer>(),
                p.GetRequiredService<ILogger<ArangoDbAdminReadService>>(),
                p,
                ArangoConstants.DatabaseClientNameUserProfileStorage,
                new ArangoPrefixSettings()));

        services.TryAddScoped<IReadService>(
            p
                => new ArangoReadService(
                    p,
                    p.GetRequiredService<IDbInitializer>(),
                    p.GetRequiredService<ILogger<ArangoReadService>>(),
                    ArangoConstants.DatabaseClientNameUserProfileStorage,
                    prefix));

        logger.LogInfoMessage("Registered ArangoDB read service.", LogHelpers.Arguments());

        return services;
    }

    /// <summary>
    ///     Registers all arango dependencies for the User Profile Service API.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> where the services will be registered. </param>
    /// <param name="configurationSection">
    ///     The <inheritdoc cref="IConfigurationSection" /> where the configuration for the
    ///     arango database can be found.
    /// </param>
    /// <param name="logger">Logger instance that will be used to log incoming messages of the registration process.</param>
    /// <returns>The original service object.</returns>
    public static IServiceCollection AddArangoRepositoriesForService(
        this IServiceCollection services,
        IConfigurationSection configurationSection,
        ILogger logger = null)
    {
        services.AddArangoRepositoriesToReadFromProfileStorage(
            configurationSection,
            WellKnownDatabasePrefixes.ApiService,
            logger);

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<ICollectionDetailsProvider, UserProfileStoreCollectionsProvider>(
                _ => new UserProfileStoreCollectionsProvider(WellKnownDatabasePrefixes.ApiService)));

        return services;
    }

    /// <summary>
    ///     Registers the ticket store with all his dependencies.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> where the services will be registered. </param>
    /// <param name="configurationSection">
    ///     The <inheritdoc cref="IConfigurationSection" /> where the configuration for the
    ///     event id store can be found.
    /// </param>
    /// <param name="logger">The logger  will accept logging messages from this instance.</param>
    /// <param name="prefix">An optional parameter that is used as a collection prefix to define an unique collection name.</param>
    /// <returns>The <inheritdoc cref="IServiceCollection" /> where other services can be registered.</returns>
    public static IServiceCollection AddArangoTicketStore(
        this IServiceCollection services,
        IConfigurationSection configurationSection,
        string prefix,
        ILogger logger = null)
    {
        logger.EnterMethod();

        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (configurationSection == null)
        {
            throw new ArgumentNullException(nameof(configurationSection));
        }

        if (string.IsNullOrWhiteSpace(prefix))
        {
            throw new ArgumentNullException(nameof(prefix));
        }

        logger.LogInfoMessage("Starting to register all arango ticket store dependencies.", LogHelpers.Arguments());

        ConfigureArangoConfiguration(services, configurationSection, logger);

        logger.LogInfoMessage("Registered ArangoDB configuration.", LogHelpers.Arguments());

        services.AddArangoClientFactory()
            .AddArangoClient(
                ArangoConstants.DatabaseClientNameTicketStore,
                GetConfiguration,
                defaultSerializerSettings: new JsonSerializerSettings
                {
                    Converters = WellKnownJsonConverters
                        .GetDefaultTicketStoreConverters()
                        .ToList(),
                    ContractResolver = new DefaultContractResolver()
                });

        services.AddSingleton<ICollectionDetailsProvider>(new TicketStoreCollectionsProvider(prefix));

        logger.LogInfoMessage(
            "Registered {TicketStoreCollectionsProvider} collections provider.",
            LogHelpers.Arguments(nameof(TicketStoreCollectionsProvider)));

        services.TryAddSingleton<IDbInitializer>(
            p => new ArangoDbInitializer(
                p.GetRequiredService<IOptionsMonitor<ArangoConfiguration>>(),
                p,
                p.GetRequiredService<ILogger<ArangoDbInitializer>>(),
                ArangoConstants.DatabaseClientNameTicketStore,
                p.GetRequiredService<IEnumerable<ICollectionDetailsProvider>>()));

        services.TryAddSingleton<IJsonSerializerSettingsProvider, DefaultJsonSettingsProvider>();

        services.TryAddScoped<ITicketStore>(
            p => new ArangoTicketStore(
                p.GetRequiredService<ILogger<ArangoTicketStore>>(),
                p,
                p.GetRequiredService<IJsonSerializerSettingsProvider>().GetNewtonsoftSettings(),
                p.GetRequiredService<IDbInitializer>(),
                ArangoConstants.DatabaseClientNameTicketStore,
                prefix));

        logger.LogInfoMessage("Registered ticket store.", LogHelpers.Arguments());

        return logger.ExitMethod(services);
    }

    /// <summary>
    ///     Registers the event collector store with all his dependencies.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> where the services will be registered. </param>
    /// <param name="configurationSection">
    ///     The <inheritdoc cref="IConfigurationSection" /> where the configuration for the
    ///     event id store can be found.
    /// </param>
    /// <param name="logger">The logger  will accept logging messages from this instance.</param>
    /// <param name="prefix">An optional parameter that is used as a collection prefix to define an unique collection name.</param>
    /// <returns>The <inheritdoc cref="IServiceCollection" /> where other services can be registered.</returns>
    public static IServiceCollection AddEventCollectorStore(
        this IServiceCollection services,
        IConfigurationSection configurationSection,
        string prefix,
        ILogger logger = null)
    {
        logger.EnterMethod();

        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (configurationSection == null)
        {
            throw new ArgumentNullException(nameof(configurationSection));
        }

        if (string.IsNullOrWhiteSpace(prefix))
        {
            throw new ArgumentNullException(nameof(prefix));
        }

        logger.LogInfoMessage("Starting to register all arango ticket store dependencies.", LogHelpers.Arguments());

        ConfigureArangoConfiguration(services, configurationSection, logger);

        logger.LogInfoMessage("Registered ArangoDB configuration.", LogHelpers.Arguments());

        services.AddArangoClientFactory()
            .AddArangoClient(
                ArangoConstants.DatabaseClientNameEventCollector,
                GetConfiguration);

        services.AddSingleton<ICollectionDetailsProvider>(new EventCollectorCollectionsProvider(prefix));

        logger.LogInfoMessage(
            "Registered {EventCollectorCollectionsProvider} collections provider.",
            LogHelpers.Arguments(nameof(EventCollectorCollectionsProvider)));

        services.TryAddSingleton<IDbInitializer>(
            p => new ArangoDbInitializer(
                p.GetRequiredService<IOptionsMonitor<ArangoConfiguration>>(),
                p,
                p.GetRequiredService<ILogger<ArangoDbInitializer>>(),
                ArangoConstants.DatabaseClientNameEventCollector,
                p.GetRequiredService<IEnumerable<ICollectionDetailsProvider>>()));

        services.TryAddSingleton<IJsonSerializerSettingsProvider, DefaultJsonSettingsProvider>();

        services.TryAddScoped<IEventCollectorStore>(
            p =>
            {
                var store = new ArangoEventCollectorStore(
                    p.GetRequiredService<ILogger<ArangoEventCollectorStore>>(),
                    p,
                    p.GetRequiredService<IDbInitializer>(),
                    ArangoConstants.DatabaseClientNameEventCollector,
                    prefix);

                return store;
            });

        logger.LogInfoMessage("Registered event collector store.", LogHelpers.Arguments());

        return logger.ExitMethod(services);
    }

    /// <summary>
    /// Adds an ArangoDB implementation of <see cref="IDatabaseCleanupProvider"/> and all dependencies.
    /// </summary>
    /// <param name="serviceCollection">The service collection the dependencies should be added to.</param>
    /// <param name="setupCleanupTimeSpans">A setup function to retrieve cleanup configuration.</param>
    /// <param name="setupPrefixTerms">A setup function to retrieve prefix terms.</param>
    /// <param name="arangoDbClientName">The name of the ArangoDB client to be used.</param>
    public static void AddArangoDatabaseCleanupProvider(
        this IServiceCollection serviceCollection,
        Action<IServiceProvider, ArangoDbCleanupConfiguration> setupCleanupTimeSpans = null,
        Action<IServiceProvider, ArangoPrefixSettings> setupPrefixTerms = null,
        string arangoDbClientName = ArangoConstants.ArangoClientName)
    {
        serviceCollection.AddOptions<ArangoDbCleanupConfiguration>()
            .PostConfigure<IServiceProvider>(
                (o, p) =>
                {
                    setupCleanupTimeSpans?.Invoke(p, o);
                    var prefixSettings = new ArangoPrefixSettings();
                    setupPrefixTerms?.Invoke(p, prefixSettings);

                    o.ArangoDbClientName = arangoDbClientName;
                    o.AssignmentCollectionPrefix = prefixSettings.AssignmentsCollectionPrefix;
                    o.EventCollectorCollectionPrefix = prefixSettings.EventCollectorCollectionPrefix;
                    o.FirstLevelCollectionPrefix = prefixSettings.FirstLevelCollectionPrefix;
                    o.ServiceCollectionPrefix = prefixSettings.ServiceCollectionPrefix;
                });

        serviceCollection.AddArangoClientFactory()
            .AddArangoClient(
                arangoDbClientName,
                GetConfiguration);

        serviceCollection.AddScoped<IDatabaseCleanupProvider, ArangoDatabaseCleanupProvider>();
    }
}
