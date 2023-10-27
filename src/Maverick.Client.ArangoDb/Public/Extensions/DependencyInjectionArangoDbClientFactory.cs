using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Maverick.Client.ArangoDb.Public.Extensions;

/// <summary>
///     Implementation of <see cref="IArangoDbClientFactory" /> that uses <see cref="IServiceProvider" /> as container of
///     <see cref="IArangoDbClient" /> instances.
/// </summary>
public class DependencyInjectionArangoDbClientFactory : IArangoDbClientFactory
{
    private readonly ILogger<DependencyInjectionArangoDbClientFactory> _logger;
    private readonly IServiceProvider _provider;

    /// <summary>
    ///     The constructor for the class
    /// </summary>
    /// <param name="provider">The service provider that gets all required object.</param>
    public DependencyInjectionArangoDbClientFactory(IServiceProvider provider)
    {
        _provider = provider;
        _logger = _provider.GetService<ILogger<DependencyInjectionArangoDbClientFactory>>();
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentException">
    ///     If <paramref name="name" /> is empty or only contains whitespaces<br />
    ///     If <paramref name="name" /> refers to a client, that cannot be found in the list of registered services.
    /// </exception>
    /// <exception cref="ArgumentNullException">If <paramref name="name" /> is <c>null</c>.</exception>
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

        IArangoDbClient client = _provider.GetServices<IArangoDbClient>()?.FirstOrDefault(cl => cl.Name == name);

        if (client == null)
        {
            throw new ArgumentException($"The client with the name {name} could not be found!");
        }

        _logger?.LogTrace($"Exit method {nameof(Create)}('{name}').");

        return client;
    }
}
