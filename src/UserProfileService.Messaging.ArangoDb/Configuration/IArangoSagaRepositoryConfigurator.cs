using MassTransit;
using Maverick.Client.ArangoDb.Public.Configuration;
using Maverick.Client.ArangoDb.Public.Extensions;

// Implementation was based on the redis implementation of redis from the MassTransit project. 
// https://github.com/MassTransit/MassTransit/tree/develop/src/Persistence/MassTransit.RedisIntegration
namespace UserProfileService.Messaging.ArangoDb.Configuration;

/// <summary>
///     Describes the configurator for generic arango repository.
/// </summary>
public interface IArangoSagaRepositoryConfigurator
{
    /// <summary>
    ///     Collection name to save sagas in.
    /// </summary>
    string CollectionName { set; }

    /// <summary>
    ///     Concurrency mode for saving saga state.
    /// </summary>
    ConcurrencyMode ConcurrencyMode { set; }

    /// <summary>
    ///     Set the database factory using configuration, which caches a <see cref="IArangoDbClient" /> under the hood.
    /// </summary>
    /// <param name="clientName">Name of client.</param>
    /// <param name="configuration">Configuration of Arango</param>
    void DatabaseConfiguration(string clientName, ArangoConfiguration configuration);
}

/// <summary>
///     Describes the configurator for arango repository of <see cref="TSaga" />.
/// </summary>
/// <typeparam name="TSaga">Type of saga repository to configure.</typeparam>
public interface IArangoSagaRepositoryConfigurator<TSaga> :
    IArangoSagaRepositoryConfigurator
    where TSaga : class, ISaga
{
}
