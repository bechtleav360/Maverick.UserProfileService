using System;
using System.Collections.Concurrent;
using System.Net.Http;
using Maverick.Client.ArangoDb.Public.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Maverick.Client.ArangoDb.Public.Extensions;

/// <summary>
///     The arango client factory implementation to registered more than
///     one arango client if needed.
/// </summary>
public class SingletonArangoDbClientFactory : IArangoDbClientFactory
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly ILogger<SingletonArangoDbClientFactory> _logger;

    private readonly ConcurrentDictionary<string, IArangoDbClient> _registeredClients =
        new ConcurrentDictionary<string, IArangoDbClient>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     Initializes a new instance of <see cref="SingletonArangoDbClientFactory" />.
    /// </summary>
    /// <param name="logger">The logger instance that will accept logger messages coming from this method.</param>
    /// <param name="clientFactory">
    ///     The <see cref="IHttpClientFactory" /> that will handle request to get
    ///     <see cref="HttpClient" />s.
    /// </param>
    public SingletonArangoDbClientFactory(
        IHttpClientFactory clientFactory,
        ILogger<SingletonArangoDbClientFactory> logger = null)
    {
        _clientFactory = clientFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public IArangoDbClient Create(string name)
    {
        _logger?.LogTrace($"Enter method {nameof(Create)}('{name}').");

        if (name == null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
        }

        if (!_registeredClients.TryGetValue(name, out IArangoDbClient client))
        {
            throw new ArgumentException($"No registered client called '{name}' found.", nameof(name));
        }

        _logger?.LogTrace($"Exit method {nameof(Create)}().");

        return client;
    }

    /// <summary>
    ///     Registers a client when not already registered.
    /// </summary>
    /// <param name="name">The name of the client to register.</param>
    /// <param name="connectionString">The connection string for the arango client.</param>
    /// <param name="exceptionOptions">Options, how to handle exceptions within the client.</param>
    /// <param name="settings">Optional serializer setting for the arango client.</param>
    public void RegisterClient(
        string name,
        string connectionString,
        ArangoExceptionOptions exceptionOptions = null,
        JsonSerializerSettings settings = null)
    {
        _logger?.LogTrace($"Enter method {nameof(RegisterClient)}('{name}').");

        if (name == null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
        }

        if (connectionString == null)
        {
            throw new ArgumentNullException(nameof(connectionString));
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(connectionString));
        }

        _registeredClients.GetOrAdd(
            name,
            key =>
                new ArangoDbClient(
                    key,
                    connectionString,
                    _clientFactory,
                    exceptionOptions,
                    settings));

        _logger?.LogTrace($"Exit method {nameof(RegisterClient)}().");
    }
}
