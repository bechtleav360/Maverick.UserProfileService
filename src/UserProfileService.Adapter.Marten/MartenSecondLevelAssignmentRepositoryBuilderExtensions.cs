using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UserProfileService.Adapter.Marten.Implementations;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Projection.Abstractions;

namespace UserProfileService.Adapter.Marten;

/// <summary>
///     Registers all needed dependencies for the second level projection to use the arango database.
/// </summary>
public static class MartenSecondLevelAssignmentRepositoryBuilderExtensions
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
    /// <param name="logger">Logger instance that will be used to log incoming messages of the registration process.</param>
    /// <returns>The original service object.</returns>
    public static ISecondLevelVolatileDataProjectionBuilder AddMartenVolatileDataProjectionRepository(
        this ISecondLevelVolatileDataProjectionBuilder builder,
        IConfigurationSection configurationSection,
        ILogger? logger = null)
    {
        logger.EnterMethod();

        IServiceCollection services = builder.ServiceCollection;

        if (services == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (configurationSection == null)
        {
            throw new ArgumentNullException(nameof(configurationSection));
        }

        services.AddScoped<ISecondLevelVolatileDataRepository, MartenVolatileDataProjectionRepository>();

        // common dependencies that will be shared from read AND projection write service. - Seems save
        logger.LogInfoMessage("Register all needed Marten projection dependencies.", LogHelpers.Arguments());

        return logger.ExitMethod(builder);
    }
}
