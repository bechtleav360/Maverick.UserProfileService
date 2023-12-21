using System;
using System.Data;
using System.Net.Http;
using System.Threading;
using Maverick.Client.ArangoDb.Public.Configuration;
using Maverick.Client.ArangoDb.Public.Handlers;
using Maverick.Client.ArangoDb.Public.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Maverick.Client.ArangoDb.Public.Extensions;

/// <summary>
///     Registers the arango client with the http client factory.
///     The registered client is a named client and the name is a constant.
///     This extension is used because of polly problems that occurs
///     with the arango extension package (7.1) and the configuration
///     rest client (6.0). So the extension was moved in this project.
/// </summary>
public static class ArangoClientServiceCollectionExtension
{
    private static ArangoExceptionOptions MapToExceptionOptions(
        IServiceProvider serviceProvider,
        Func<IServiceProvider, ArangoConfiguration> connectionFactory)
    {
        ArangoExceptionConfiguration config = connectionFactory.Invoke(serviceProvider)?.ExceptionConfiguration
            ?? new ArangoExceptionConfiguration();

        return new ArangoExceptionOptions
        {
            DurationOfBreak = config.DurationOfBreak,
            ExceptionHandler = config.ExceptionHandler,
            HandledEventsAllowedBeforeBreaking = config.HandledEventsAllowedBeforeBreaking,
            RetryCount = config.RetryCount,
            RetryEnabled = config.RetryEnabled,
            SleepDuration = config.SleepDuration
        };
    }

    /// <summary>
    ///     Registers an arango client.
    /// </summary>
    /// <param name="services">The services collection to register the client.</param>
    /// <param name="connectionFactory">The connection string for the client.</param>
    /// <param name="logger">An optional logger.</param>
    /// <param name="lifetime">The lifetime of the arango client.</param>
    /// <param name="defaultSerializerSettings">Default serializer settings as optional parameter.</param>
    /// <param name="clientName">
    ///     The name of <see cref="IArangoDbClient" /> and the <see cref="HttpClient" /> in the
    ///     <see cref="IHttpClientFactory" />.
    /// </param>
    /// <returns>The service collection <see cref="IServiceCollection" />.</returns>
    /// <exception cref="DuplicateNameException">An arango client with the same name is already registered.</exception>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="services" /> is <c>null</c>.<br />
    ///     <paramref name="connectionFactory" /> is <c>null</c>.
    /// </exception>
    public static IServiceCollection AddArangoClient(
        this IServiceCollection services,
        Func<IServiceProvider, ArangoConfiguration> connectionFactory,
        ILogger logger,
        ServiceLifetime lifetime,
        JsonSerializerSettings defaultSerializerSettings,
        string clientName)
    {
        if (services == null)
        {
            throw new ArgumentNullException($"The variable {nameof(services)} was null!");
        }

        if (connectionFactory == null)
        {
            throw new ArgumentNullException($"The parameter {nameof(connectionFactory)} was null.");
        }

        logger?.LogInformation(
            "Registers a named client with a lifetime: {lifetime}. The name of the client is: {clientName}."
            + "Default {JsonSerializerSettings} are {defaultSerializerSettings} set",
            nameof(lifetime),
            clientName,
            nameof(JsonSerializerSettings),
            defaultSerializerSettings == null ? "not " : "");

        return AddArangoClient(
            services,
            connectionFactory,
            logger,
            lifetime,
            _ => defaultSerializerSettings,
            clientName);
    }

