using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UserProfileService.Adapter.Arango.V2.Abstractions;
using UserProfileService.Adapter.Arango.V2.Contracts;
using UserProfileService.Adapter.Arango.V2.Helpers;
using UserProfileService.Adapter.Arango.V2.Implementations;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Contracts;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Common.Abstractions;

namespace UserProfileService.Adapter.Arango.V2.Extensions;

/// <summary>
///     Registers all needed dependencies for the second level projection to use the arango database.
/// </summary>
public static class ArangoSecondLevelAssignmentRepositoryBuilderExtensions
{
    /// <summary>
    ///     Adds all dependent services regarding the <see cref="ISecondLevelProjectionRepository" /> to the
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
    /// <param name="queryPrefix">The prefix of the query collections.</param>
    /// <param name="logger">Logger instance that will be used to log incoming messages of the registration process.</param>
    /// <returns>The original service object.</returns>
    public static ISecondLevelAssignmentProjectionBuilder AddArangoAssignmentRepository(
        this ISecondLevelAssignmentProjectionBuilder builder,
        IConfigurationSection configurationSection,
        string prefix = WellKnownDatabasePrefixes.AssignmentProjection,
        string queryPrefix = WellKnownDatabasePrefixes.ApiService,
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

        services.AddAutoMapper(typeof(MappingProfiles).Assembly);

        services.AddScoped<ISecondLevelAssignmentRepository>(
            sp => new ArangoSecondLevelAssignmentRepository(
                ArangoConstants.SecondLevelAssignmentsArangoClientName,
                prefix,
                queryPrefix,
                sp.GetRequiredService<ILogger<ArangoSecondLevelAssignmentRepository>>(),
                sp));

        // common dependencies that will be shared from read AND projection write service. - Seems save
        logger.LogInfoMessage("Register all needed arango dependencies.", LogHelpers.Arguments());

        services.AddCommonDependenciesForArangoProfileRepositories(
            configurationSection,
            arangoClientName: ArangoConstants.SecondLevelAssignmentsArangoClientName,
            serializerSettings: new JsonSerializerSettings
            {
                Converters = WellKnownJsonConverters
                    .GetDefaultSecondLevelAssignmentProjectionConverters()
                    .ToList(),
                ContractResolver = new DefaultContractResolver()
            },
            logger: logger);

        logger.LogInfoMessage("Register the secondLevelProjectionCollectionsProvider.", LogHelpers.Arguments());

        services.AddSingleton<ICollectionDetailsProvider>(
            _ => new SecondLevelAssignmentsProjectionCollectionsProvider(prefix, queryPrefix));

        return logger.ExitMethod(builder);
    }
}
