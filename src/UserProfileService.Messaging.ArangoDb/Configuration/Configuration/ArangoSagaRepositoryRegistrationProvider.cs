using System;
using MassTransit;
using MassTransit.Configuration;
using MassTransit.Internals;

// Implementation was based on the redis implementation of redis from the MassTransit project. 
// https://github.com/MassTransit/MassTransit/tree/develop/src/Persistence/MassTransit.RedisIntegration
namespace UserProfileService.Messaging.ArangoDb.Configuration.Configuration;

/// <summary>
///     Arango implementation of <see cref="ISagaRepositoryRegistrationProvider" />.
/// </summary>
public class ArangoSagaRepositoryRegistrationProvider :
    ISagaRepositoryRegistrationProvider
{
    private readonly Action<IArangoSagaRepositoryConfigurator> _configure;

    /// <summary>
    ///     Create an instance of <see cref="ArangoSagaRepositoryRegistrationProvider" />.
    /// </summary>
    /// <param name="configure">Action to configure saga repository.</param>
    public ArangoSagaRepositoryRegistrationProvider(Action<IArangoSagaRepositoryConfigurator> configure)
    {
        _configure = configure;
    }

    /// <inheritdoc />
    void ISagaRepositoryRegistrationProvider.Configure<TSaga>(ISagaRegistrationConfigurator<TSaga> configurator)
        where TSaga : class
    {
        if (configurator == null)
        {
            throw new ArgumentNullException(nameof(configurator));
        }

        if (!typeof(TSaga).HasInterface<ISagaVersion>())
        {
            return;
        }

        IProxy proxy = (IProxy)Activator.CreateInstance(
                typeof(Proxy<>).MakeGenericType(typeof(TSaga)),
                configurator)
            ?? throw new InvalidOperationException($"Could not create an instance of {typeof(Proxy<>).MakeGenericType(typeof(TSaga)).FullName}");

        proxy.Configure(this);
    }
    
    /// <summary>
    ///     Configure arango repository for saga using given configurator.
    /// </summary>
    /// <typeparam name="TSaga">Saga to configure.</typeparam>
    /// <param name="configurator">Configurator to use for saga repository.</param>
    protected virtual void Configure<TSaga>(ISagaRegistrationConfigurator<TSaga> configurator)
        where TSaga : class, ISagaVersion
    {
        if (configurator == null)
        {
            throw new ArgumentNullException(nameof(configurator));
        }

        configurator.ArangoRepository(r => _configure?.Invoke(r));
    }

    /// <summary>
    ///     Describes a proxy to configure saga repo.
    /// </summary>
    private interface IProxy
    {
        /// <summary>
        ///     Configure arango saga repository through registration provider.
        /// </summary>
        /// <typeparam name="T">Type of provider.</typeparam>
        /// <param name="provider">Provider to use.</param>
        public void Configure<T>(T provider)
            where T : ArangoSagaRepositoryRegistrationProvider;
    }

    /// <summary>
    ///     Default implementation of <see cref="IProxy" />.
    /// </summary>
    /// <typeparam name="TSaga"></typeparam>
    private class Proxy<TSaga> :
        IProxy
        where TSaga : class, ISagaVersion
    {
        private readonly ISagaRegistrationConfigurator<TSaga> _configurator;

        /// <summary>
        ///     Create an instance of <see cref="Proxy{TSaga}" />.
        /// </summary>
        /// <param name="configurator">Configuration to use for proxy.</param>
        public Proxy(ISagaRegistrationConfigurator<TSaga> configurator)
        {
            _configurator = configurator;
        }

        /// <inheritdoc />
        public void Configure<T>(T provider)
            where T : ArangoSagaRepositoryRegistrationProvider
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            provider.Configure(_configurator);
        }
    }
}
