using System;
using System.Data;
using Maverick.Client.ArangoDb.Public.Configuration;
using Maverick.Client.ArangoDb.Public.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Maverick.Client.ArangoDb.Public.Helpers;

/// <summary>
///     Builder interface that is used to set up the <see cref="IArangoDbClientFactory" /> registration in more detail.
/// </summary>
public interface IClientFactoryBuilder
{
    /// <summary>
    ///     Registers an instance of <see cref="IArangoDbClient" /> which can be used with the factory.
    /// </summary>
    /// <param name="name">The name of the <see cref="IArangoDbClient" /></param>
    /// <param name="connectionFactory">The connection config for the client.</param>
    /// <param name="lifetime">The lifetime of the arango client.</param>
    /// <param name="defaultSerializerSettings">Default JSON settings as optional parameter.</param>
    /// <returns>The modified instance of <see cref="IClientFactoryBuilder" />.</returns>
    /// <exception cref="DuplicateNameException">An arango client with the same name is already registered.</exception>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="connectionFactory" /> is <c>null</c>.
    /// </exception>
    IClientFactoryBuilder AddArangoClient<TJsonSettings>(
        string name,
        Func<IServiceProvider, ArangoConfiguration> connectionFactory,
        ServiceLifetime lifetime = ServiceLifetime.Scoped,
        TJsonSettings defaultSerializerSettings = null)
        where TJsonSettings : class, IArangoClientJsonSettings;

    /// <summary>
    ///     Registers an instance of <see cref="IArangoDbClient" /> which can be used with the factory.
    /// </summary>
    /// <param name="name">The name of the <see cref="IArangoDbClient" /></param>
    /// <param name="connectionConfiguration">The connection config for the client.</param>
    /// <param name="lifetime">The lifetime of the arango client.</param>
    /// <param name="defaultSerializerSettings">Default JSON settings as optional parameter.</param>
    /// <returns>The modified instance of <see cref="IClientFactoryBuilder" />.</returns>
    /// <exception cref="DuplicateNameException">An arango client with the same name is already registered.</exception>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="connectionConfiguration" /> is <c>null</c>.
    /// </exception>
    IClientFactoryBuilder AddArangoClient<TJsonSettings>(
        string name,
        ArangoConfiguration connectionConfiguration,
        ServiceLifetime lifetime = ServiceLifetime.Scoped,
        TJsonSettings defaultSerializerSettings = null)
        where TJsonSettings : class, IArangoClientJsonSettings;
}
