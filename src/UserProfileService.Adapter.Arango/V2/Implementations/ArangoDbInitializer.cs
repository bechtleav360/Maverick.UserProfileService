using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.Public;
using Maverick.Client.ArangoDb.Public.Configuration;
using Maverick.Client.ArangoDb.Public.Models.Collection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserProfileService.Adapter.Arango.V2.Abstractions;
using UserProfileService.Adapter.Arango.V2.Contracts;
using UserProfileService.Adapter.Arango.V2.Extensions;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Enums;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Common.V2.Models;

namespace UserProfileService.Adapter.Arango.V2.Implementations;

/// <summary>
///     Initializes the database schema in ArangoDB. It should be registered as singleton instance in an IOC container.
/// </summary>
public class ArangoDbInitializer : ArangoRepositoryBase, IDbInitializer
{
    private ArangoClusterConfiguration _arangoClusterConfiguration;
    private readonly IList<ICollectionDetailsProvider> _collectionDetailsProviders;
    private readonly object _configChangesLock = new object();
    private DateTime _lastInit = DateTime.MinValue;
    private int _timeBetweenChecks;

    private IOptionsMonitor<ArangoConfiguration> Options { get; }

    /// <inheritdoc />
    protected override string ArangoDbClientName { get; }

    // for test purpose
    internal ArangoDbInitializer(
        IOptionsMonitor<ArangoConfiguration> options,
        IServiceProvider serviceProvider,
        ILogger<ArangoDbInitializer> logger,
        string arangoDbClientName,
        IEnumerable<ICollectionDetailsProvider> collectionProviders) : this(
        options,
        serviceProvider,
        logger,
        collectionProviders)
    {
        ArangoDbClientName = arangoDbClientName;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ArangoDbInitializer" /> class providing a Logger factory and an
    ///     optional parameter to limit the timespan between database checks.
    /// </summary>
    /// <param name="options">The ArangoDB options object wrapped by a <see cref="IOptionsMonitor{TOptions}" /> item.</param>
    /// <param name="serviceProvider">The instance of <see cref="IServiceProvider" /> that is used to get required services.</param>
    /// <param name="logger">The logger that will accept logging messages from this instance.</param>
    /// <param name="collectionProviders"></param>
    [ActivatorUtilitiesConstructor]
    public ArangoDbInitializer(
        IOptionsMonitor<ArangoConfiguration> options,
        IServiceProvider serviceProvider,
        ILogger<ArangoDbInitializer> logger,
        IEnumerable<ICollectionDetailsProvider> collectionProviders) : base(logger, serviceProvider)
    {
        Options = options ?? throw new ArgumentNullException(nameof(options));
        Options.OnChange(OnConfigurationChanged);

        _arangoClusterConfiguration = options.CurrentValue?.ClusterConfiguration;
        _timeBetweenChecks = options.CurrentValue?.MinutesBetweenChecks ?? 480;
        _collectionDetailsProviders = collectionProviders.ToList();

        ArangoDbClientName = ArangoConstants.DatabaseClientNameUserProfileStorage;
    }

    private void OnConfigurationChanged(ArangoConfiguration newConfig, string sender)
    {
        lock (_configChangesLock)
        {
            if (newConfig == null)
            {
                return;
            }

            if (newConfig.ClusterConfiguration != null
                && (_arangoClusterConfiguration == null
                    || !newConfig.ClusterConfiguration.Equals(_arangoClusterConfiguration)))
            {
                Logger.LogInfoMessage("Cluster configuration has been changed.", LogHelpers.Arguments());
                _arangoClusterConfiguration = newConfig.ClusterConfiguration;
            }

            if (_timeBetweenChecks != newConfig.MinutesBetweenChecks && newConfig.MinutesBetweenChecks > 0)
            {
                Logger.LogInfoMessage(
                    "Configuration has been changed. New configured time between checks: {minutesNumber} minutes.",
                    LogHelpers.Arguments(newConfig.MinutesBetweenChecks));

                _timeBetweenChecks = newConfig.MinutesBetweenChecks;
            }
        }
    }

    private async Task<SchemaInitializationResponse> CreateDatabaseSchemaAsync(
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        GetAllCollectionsResponse collectionQuery =
            await SendRequestAsync(
                    client => client.GetAllCollectionsAsync(),
                    false,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);

        if (collectionQuery.Code == HttpStatusCode.NotFound)
        {
            throw new ConfigurationException("The configured database does not exist in the backend.");
        }

        if ((int)collectionQuery.Code < 200 || (int)collectionQuery.Code > 299)
        {
            throw collectionQuery.Exception.ToDatabaseException(
                $"{nameof(CreateDatabaseSchemaAsync)}() Error occurred (Status code {collectionQuery.Code:D}: {collectionQuery.Exception?.Message}");
        }

        if (collectionQuery.Error || collectionQuery.Result == null)
        {
            return Logger.ExitMethod(
                new SchemaInitializationResponse(
                    SchemaInitializationStatus.ErrorOccurred,
                    collectionQuery.Exception));
        }

        Logger.LogTraceMessage(
            "Iterating through document collections and add missing ones",
            LogHelpers.Arguments());

        string[] alreadyCreatedCollectionNames = collectionQuery.Result
            .Where(c => !string.IsNullOrEmpty(c?.Name) && !c.IsSystem)
            .Select(c => c.Name)
            .ToArray();

        bool[] collectionsCreated = await Task
            .WhenAll(
                _collectionDetailsProviders
                    .Select(
                        async p =>
                            await CreateCollectionsAsync(
                                p.GetCollectionDetails(),
                                alreadyCreatedCollectionNames,
                                cancellationToken)));

        Logger.LogInfoMessage(
            "{DatabaseSchema}",
            LogHelpers.Arguments(
                collectionsCreated.Any(b => b)
                    ? "Database schema updated."
                    : "Database schema checked."));

        SchemaInitializationStatus status = collectionsCreated.Any(b => b)
            ? SchemaInitializationStatus.SchemaCreated
            : SchemaInitializationStatus.Checked;

        return Logger.ExitMethod(new SchemaInitializationResponse(status));
    }

    private async Task<bool> CreateCollectionsAsync(
        IEnumerable<CollectionDetails> collectionsToCreate,
        ICollection<string> alreadyCreatedCollections,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        var created = false;

        foreach (CollectionDetails collection in collectionsToCreate)
        {
            if (alreadyCreatedCollections.Contains(collection.CollectionName))
            {
                continue;
            }

            Logger.LogDebugMessage(
                "{Edge/Document} collection {collectionName} has to be added to database.",
                LogHelpers.Arguments(
                    collection.CollectionType == ACollectionType.Edge ? "Edge" : "Document",
                    collection.CollectionName));

            CreateCollectionOptions createCollectionOptions = null;

            Dictionary<string, ArangoCollectionClusterConfiguration> arangoCollectionConfiguration =
                GetArangoClusterInformation(collection.CollectionType);

            if (arangoCollectionConfiguration != null
                && arangoCollectionConfiguration.ContainsKey(collection.CollectionName)
                && arangoCollectionConfiguration[collection.CollectionName] != null)
            {
                createCollectionOptions = new CreateCollectionOptions
                {
                    ReplicationFactor =
                        arangoCollectionConfiguration[collection.CollectionName].ReplicationFactor,
                    WriteConcern = arangoCollectionConfiguration[collection.CollectionName].WriteConcern,
                    NumberOfShards = arangoCollectionConfiguration[collection.CollectionName].NumberOfShards
                };

                Logger.LogInfoMessage(
                    "Set cluster configuration like in settings as key {collectionName}.",
                    LogHelpers.Arguments(collection.CollectionName));
            }

            if (createCollectionOptions == null
                && arangoCollectionConfiguration != null
                && arangoCollectionConfiguration.ContainsKey("*")
                && arangoCollectionConfiguration["*"] != null)
            {
                createCollectionOptions = new CreateCollectionOptions
                {
                    ReplicationFactor = arangoCollectionConfiguration["*"].ReplicationFactor,
                    WriteConcern = arangoCollectionConfiguration["*"].WriteConcern,
                    NumberOfShards = arangoCollectionConfiguration["*"].NumberOfShards
                };

                Logger.LogInfoMessage(
                    "Set cluster configuration like in settings as key '*'.",
                    LogHelpers.Arguments());
            }

            CreateCollectionResponse sourceQuery =
                await SendRequestAsync(
                        client => client.CreateCollectionAsync(
                            collection.CollectionName,
                            collection.CollectionType,
                            createCollectionOptions),
                        false,
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            if (sourceQuery.Error)
            {
                Logger.LogErrorMessage(
                    sourceQuery.Exception,
                    "{collectionType} collection {collectionName} could not be created, code: {ApiError}.",
                    LogHelpers.Arguments(
                        collection.CollectionType == ACollectionType.Edge ? "Edge" : "Document",
                        collection.CollectionName,
                        sourceQuery.Code));
            }

            created = true;
        }

        return created;
    }

    private Dictionary<string, ArangoCollectionClusterConfiguration>
        GetArangoClusterInformation(ACollectionType collectionType)
    {
        return collectionType == ACollectionType.Document
            ? _arangoClusterConfiguration?.DocumentCollections
            : _arangoClusterConfiguration?.EdgeCollections;
    }

    /// <inheritdoc cref="IDbInitializer" />
    public async Task<SchemaInitializationResponse> EnsureDatabaseAsync(
        bool forceRecreation = false,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        await LockProfileRepoAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (!forceRecreation && _lastInit.AddMinutes(_timeBetweenChecks) > DateTime.UtcNow)
            {
                Logger.LogDebugMessage(
                    "Not necessary to check. Next check in {minutes}.",
                    LogHelpers.Arguments(_lastInit.AddMinutes(_timeBetweenChecks)));

                return new SchemaInitializationResponse(SchemaInitializationStatus.WaitingForNextCheck);
            }

            SchemaInitializationResponse created = await CreateDatabaseSchemaAsync(cancellationToken)
                .ConfigureAwait(false);

            _lastInit = DateTime.UtcNow;

            return Logger.ExitMethod(created);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            Logger.LogError(
                new EventId(0),
                e,
                $"{e.GetType().Name} occurred during call of {nameof(ArangoDbInitializer)}.{nameof(EnsureDatabaseAsync)}(). {e.Message}{Environment.NewLine}Details: {e}");

            return Logger.ExitMethod(new SchemaInitializationResponse(SchemaInitializationStatus.ErrorOccurred, e));
        }
        finally
        {
            await UnlockProfileRepoAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
