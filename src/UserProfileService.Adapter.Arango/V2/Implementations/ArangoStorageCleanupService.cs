using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.Public.Models.Collection;
using Microsoft.Extensions.Logging;
using UserProfileService.Adapter.Arango.V2.Abstractions;
using UserProfileService.Adapter.Arango.V2.Contracts;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Contracts;

namespace UserProfileService.Adapter.Arango.V2.Implementations;

/// <summary>
///     Represents the ArangoDb implementation of <see cref="IStorageCleanupService" />.
/// </summary>
public class ArangoStorageCleanupService : ArangoRepositoryBase, IStorageCleanupService
{
    /// <inheritdoc cref="ArangoRepositoryBase" />
    protected override string ArangoDbClientName { get; }

    /// <inheritdoc cref="IStorageCleanupService" />
    public string RelevantFor => "Arango";

    /// <summary>
    ///     Initializes a new instance of <see cref="ArangoStorageCleanupService" /> with a specified logger instance, the
    ///     service provider and an ArangoDb client name.
    /// </summary>
    /// <param name="logger">The logger that accepts logging messages.</param>
    /// <param name="serviceProvider">The service provider that contains previously registered services.</param>
    /// <param name="arangoDbClientName">A name of an ArangoDb client to generate clients.</param>
    public ArangoStorageCleanupService(
        ILogger logger,
        IServiceProvider serviceProvider,
        string arangoDbClientName) : base(logger, serviceProvider)
    {
        ArangoDbClientName = arangoDbClientName ?? ArangoConstants.DatabaseClientNameUserProfileStorage;
    }

    private async Task CleanupInternal(
        List<string> usedPrefixes,
        CancellationToken cancellationToken = default)
    {
        List<Type> relevantTypes =
            AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(
                    a => a.GetTypes()
                        .Where(t => typeof(ICollectionDetailsProvider).IsAssignableFrom(t) && !t.IsInterface))
                .ToList();

        var collectionNames = new List<string>();

        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        foreach (Type relevantType in relevantTypes)
        {
            ConstructorInfo ctor = relevantType
                .GetConstructors()
                .FirstOrDefault(
                    c =>
                        c.GetParameters().All(p => p.ParameterType == typeof(string))
                        && c.GetParameters().Length == 1);

            if (ctor == null)
            {
                Logger.LogWarnMessage(
                    "Constructor of type {type} is missing.",
                    LogHelpers.Arguments(relevantType.Name));

                continue;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            foreach (string prefix in usedPrefixes)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                var detailsProvider = (ICollectionDetailsProvider)ctor.Invoke(new object[] { prefix });

                collectionNames.AddRange(
                    detailsProvider
                        .GetCollectionDetails()
                        ?
                        .Select(d => d?.CollectionName)
                        .Where(name => !string.IsNullOrWhiteSpace(name))
                    ?? new List<string>());
            }
        }

        collectionNames = collectionNames.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        var deletedCollections = 0;

        foreach (string collectionName in collectionNames)
        {
            bool deleted;

            try
            {
                deleted = await DeleteAsync(
                    collectionName,
                    cancellationToken: cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception e)
            {
                Logger.LogErrorMessage(
                    null,
                    "Error occurred during deletion of {collection}. {message}.",
                    Arguments(collectionName, e.Message));

                continue;
            }

            if (!deleted)
            {
                continue;
            }

            ++deletedCollections;

            Logger.LogInfoMessage(
                "Collection {collectionName} successfully deleted.",
                LogHelpers.Arguments(collectionName));
        }

        if (deletedCollections > 0)
        {
            Logger.LogInfoMessage(
                "{count} collections successfully deleted (total amount).",
                Arguments(deletedCollections));
        }
        else
        {
            Logger.LogInfoMessage(
                "No collections deleted.",
                Arguments());
        }
    }

    /// <summary>
    ///     Deletes a collection. If it is still existent, it will try to delete it again (max retries:
    ///     <paramref name="retryCount" />).<br />
    ///     The method return true, if collection has been deleted.
    /// </summary>
    private async Task<bool> DeleteAsync(
        string collectionName,
        int retryCount = 5,
        CancellationToken cancellationToken = default)
    {
        var retried = 0;

        while (!cancellationToken.IsCancellationRequested && retried < retryCount)
        {
            cancellationToken.ThrowIfCancellationRequested();

            DeleteCollectionResponse firstResponse = await SendRequestAsync(
                client => client
                    .DeleteCollectionAsync(collectionName),
                cancellationToken: cancellationToken);

            if (firstResponse.Code == HttpStatusCode.NotFound)
            {
                return false;
            }

            GetCollectionResponse deleteResponse = await SendRequestAsync(
                client => client.GetCollectionAsync(collectionName),
                cancellationToken: cancellationToken);

            // if collection has been deleted
            if (deleteResponse.Code == HttpStatusCode.NotFound)
            {
                return true;
            }

            retried++;
        }

        return false;
    }

    /// <inheritdoc cref="IStorageCleanupService" />
    public Task CleanupAll(object argument = null, CancellationToken cancellationToken = default)
    {
        List<string> wellKnownPrefixes = typeof(WellKnownDatabasePrefixes)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(p => p.FieldType == typeof(string))
            .Select(p => (string)p.GetValue(null))
            .Where(prefix => !string.IsNullOrWhiteSpace(prefix))
            .ToList();

        return CleanupInternal(wellKnownPrefixes, cancellationToken);
    }

    /// <inheritdoc />
    public Task CleanupMainProjectionDataAsync(
        object argument = null,
        CancellationToken cancellationToken = default)
    {
        Logger.LogWarnMessage(
            "Cleaning all repo data of projections regarding first-level projection and sagas.",
            LogHelpers.Arguments());

        return CleanupInternal(
            new List<string>
            {
                WellKnownDatabasePrefixes.SagaWorker,
                WellKnownDatabasePrefixes.FirstLevelProjection
            },
            cancellationToken);
    }

    /// <inheritdoc />
    public Task CleanupExtendedProjectionDataAsync(
        object argument = null,
        CancellationToken cancellationToken = default)
    {
        Logger.LogWarnMessage(
            "Cleaning all projection data of API including bridge.",
            LogHelpers.Arguments());

        return CleanupInternal(
            new List<string>
            {
                WellKnownDatabasePrefixes.ApiService,
                WellKnownDatabasePrefixes.Bridge
            },
            cancellationToken);
    }
}
