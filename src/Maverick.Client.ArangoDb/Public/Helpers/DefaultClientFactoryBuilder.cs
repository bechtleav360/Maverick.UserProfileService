﻿using System;
using Maverick.Client.ArangoDb.Public.Configuration;
using Maverick.Client.ArangoDb.Public.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Maverick.Client.ArangoDb.Public.Helpers;

/// <summary>
///     The factory is used to build an arango client via
///     a factory.
/// </summary>
public class DefaultClientFactoryBuilder : IClientFactoryBuilder
{
    private readonly ILogger _logger;
    private readonly Func<IServiceProvider, ILogger> _loggerCreation;
    private readonly IServiceCollection _services;

    /// <summary>
    ///     The constructor of the instance <see cref="DefaultClientFactoryBuilder" />.
    /// </summary>
    /// <param name="services">The service collection to register services.</param>
    /// <param name="logger">An optional logger for logging purposes.</param>
    public DefaultClientFactoryBuilder(IServiceCollection services, ILogger logger = null)
    {
        _services = services;
        _logger = logger;
    }

    /// <summary>
    ///     The constructor of the instance <see cref="DefaultClientFactoryBuilder" />.
    /// </summary>
    /// <param name="services">The service collection to register services.</param>
    /// <param name="loggerCreation">A function to retrieve a logging instance.</param>
    public DefaultClientFactoryBuilder(
        IServiceCollection services,
        Func<IServiceProvider, ILogger> loggerCreation = null)
    {
        _services = services;
        _loggerCreation = loggerCreation;
    }

    /// <inheritdoc />
    public IClientFactoryBuilder AddArangoClient<TJsonSettings>(
        string name,
        Func<IServiceProvider, ArangoConfiguration> connectionFactory,
        ServiceLifetime lifetime = ServiceLifetime.Scoped,
        TJsonSettings defaultSerializerSettings = null)
        where TJsonSettings : class, IArangoClientJsonSettings
    {
        if (name == null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException($"Parameter {nameof(name)} cannot be empty or whitespace.", nameof(name));
        }

        if (defaultSerializerSettings != null)
        {
            _services.TryAddSingleton(defaultSerializerSettings);
        }

        if (_loggerCreation != null)
        {
            _services.AddArangoClient(
                connectionFactory,
                _loggerCreation,
                lifetime,
                p => (p.GetService<TJsonSettings>() as IArangoClientJsonSettings ?? new DefaultArangoClientJsonSettings())
                    .SerializerSettings,
                name);

            return this;
        }

        _services.AddArangoClient(
            connectionFactory,
            _logger,
            lifetime,
            p => (p.GetService<TJsonSettings>() as IArangoClientJsonSettings ?? new DefaultArangoClientJsonSettings())
                .SerializerSettings,
            name);

        return this;
    }

    /// <inheritdoc />
    public IClientFactoryBuilder AddArangoClient<TJsonSettings>(
        string name,
        ArangoConfiguration arangoConfiguration,
        ServiceLifetime lifetime = ServiceLifetime.Scoped,
        TJsonSettings defaultSerializerSettings = null)
        where TJsonSettings : class, IArangoClientJsonSettings
    {
        if (arangoConfiguration == null)
        {
            throw new ArgumentNullException(nameof(arangoConfiguration));
        }

        if (defaultSerializerSettings != null)
        {
            _services.TryAddSingleton(defaultSerializerSettings);
        }

        if (_loggerCreation != null)
        {
            _services.AddArangoClient(
                _ => arangoConfiguration,
                _loggerCreation,
                lifetime,
                p => (p.GetService<TJsonSettings>() as IArangoClientJsonSettings ?? new DefaultArangoClientJsonSettings())
                    .SerializerSettings,
                name);

            return this;
        }

        _services.AddArangoClient(
            _ => arangoConfiguration,
            _logger,
            lifetime,
            p => (p.GetService<TJsonSettings>() as IArangoClientJsonSettings ?? new DefaultArangoClientJsonSettings())
                .SerializerSettings,
            name);

        return this;
    }
}