    /// <summary>
    ///     Registers an ArangoDB client in the specified <paramref name="services" /> collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to register the ArangoDB client.</param>
    /// <param name="connectionFactory">A delegate that provides the connection string for the ArangoDB client.</param>
    /// <param name="logger">An optional logger.</param>
    /// <param name="lifetime">The lifetime of the ArangoDB client.</param>
    /// <param name="defaultSerializerSettingsFactory">
    ///     A method that acts as a factory to create <see cref="JsonSerializerSettings" />
    ///     using the specified <see cref="IServiceProvider" />.
    /// </param>
    /// <param name="clientName">
    ///     The name of <see cref="IArangoDbClient" /> and the <see cref="HttpClient" /> in the
    ///     <see cref="IHttpClientFactory" />.
    /// </param>
    /// <returns>The <see cref="IServiceCollection" /> with the ArangoDB client services added.</returns>
    /// <exception cref="DuplicateNameException">An arango client with the same name is already registered.</exception>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="services" /> is <c>null</c>.<br />
    ///     <paramref name="connectionFactory" /> is <c>null</c>.
    ///     <paramref name="defaultSerializerSettingsFactory" /> is <c>null</c>.
    /// </exception>
    public static IServiceCollection AddArangoClient(
        this IServiceCollection services,
        Func<IServiceProvider, ArangoConfiguration> connectionFactory,
        ILogger logger,
        ServiceLifetime lifetime,
        Func<IServiceProvider, JsonSerializerSettings> defaultSerializerSettingsFactory,
        string clientName)
    {
        if (services == null)
        {
            throw new ArgumentNullException($"The variable {nameof(services)} was null!");
        }

        if (connectionFactory == null)
        {
            throw new ArgumentNullException($"The parameter {nameof(connectionFactory)} was null.");
        }

        if (defaultSerializerSettingsFactory == null)
        {
            throw new ArgumentNullException(nameof(defaultSerializerSettingsFactory));
        }

        logger?.LogInformation(
            "Registers a named client with a lifetime: {lifetime}. The name of the client is: {clientName}.",
            nameof(lifetime),
            clientName);

        services.TryAddTransient<TimeoutHttpHandler>();

        services.AddHttpClient(clientName)
                .AddHttpMessageHandler<TimeoutHttpHandler>()
                .SetHandlerLifetime(Timeout.InfiniteTimeSpan)
                // Note that we need to disable the HttpClient’s timeout by setting it to an infinite value,
                // otherwise the default behavior will interfere with the timeout handler.
                .ConfigureHttpClient(c => c.Timeout = Timeout.InfiniteTimeSpan);

        logger?.LogInformation(
            "Try to register the arango db client of type {IArangoDbClient}.",
            nameof(IArangoDbClient));

        // remove this handler to prevent the client to log too much BS
        services.RemoveAll<IHttpMessageHandlerBuilderFilter>();

        services.Add(
            new ServiceDescriptor(
                typeof(IArangoDbClient),
                p => new ArangoDbClient(
                    clientName,
                    connectionFactory.Invoke(p)?.ConnectionString,
                    p.GetRequiredService<IHttpClientFactory>(),
                    MapToExceptionOptions(p, connectionFactory),
                    defaultSerializerSettingsFactory.Invoke(p)),
                lifetime));

        return services;
    }

    /// <summary>
    ///     Registers an arango client.
    /// </summary>
    /// <param name="services">The services collection to register the client.</param>
    /// <param name="connectionFactory">The connection string for the client.</param>
    /// <param name="loggerCreation">A function to retrieve a logging instance.</param>
    /// <param name="lifetime">The lifetime of the arango client.</param>
    /// <param name="defaultSerializerSettings">Default serializer settings as optional parameter.</param>
    /// <param name="clientName">
    ///     The name of <see cref="IArangoDbClient" /> and the <see cref="HttpClient" /> in the
    ///     <see cref="IHttpClientFactory" />.
    /// </param>
    /// <returns>The service collection <see cref="IServiceCollection" />.</returns>
    /// <exception cref="DuplicateNameException">An arango client with the same name is already registered.</exception>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="services" /> is <c>null</c>.<br />
    ///     <paramref name="connectionFactory" /> is <c>null</c>.
    ///     <paramref name="loggerCreation" /> is <c>null</c>.
    /// </exception>
    public static IServiceCollection AddArangoClient(
        this IServiceCollection services,
        Func<IServiceProvider, ArangoConfiguration> connectionFactory,
        Func<IServiceProvider, ILogger> loggerCreation,
        ServiceLifetime lifetime,
        JsonSerializerSettings defaultSerializerSettings,
        string clientName)
    {
        if (services == null)
        {
            throw new ArgumentNullException($"The variable {nameof(services)} was null!");
        }

        if (connectionFactory == null)
        {
            throw new ArgumentNullException($"The parameter {nameof(connectionFactory)} was null.");
        }

        if (loggerCreation == null)
        {
            throw new ArgumentNullException(nameof(loggerCreation));
        }
        
        return AddArangoClient(
            services,
            connectionFactory,
            loggerCreation,
            lifetime,
            _ => defaultSerializerSettings,
            clientName);
    }

