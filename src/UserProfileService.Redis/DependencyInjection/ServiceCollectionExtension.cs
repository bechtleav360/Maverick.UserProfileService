using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Redis.Configuration;
using UserProfileService.Redis.Validation;

namespace UserProfileService.Redis.DependencyInjection;

/// <summary>
///     Extension to register the redis connection with the configuration.
/// </summary>
public static class ServiceCollectionExtension
{
    /// <summary>
    ///     Tries to add redis multiplexer as singleton service. If redis is not configured in
    ///     <paramref name="configuration" /> section, the method will return <c>false</c>. Exceptions will be still thrown.
    /// </summary>
    /// <param name="services">The services collection to register the client.</param>
    /// <param name="configuration">Configuration section representing <see cref="RedisConfiguration" />.</param>
    /// <param name="logger">An optional logger.</param>
    /// <returns>The service collection <see cref="IServiceCollection" />.</returns>
    /// <paramref name="services"> is <c>null</c>.</paramref>
    /// <paramref name="configuration"> is <c>null</c>.</paramref>
    /// <exception cref="ArgumentNullException">The ArgumentException is thrown when an argument is null when it shouldn't be.</exception>
    public static bool TryAddRedis(
        this IServiceCollection services,
        IConfigurationSection configuration,
        ILogger logger = null)
    {
        logger?.EnterMethod();

        logger?.LogInfoMessage("Try to register redis cache.", LogHelpers.Arguments());

        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        if (!configuration.Exists())
        {
            logger?.LogInfoMessage("Redis was not configured. Skipping registration.", LogHelpers.Arguments());

            return false;
        }

        services.AddRedis(configuration, logger);

        logger?.ExitMethod();

        return true;
    }

    /// <summary>
    ///     Add redis multiplexer as singleton service.
    /// </summary>
    /// <param name="services">The services collection to register the client.</param>
    /// <param name="configuration">Configuration section representing <see cref="RedisConfiguration" />.</param>
    /// <param name="logger">An optional logger.</param>
    /// <returns>The service collection <see cref="IServiceCollection" />.</returns>
    /// <paramref name="services"> is <c>null</c>.</paramref>
    /// <paramref name="configuration"> is <c>null</c>.</paramref>
    /// <exception cref="ArgumentNullException">The ArgumentException is thrown when an argument is null when it shouldn't be.</exception>
    public static IServiceCollection AddRedis(
        this IServiceCollection services,
        IConfigurationSection configuration,
        ILogger logger = null)
    {
        logger?.EnterMethod();

        logger?.LogInfoMessage("Try to register redis cache.", LogHelpers.Arguments());

        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        var redisConfig = configuration.Get<RedisConfiguration>();

        services.AddSingleton<IValidateOptions<RedisConfiguration>, RedisConfigurationValidation>();
        services.Configure<RedisConfiguration>(configuration);

        var configOptions = new ConfigurationOptions
        {
            AbortOnConnectFail = redisConfig.AbortOnConnectFail,
            AllowAdmin = redisConfig.AllowAdmin,
            ChannelPrefix = redisConfig.ChannelPrefix,
            CheckCertificateRevocation = redisConfig.CheckCertificateRevocation,
            ConnectRetry = redisConfig.ConnectRetry,
            ConnectTimeout = redisConfig.ConnectTimeout,
            ConfigurationChannel = redisConfig.ConfigurationChannel,
            ConfigCheckSeconds = redisConfig.ConfigCheckSeconds,
            DefaultDatabase = redisConfig.DefaultDatabase,
            KeepAlive = redisConfig.KeepAlive,
            ClientName = redisConfig.ClientName,
            Password = redisConfig.Password,
            User = redisConfig.User,
            Proxy = redisConfig.Proxy,
            ResolveDns = redisConfig.ResolveDns,
            ServiceName = redisConfig.ServiceName,
            Ssl = redisConfig.Ssl,
            SslHost = redisConfig.SslHost,
            SslProtocols = redisConfig.SslProtocols,
            SyncTimeout = redisConfig.SyncTimeout,
            AsyncTimeout = redisConfig.AsyncTimeout,
            TieBreaker = redisConfig.TieBreaker,
            SetClientLibrary = redisConfig.SetClientLibrary,
            Protocol = redisConfig.Protocol
        };

        foreach (string endpoint in redisConfig.EndpointUrls
                     .Where(e => !string.IsNullOrWhiteSpace(e)))
        {
            configOptions.EndPoints.Add(endpoint);
        }

        logger?.LogInfoMessage("Try to connect multiplexer to redis server.", LogHelpers.Arguments());

        ConnectionMultiplexer connectionMultiplexer = ConnectionMultiplexer.Connect(configOptions);

        logger?.LogInfoMessage(
            "Connection multiplexer for redis was created with client name {connectionMultiplexer.ClientName} and will be registered as singleton.",
            LogHelpers.Arguments(connectionMultiplexer.ClientName));

        services.AddSingleton<IConnectionMultiplexer>(connectionMultiplexer);

        logger?.ExitMethod();

        return services;
    }
}
