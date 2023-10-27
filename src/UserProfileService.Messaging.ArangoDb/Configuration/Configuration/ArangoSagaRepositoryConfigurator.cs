using System;
using System.Collections.Generic;
using MassTransit;
using MassTransit.Configuration;
using MassTransit.Saga;
using Maverick.Client.ArangoDb.Public.Configuration;
using Maverick.Client.ArangoDb.Public.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using UserProfileService.Messaging.ArangoDb.Saga;

// Implementation was based on the redis implementation of redis from the MassTransit project. 
// https://github.com/MassTransit/MassTransit/tree/develop/src/Persistence/MassTransit.RedisIntegration
namespace UserProfileService.Messaging.ArangoDb.Configuration.Configuration;

/// <summary>
///     Create an instance of <see cref="ArangoSagaRepositoryConfigurator{TSaga}" />.
/// </summary>
/// <typeparam name="TSaga">Type of saga.</typeparam>
public class ArangoSagaRepositoryConfigurator<TSaga> :
    IArangoSagaRepositoryConfigurator<TSaga>,
    ISpecification
    where TSaga : class, ISaga
{
    private ArangoConfiguration _arangoConfiguration;
    private string _clientName;

    private Func<IServiceProvider, IArangoDbClient> _connectionFactory;

    /// <inheritdoc />
    public string CollectionName { get; set; }

    /// <inheritdoc />
    public ConcurrencyMode ConcurrencyMode { get; set; }

    /// <summary>
    ///     Prefix for key generation in repository.
    /// </summary>
    public string KeyPrefix { get; set; }

    /// <summary>
    ///     Create an instance of <see cref="ArangoSagaRepositoryConfigurator{TSaga}" />.
    /// </summary>
    public ArangoSagaRepositoryConfigurator()
    {
        ConcurrencyMode = ConcurrencyMode.Optimistic;
        KeyPrefix = "";
        CollectionName = $"saga_state_{typeof(TSaga).Name}";
    }

    /// <inheritdoc />
    public void DatabaseConfiguration(string clientName, ArangoConfiguration arangoConfiguration)
    {
        if (string.IsNullOrWhiteSpace(clientName))
        {
            throw new ArgumentNullException(nameof(clientName));
        }

        _clientName = clientName;
        _arangoConfiguration = arangoConfiguration ?? throw new ArgumentNullException(nameof(arangoConfiguration));

        _connectionFactory = provider => provider.GetRequiredService<IArangoDbClientFactory>()
            .Create(_clientName);
    }

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate()
    {
        if (_connectionFactory == null)
        {
            yield return this.Failure("ConnectionFactory", "must be specified");
        }

        if (string.IsNullOrWhiteSpace(CollectionName))
        {
            yield return this.Failure("CollectionName", "Must be not null or empty");
        }

        if (string.IsNullOrWhiteSpace(_clientName))
        {
            yield return this.Failure("ClientName", "Must be not null or empty");
        }
    }

    /// <summary>
    ///     Method to register saga repository with configuration.
    /// </summary>
    /// <typeparam name="T">Type of saga.</typeparam>
    /// <param name="configurator">Configurator for saga repository.</param>
    public void Register<T>(ISagaRepositoryRegistrationConfigurator<T> configurator)
        where T : class, ISagaVersion
    {
        if (configurator == null)
        {
            throw new ArgumentNullException(nameof(configurator));
        }

        configurator.TryAddSingleton(_connectionFactory);

        configurator.TryAddSingleton(
            new ArangoSagaRepositoryOptions<T>(
                ConcurrencyMode,
                KeyPrefix,
                CollectionName,
                _clientName));

        configurator.AddArangoClientFactory()
            .AddArangoClient(
                _clientName,
                _arangoConfiguration,
                defaultSerializerSettings: new JsonSerializerSettings
                {
                    Converters = new List<JsonConverter>
                    {
                        new StringEnumConverter()
                    },
                    ContractResolver = new DefaultContractResolver()
                });

        // Custom query context factory to be used to get saga states in controller.
        configurator
            .AddScoped<ISagaRepositoryQueryContextFactory<T>, ArangoSagaRepositoryContextFactory<T>>();

        configurator
            .RegisterSagaRepository<T, IDatabaseContext<T>, SagaConsumeContextFactory<IDatabaseContext<T>, T>,
                ArangoSagaRepositoryContextFactory<T>>();
    }
}