    /// <summary>
    ///     Registers an ArangoDB client in the specified <paramref name="services" /> collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to register the ArangoDB client.</param>
    /// <param name="connectionFactory">A delegate that provides the connection string for the ArangoDB client.</param>
    /// <param name="loggerCreation">A function to retrieve an instance of a logger for the ArangoDB client.</param>
    /// <param name="lifetime">The lifetime of the ArangoDB client.</param>
    /// <param name="defaultSerializerSettingsFactory">
    ///     A method that acts as a factory to create <see cref="JsonSerializerSettings" />
    ///     using the specified <see cref="IServiceProvider" />.
    /// </param>
    /// <param name="clientName">
    ///     The name of <see cref="IArangoDbClient" /> and the <see cref="HttpClient" /> in the
    ///     <see cref="IHttpClientFactory" />.
    /// </param>
    /// <returns>The <see cref="IServiceCollection" /> with the ArangoDB client services added.</returns>
    /// <exception cref="DuplicateNameException">An arango client with the same name is already registered.</exception>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="services" /> is <c>null</c>.<br />
    ///     <paramref name="connectionFactory" /> is <c>null</c>.
    ///     <paramref name="loggerCreation" /> is <c>null</c>.
    ///     <paramref name="defaultSerializerSettingsFactory" /> is <c>null</c>.
    /// </exception>
    public static IServiceCollection AddArangoClient(
        this IServiceCollection services,
        Func<IServiceProvider, ArangoConfiguration> connectionFactory,
        Func<IServiceProvider, ILogger> loggerCreation,
        ServiceLifetime lifetime,
        Func<IServiceProvider, JsonSerializerSettings> defaultSerializerSettingsFactory,
        string clientName)
    {
        if (services == null)
        {
            throw new ArgumentNullException($"The variable {nameof(services)} was null!");
        }

        if (connectionFactory == null)
        {
            throw new ArgumentNullException($"The parameter {nameof(connectionFactory)} was null.");
        }

        if (loggerCreation == null)
        {
            throw new ArgumentNullException(nameof(loggerCreation));
        }

        if (defaultSerializerSettingsFactory == null)
        {
            throw new ArgumentNullException(nameof(defaultSerializerSettingsFactory));
        }

        services.TryAddTransient<TimeoutHttpHandler>();
        // The client is logging too much BS, so we turned logging of.

        services.AddHttpClient(clientName)
            .SetHandlerLifetime(Timeout.InfiniteTimeSpan)
            .AddHttpMessageHandler<TimeoutHttpHandler>()
            // Note that we need to disable the HttpClient’s timeout by setting it to an infinite value,
            // otherwise the default behavior will interfere with the timeout handler.
            .ConfigureHttpClient(c => c.Timeout = Timeout.InfiniteTimeSpan);

        // remove this handler to prevent the client to log to much BS
        services.RemoveAll<IHttpMessageHandlerBuilderFilter>();

        services.Add(
            new ServiceDescriptor(
                typeof(IArangoDbClient),
                p => new ArangoDbClient(
                    clientName,
                    connectionFactory.Invoke(p)?.ConnectionString,
                    loggerCreation.Invoke(p),
                    p.GetRequiredService<IHttpClientFactory>(),
                    MapToExceptionOptions(p, connectionFactory),
                    defaultSerializerSettingsFactory.Invoke(p)),
                lifetime));

        return services;
    }

    /// <summary>
    ///     Registers the arango db client in a fluent method.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> for registering services.</param>
    /// <param name="logger">The logger for logging purposes.</param>
    /// <param name="lifeLifetime">The service lifetime of the <see cref="IArangoDbClientFactory" />.</param>
    /// <returns>A <see cref="IClientFactoryBuilder" /> for using fluent api.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services" /> is <c>null</c>.</exception>
    public static IClientFactoryBuilder AddArangoClientFactory(
        this IServiceCollection services,
        ILogger logger = null,
        ServiceLifetime lifeLifetime = ServiceLifetime.Scoped)
    {
        if (services == null)
        {
            throw new ArgumentNullException($"The parameter {nameof(services)} was null!");
        }

        logger?.LogInformation(
            "Try to register the {IArangoDbClientFactory} with the implementation {DependencyInjectionArangoDbClientFactory}.",
            nameof(IArangoDbClientFactory),
            nameof(DependencyInjectionArangoDbClientFactory));

        services.TryAdd(
            new ServiceDescriptor(
                typeof(IArangoDbClientFactory),
                p => new DependencyInjectionArangoDbClientFactory(p),
                lifeLifetime));

        return new DefaultClientFactoryBuilder(services, logger);
    }

    /// <summary>
    ///     Registers the arango db client in a fluent method.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> for registering services.</param>
    /// <param name="loggerCreation">
    ///     A function to retrieve logging instances from service provider that will take log
    ///     requests.
    /// </param>
    /// <param name="lifeLifetime">The service lifetime of the <see cref="IArangoDbClientFactory" />.</param>
    /// <returns>A <see cref="IClientFactoryBuilder" /> for using fluent api.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services" /> is <c>null</c>.</exception>
    public static IClientFactoryBuilder AddArangoClientFactory(
        this IServiceCollection services,
        Func<IServiceProvider, ILogger> loggerCreation,
        ServiceLifetime lifeLifetime = ServiceLifetime.Scoped)
    {
        if (services == null)
        {
            throw new ArgumentNullException($"The parameter {nameof(services)} was null!");
        }

        if (loggerCreation == null)
        {
            throw new ArgumentNullException(nameof(loggerCreation));
        }

        services.TryAdd(
            new ServiceDescriptor(
                typeof(IArangoDbClientFactory),
                p => new DependencyInjectionArangoDbClientFactory(p),
                lifeLifetime));

        return new DefaultClientFactoryBuilder(services, loggerCreation);
    }
}
