using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using UserProfileService.Adapter.Arango.V2.Abstractions;
using UserProfileService.Adapter.Arango.V2.Contracts;
using UserProfileService.Adapter.Arango.V2.Helpers;
using UserProfileService.Adapter.Arango.V2.Implementations;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Contracts;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.Common.DependencyInjection;

namespace UserProfileService.Adapter.Arango.V2.Extensions;

/// <summary>
///     Registers all needed dependencies for the first level projection to use the arango database.
/// </summary>
public static class ArangoFirstLevelRepositoryBuilderExtensions
{
    /// <summary>
    ///     Adds all dependent services regarding the <see cref="IFirstLevelProjectionRepository" /> to the
    ///     <br />
    ///     specified service collection using the specified configuration section as ArangoNestedQueryEnumerable
    ///     configuration.<br />
    ///     It will try to add a default json settings provider as well, if not already registered.
    /// </summary>
    /// <param name="builder">The <see cref="IServiceCollection" /> where the services will be registered. </param>
    /// <param name="configurationSection">
    ///     The <inheritdoc cref="IConfigurationSection" /> where the configuration for the
    ///     event id store can be found.
    /// </param>
    /// <param name="prefix">The prefix string of each collection name.</param>
    /// <param name="logger">Logger instance that will be used to log incoming messages of the registration process.</param>
    /// <returns>The original service object.</returns>
    public static IFirstLevelProjectionBuilder AddArangoFirstLevelRepository(
        this IFirstLevelProjectionBuilder builder,
        IConfigurationSection configurationSection,
        string prefix = WellKnownDatabasePrefixes.FirstLevelProjection,
        ILogger logger = null)
    {
        logger.EnterMethod();

        IServiceCollection services = builder?.ServiceCollection;

        if (services == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (configurationSection == null)
        {
            throw new ArgumentNullException(nameof(configurationSection));
        }

        if (string.IsNullOrWhiteSpace(prefix))
        {
            throw new ArgumentNullException(nameof(prefix));
        }

        // common dependencies that will be shared from read AND projection write service. - Seems save
        logger.LogInfoMessage("Register all needed arango dependencies.", LogHelpers.Arguments());

        services.AddCommonDependenciesForArangoProfileRepositories(
            configurationSection,
            arangoClientName: ArangoConstants.ArangoFirstLevelProjectionName,
            serializerSettings: new JsonSerializerSettings
            {
                Converters = WellKnownJsonConverters
                    .GetDefaultFirstLevelProjectionConverters()
                    .ToList(),
                ContractResolver = new DefaultContractResolver()
            },
            logger: logger);

        logger.LogInfoMessage("Register the firstLevelProjectionCollectionsProvider.", LogHelpers.Arguments());
        services.AddSingleton<ICollectionDetailsProvider>(_ => new FirstLevelProjectionCollectionsProvider(prefix));

        logger.LogInfoMessage("Register the ArangoFirstLevelProjectionRepository.", LogHelpers.Arguments());

        services.AddTransient<IFirstLevelProjectionRepository>(
            sp => new ArangoFirstLevelProjectionRepository(
                ArangoConstants.ArangoFirstLevelProjectionName,
                prefix,
                sp.GetRequiredService<ILogger<ArangoFirstLevelProjectionRepository>>(),
                sp));

        return logger.ExitMethod(builder);
    }

    /// <summary>
    ///     Adds all dependent services regarding the <see cref="IFirstLevelProjectionRepository" /> to the
    ///     <br />
    ///     specified service collection using the specified configuration section as ArangoNestedQueryEnumerable
    ///     configuration.<br />
    ///     It will try to add a default json settings provider as well, if not already registered.
    /// </summary>
    /// <param name="builder">The <see cref="IServiceCollection" /> where the services will be registered. </param>
    /// <param name="configurationSection">
    ///     The <inheritdoc cref="IConfigurationSection" /> where the configuration for the
    ///     event id store can be found.
    /// </param>
    /// <param name="prefix">The prefix string of each collection name.</param>
    /// <param name="arangoClientName">The name of the arango client that is used to communicate with the arango database.</param>
    /// <param name="logger">Logger instance that will be used to log incoming messages of the registration process.</param>
    /// <returns>The original service object.</returns>
    public static ISagaServiceOptionsBuilder AddArangoEventLogWriter(
        this ISagaServiceOptionsBuilder builder,
        IConfigurationSection configurationSection,
        string prefix = WellKnownDatabasePrefixes.FirstLevelProjection,
        string arangoClientName = ArangoConstants.ArangoFirstLevelLogStore,
        ILogger logger = null)
    {
        logger.EnterMethod();

        IServiceCollection services = builder?.Services;

        if (services == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (configurationSection == null)
        {
            throw new ArgumentNullException(nameof(configurationSection));
        }

        if (string.IsNullOrWhiteSpace(prefix))
        {
            throw new ArgumentNullException(nameof(prefix));
        }

        // common dependencies that will be shared from read AND projection write service. - Seems save
        logger.LogInfoMessage("Register all needed arango dependencies.", LogHelpers.Arguments());

        var serializerSettings = new JsonSerializerSettings
        {
            Converters = new List<JsonConverter>
            {
                new StringEnumConverter(),
                WellKnownSecondLevelConverter.GetSecondLevelDefaultConverters()
            },
            ContractResolver = new DefaultContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        };

        services.AddCommonDependenciesForArangoProfileRepositories(
            configurationSection,
            arangoClientName: arangoClientName,
            serializerSettings: serializerSettings,
            logger: logger);

        logger.LogInfoMessage("Register the firstLevelProjectionCollectionsProvider.", LogHelpers.Arguments());

        services.AddSingleton<ICollectionDetailsProvider>(
            _ => new FirstLevelProjectionEventLogCollectionsProvider(prefix));

        logger.LogInfoMessage("Register the ArangoFirstLevelProjectionRepository.", LogHelpers.Arguments());

        services.AddTransient<IFirstProjectionEventLogWriter>(
            sp => new ArangoEventLogStore(
                sp.GetRequiredService<ILogger<ArangoEventLogStore>>(),
                sp,
                arangoClientName,
                prefix));

        return logger.ExitMethod(builder);
    }
}
