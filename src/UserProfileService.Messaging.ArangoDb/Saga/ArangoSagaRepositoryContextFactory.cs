using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Saga;
using Maverick.Client.ArangoDb.Public;
using Maverick.Client.ArangoDb.Public.Extensions;
using Maverick.Client.ArangoDb.Public.Models.Collection;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Messaging.ArangoDb.Configuration;

// Implementation was based on the redis implementation of redis from the MassTransit project. 
// https://github.com/MassTransit/MassTransit/tree/develop/src/Persistence/MassTransit.RedisIntegration
namespace UserProfileService.Messaging.ArangoDb.Saga;

/// <summary>
///     Arango implementation of <see cref="ArangoSagaRepositoryContextFactory{TSaga}" />.
/// </summary>
/// <typeparam name="TSaga">Type of saga.</typeparam>
public class ArangoSagaRepositoryContextFactory<TSaga> :
    ISagaRepositoryQueryContextFactory<TSaga>
    where TSaga : class, ISagaVersion
{
    private readonly Func<IArangoDbClient> _clientFactory;
    private readonly ISagaConsumeContextFactory<IDatabaseContext<TSaga>, TSaga> _factory;
    private readonly ILogger<ArangoSagaRepositoryContextFactory<TSaga>> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ArangoSagaRepositoryOptions<TSaga> _options;

    /// <summary>
    ///     Create an instance of <see cref="ArangoSagaRepositoryContextFactory{TSaga}" />.
    /// </summary>
    /// <param name="clientFactory">Factory to create arango client.</param>
    /// <param name="factory">Factory to create <see cref="IDatabaseContext{TSaga}" />.</param>
    /// <param name="options">Options for arango saga repository.</param>
    /// <param name="loggerFactory">The factory to create logger.</param>
    public ArangoSagaRepositoryContextFactory(
        IArangoDbClientFactory clientFactory,
        ISagaConsumeContextFactory<IDatabaseContext<TSaga>, TSaga> factory,
        ArangoSagaRepositoryOptions<TSaga> options,
        ILoggerFactory loggerFactory)
    {
        IArangoDbClient DatabaseFactory()
        {
            return clientFactory.Create(options.ClientName);
        }

        _clientFactory = DatabaseFactory;

        _factory = factory;
        _options = options;
        _loggerFactory = loggerFactory;
        _logger = _loggerFactory.CreateLogger<ArangoSagaRepositoryContextFactory<TSaga>>();
    }

    /// <summary>
    ///     Create an instance of <see cref="ArangoSagaRepositoryContextFactory{TSaga}" />.
    /// </summary>
    /// <param name="clientFactory">Factory to create arango client.</param>
    /// <param name="factory">Factory to create <see cref="IDatabaseContext{TSaga}" />.</param>
    /// <param name="options">Options for arango saga repository.</param>
    /// <param name="loggerFactory">The factory to create logger.</param>
    public ArangoSagaRepositoryContextFactory(
        Func<IArangoDbClient> clientFactory,
        ISagaConsumeContextFactory<IDatabaseContext<TSaga>, TSaga> factory,
        ArangoSagaRepositoryOptions<TSaga> options,
        ILoggerFactory loggerFactory)
    {
        _clientFactory = clientFactory;

        _factory = factory;
        _options = options;
        _loggerFactory = loggerFactory;
        _logger = _loggerFactory.CreateLogger<ArangoSagaRepositoryContextFactory<TSaga>>();
    }

    private async Task CreateDatabaseSchemaAsync(IArangoDbClient database)
    {
        _logger.EnterMethod();

        if (database == null)
        {
            throw new ArgumentNullException(nameof(database));
        }

        GetAllCollectionsResponse collectionQuery = await database.GetAllCollectionsAsync();

        if (collectionQuery.Code == HttpStatusCode.NotFound)
        {
            throw new ConfigurationException("The configured database does not exist in the backend.");
        }

        if ((int)collectionQuery.Code < 200 || (int)collectionQuery.Code > 299)
        {
            throw new Exception(
                $"{nameof(CreateDatabaseSchemaAsync)}() Error occurred (Status code {collectionQuery.Code:D}: {collectionQuery.Exception?.Message}");
        }

        if (collectionQuery.Error || collectionQuery.Result == null)
        {
            throw new Exception(
                $"{nameof(CreateDatabaseSchemaAsync)}() Error occurred (Status code {collectionQuery.Code:D}: {collectionQuery.Exception?.Message}");
        }

        string[] alreadyCreatedCollectionNames = collectionQuery.Result
            .Where(
                c => !string.IsNullOrEmpty(c?.Name)
                    && !c.IsSystem)
            .Select(c => c.Name)
            .ToArray();

        if (!alreadyCreatedCollectionNames.Contains(_options.CollectionName))
        {
            var collectionRequest = new CreateCollectionBody
            {
                Name = _options.CollectionName,
                Type = ACollectionType.Document
            };

            await database.CreateCollectionAsync(collectionRequest);
        }

        _logger.ExitMethod();
    }

    /// <inheritdoc />
    public void Probe(ProbeContext context)
    {
        _logger.EnterMethod();

        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        context.Add("persistence", "arango");

        _logger.ExitMethod();
    }

    /// <inheritdoc />
    public async Task Send<T>(ConsumeContext<T> context, IPipe<SagaRepositoryContext<TSaga, T>> next)
        where T : class
    {
        _logger.EnterMethod();

        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (next == null)
        {
            throw new ArgumentNullException(nameof(next));
        }

        IArangoDbClient database = _clientFactory();

        await CreateDatabaseSchemaAsync(database);

        var databaseContext = new ArangoDatabaseContext<TSaga>(database, _options, _loggerFactory);

        try
        {
            var repositoryContext = new ArangoSagaRepositoryContext<TSaga, T>(
                databaseContext,
                context,
                _factory,
                _loggerFactory);

            await next.Send(repositoryContext).ConfigureAwait(false);
        }
        finally
        {
            await databaseContext.DisposeAsync().ConfigureAwait(false);

            _logger.ExitMethod();
        }
    }

    /// <inheritdoc />
    public Task SendQuery<T>(
        ConsumeContext<T> context,
        ISagaQuery<TSaga> query,
        IPipe<SagaRepositoryQueryContext<TSaga, T>> next)
        where T : class
    {
        _logger.EnterMethod();

        throw new NotImplementedByDesignException(
            "Arango saga repository does not support queries or is not yet implemented.");
    }

    /// <inheritdoc />
    public async Task<T> Execute<T>(
        Func<LoadSagaRepositoryContext<TSaga>, Task<T>> asyncMethod,
        CancellationToken cancellationToken = default)
        where T : class
    {
        _logger.EnterMethod();

        if (asyncMethod == null)
        {
            throw new ArgumentNullException(nameof(asyncMethod));
        }

        IArangoDbClient database = _clientFactory();

        await CreateDatabaseSchemaAsync(database);

        var databaseContext = new ArangoDatabaseContext<TSaga>(database, _options, _loggerFactory);

        try
        {
            var repositoryContext = new ArangoSagaRepositoryContext<TSaga>(
                databaseContext,
                _loggerFactory,
                cancellationToken);

            return await asyncMethod(repositoryContext).ConfigureAwait(false);
        }
        finally
        {
            await databaseContext.DisposeAsync().ConfigureAwait(false);
            _logger.ExitMethod();
        }
    }

    /// <inheritdoc />
    public async Task<T> Execute<T>(
        Func<QuerySagaRepositoryContext<TSaga>, Task<T>> asyncMethod,
        CancellationToken cancellationToken = default)
        where T : class
    {
        _logger.EnterMethod();

        if (asyncMethod == null)
        {
            throw new ArgumentNullException(nameof(asyncMethod));
        }

        IArangoDbClient database = _clientFactory();

        await CreateDatabaseSchemaAsync(database);

        var databaseContext = new ArangoDatabaseContext<TSaga>(database, _options, _loggerFactory);

        try
        {
            var repositoryContext = new ArangoSagaRepositoryContext<TSaga>(
                databaseContext,
                _loggerFactory,
                cancellationToken);

            return await asyncMethod(repositoryContext).ConfigureAwait(false);
        }
        finally
        {
            await databaseContext.DisposeAsync().ConfigureAwait(false);
            _logger.ExitMethod();
        }
    }

    /// <inheritdoc />
    public async Task<T> ExecuteQuery<T>(
        Func<ISagaRepositoryQueryContext<TSaga>, Task<T>> asyncMethod,
        CancellationToken cancellationToken = default) where T : class
    {
        _logger.EnterMethod();

        if (asyncMethod == null)
        {
            throw new ArgumentNullException(nameof(asyncMethod));
        }

        IArangoDbClient database = _clientFactory();

        await CreateDatabaseSchemaAsync(database);

        var databaseContext = new ArangoDatabaseContext<TSaga>(database, _options, _loggerFactory);

        try
        {
            var repositoryContext = new ArangoSagaRepositoryContext<TSaga>(
                databaseContext,
                _loggerFactory,
                cancellationToken);

            return await asyncMethod(repositoryContext).ConfigureAwait(false);
        }
        finally
        {
            await databaseContext.DisposeAsync().ConfigureAwait(false);
            _logger.ExitMethod();
        }
    }
}
