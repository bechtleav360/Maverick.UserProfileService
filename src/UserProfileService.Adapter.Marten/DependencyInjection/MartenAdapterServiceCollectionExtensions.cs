using Marten;
using Marten.Services.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserProfileService.Adapter.Marten.Abstractions;
using UserProfileService.Adapter.Marten.DbBuilders;
using UserProfileService.Adapter.Marten.Helpers;
using UserProfileService.Adapter.Marten.Implementations;
using UserProfileService.Adapter.Marten.Options;
using UserProfileService.Adapter.Marten.Validation;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Exceptions;

namespace UserProfileService.Adapter.Marten.DependencyInjection;

/// <summary>
///     Contains extension methods for dependency injection registration related to Marten.Adapter.
/// </summary>
public static class MartenAdapterServiceCollectionExtensions
{
    /// <summary>
    ///     Registers the database and all database schema to store and retrieved
    ///     user settings related data.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to which the services are added.</param>
    /// <param name="configurationSection">The configuration to register marten db.</param>
    /// <returns>
    ///     The service collection after it has been modified.
    /// </returns>
    public static IMartenVolatileDataStoreOptionsBuilder AddMartenVolatileUserSettingsStore(
        this IServiceCollection services,
        IConfigurationSection configurationSection)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (configurationSection == null)
        {
            throw new ArgumentNullException(nameof(configurationSection));
        }

        services.AddSingleton(configurationSection.Get<MartenVolatileStoreOptions>());
        services.AddSingleton<IValidateOptions<MartenVolatileStoreOptions>, MartenVolatileStoreOptionsValidation>();

        services.AddAutoMapper(typeof(UserSettingMapper));

        services.AddMartenStore<IVolatileDataStore>(
                provider =>
                {
                    var options = new StoreOptions();
                    var volatileStoreOptions = provider.GetRequiredService<MartenVolatileStoreOptions>();
                    var logger = provider.GetService<ILogger<MartenVolatileUserSettingsStore>>();

                    if (string.IsNullOrWhiteSpace(volatileStoreOptions.ConnectionString))
                    {
                        throw new ConfigurationException(
                            "Issue in Marten volatile store configuration: Connection string missing.");
                    }

                    options.Connection(volatileStoreOptions.ConnectionString);

                    if (!string.IsNullOrWhiteSpace(volatileStoreOptions.DatabaseSchema))
                    {
                        logger?.LogInfoMessage(
                            "Using database schema name '{databaseSchemaName}'.",
                            volatileStoreOptions.DatabaseSchema.AsArgumentList());

                        options.DatabaseSchemaName = volatileStoreOptions.DatabaseSchema;
                    }

                    options.UseDefaultSerialization(serializerType: SerializerType.SystemTextJson);

                    options.Schema.Include<UserSettingsRegistry>();

                    return options;
                })
            .ApplyAllDatabaseChangesOnStartup();

        services.AddScoped<IVolatileUserSettingsService, MartenVolatileUserSettingsService>();
        services.AddScoped<IVolatileUserSettingsStore, MartenVolatileUserSettingsStore>();
        services.AddScoped<IVolatileDataReadStore, MartenVolatileUserSettingsStore>();
        services.AddScoped<IOptionsValidateParser, OptionsValidateParser>();
        services.AddScoped<IQueryConverter, QueryConverter>();

        return new MartenVolatileDataStoreOptionsBuilder(services);
    }

    /// <summary>
    ///     Registers the Marten implementation of <see cref="IUserStore" />.
    /// </summary>
    /// <param name="builder">The builder to be used.</param>
    /// <param name="logger">A logger to accept message of this instance.</param>
    /// <exception cref="ArgumentNullException"><paramref name="builder" /> is <c>null</c>.</exception>
    public static void AddMartenUserStore(
        this IMartenVolatileDataStoreOptionsBuilder builder,
        ILogger? logger = null)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.Services.AddScoped<IUserStore, MartenVolatileUserSettingsStore>();

        logger.LogInfoMessage("Registered Marten user store.", LogHelpers.Arguments());
    }
}
