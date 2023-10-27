using System;
using MassTransit;
using UserProfileService.Messaging.ArangoDb.Configuration.Configuration;

// Implementation was based on the redis implementation of redis from the MassTransit project. 
// https://github.com/MassTransit/MassTransit/tree/develop/src/Persistence/MassTransit.RedisIntegration
namespace UserProfileService.Messaging.ArangoDb.Configuration;

/// <summary>
///     Extension to register arango saga repository.
/// </summary>
public static class ArangoSagaRepositoryRegistrationExtensions
{
    /// <summary>
    ///     Adds a Arango saga repository to the registration
    /// </summary>
    /// <param name="configurator">Configurator of saga repository.</param>
    /// <param name="configure">Action to configure saga repository.</param>
    /// <typeparam name="T">Type of saga.</typeparam>
    /// <returns></returns>
    public static ISagaRegistrationConfigurator<T> ArangoRepository<T>(
        this ISagaRegistrationConfigurator<T> configurator,
        Action<IArangoSagaRepositoryConfigurator<T>> configure = null)
        where T : class, ISagaVersion
    {
        var repositoryConfigurator = new ArangoSagaRepositoryConfigurator<T>();

        configure?.Invoke(repositoryConfigurator);

        repositoryConfigurator.Validate()
            .ThrowIfContainsFailure("The Arango saga repository configuration is invalid:");

        configurator.Repository(x => repositoryConfigurator.Register(x));

        return configurator;
    }
}
