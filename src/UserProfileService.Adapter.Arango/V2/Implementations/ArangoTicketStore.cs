using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.Public;
using Maverick.Client.ArangoDb.Public.Extensions;
using Maverick.Client.ArangoDb.Public.Models.Document;
using Maverick.UserProfileService.Models.Abstraction;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UserProfileService.Adapter.Arango.V2.Abstractions;
using UserProfileService.Adapter.Arango.V2.Contracts;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Adapter.Arango.V2.EntityModels.QueryBuilding;
using UserProfileService.Adapter.Arango.V2.Extensions;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.TicketStore.Abstractions;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Adapter.Arango.V2.Implementations;

/// <summary>
///     An implementation of the <see cref="ITicketStore" /> using ArangoDb.
/// </summary>
internal class ArangoTicketStore : ArangoRepositoryBase, ITicketStore
{
    private readonly IDbInitializer _initializer;
    private readonly ModelBuilderOptions _modelsInfo;
    private readonly JsonSerializerSettings _serializerSettings;

    /// <inheritdoc cref="ArangoRepositoryBase.ArangoDbClientName" />
    protected override string ArangoDbClientName { get; }

    [ActivatorUtilitiesConstructor]
    public ArangoTicketStore(
        ILogger<ArangoTicketStore> logger,
        IServiceProvider serviceProvider,
        IJsonSerializerSettingsProvider jsonSerializerSettings,
        IDbInitializer initializer)
        : this(
            logger,
            serviceProvider,
            jsonSerializerSettings?.GetNewtonsoftSettings(),
            initializer,
            ArangoConstants.DatabaseClientNameTicketStore,
            WellKnownDatabaseKeys.CollectionPrefixUserProfileService)
    {
    }

    public ArangoTicketStore(
        ILogger<ArangoTicketStore> logger,
        IServiceProvider serviceProvider,
        JsonSerializerSettings serializerSettings,
        IDbInitializer initializer,
        string clientName,
        string collectionPrefix) : base(logger, serviceProvider)
    {
        ArangoDbClientName = clientName;
        _modelsInfo = DefaultModelConstellation.CreateNewTicketStore(collectionPrefix).ModelsInfo;
        _serializerSettings = serializerSettings;
        _initializer = initializer;
    }

    /// <inheritdoc />
    public async Task<TicketBase> AddOrUpdateEntryAsync(
        TicketBase entry,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (entry == null)
        {
            throw new ArgumentNullException(nameof(entry));
        }

        if (!Regex.IsMatch(entry.Id, ArangoConstants.ArangoKeyPattern))
        {
            throw new ArgumentException("The given id is not compatible with ArangoDb.", nameof(entry));
        }

        await _initializer.EnsureDatabaseAsync(false, cancellationToken);

        await base.SendRequestAsync(
            client =>
                client.CreateDocumentAsync(
                    _modelsInfo.GetCollectionName<TicketBase>(),
                    entry.InjectDocumentKey(e => e.Id, _serializerSettings),
                    new CreateDocumentOptions
                    {
                        Overwrite = true,
                        OverWriteMode = AOverwriteMode.Replace
                    }),
            cancellationToken: cancellationToken);

        return Logger.ExitMethod(entry);
    }

    /// <inheritdoc />
    public async Task<TicketBase> GetTicketAsync(string id, CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (id == null)
        {
            throw new ArgumentNullException(nameof(id));
        }

        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(id));
        }

        if (!Regex.IsMatch(id, ArangoConstants.ArangoKeyPattern))
        {
            // as no document can be found with an invalid _key
            return null;
        }

        try
        {
            await _initializer.EnsureDatabaseAsync(false, cancellationToken);

            GetDocumentResponse<TicketBase> result = await base.SendRequestAsync(
                client =>
                    client.GetDocumentAsync<TicketBase>(_modelsInfo.GetCollectionName<TicketBase>() + "/" + id),
                cancellationToken: cancellationToken);

            return Logger.ExitMethod(result.Result);
        }
        catch (JsonSerializationException e)
        {
            throw new InvalidDataException("Unable to parse the Ticket. Is JsonSubTypes setup correctly?", e);
        }
    }

    /// <inheritdoc />
    public async Task<int> DeleteTicketsAsync(
        Expression<Func<TicketBase, bool>> filter = null,
        CancellationToken cancellationToken = default)
    {
        await _initializer.EnsureDatabaseAsync(false, cancellationToken);
        IArangoDbEnumerable<TicketBase> enumerable = new ArangoDbEnumerable<TicketBase>(_modelsInfo);

        if (filter != null)
        {
            enumerable = enumerable.Where(filter);
        }

        IArangoDbQueryResult query = enumerable.Select(t => t.Id).Compile(CollectionScope.Command);

        var context = CallingServiceContext.CreateNewOf<ArangoTicketStore>();

        MultiApiResponse<string> response = await SendRequestAsync(
            client => client.ExecuteQueryAsync<string>(query.GetQueryString(), cancellationToken: cancellationToken),
            true,
            true,
            context,
            cancellationToken);

        List<string> toDelete = response.QueryResult.Select(id => id.Trim('"')).ToList();

        if (toDelete.Any())
        {
            await SendRequestAsync(
                client => client.DeleteDocumentsAsync(
                    _modelsInfo.GetCollectionName<TicketBase>(),
                    toDelete),
                true,
                true,
                context,
                cancellationToken);
        }

        return toDelete.Count;
    }

    /// <inheritdoc />
    public async Task<IList<TicketBase>> GetTicketsAsync(
        Expression<Func<TicketBase, bool>> filter = null,
        int page = 0,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (page < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(page), page, "page must be greater than or equal to 0.");
        }

        if (pageSize < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageSize),
                pageSize,
                "pageSize must be greater than or equal to 0.");
        }

        await _initializer.EnsureDatabaseAsync(false, cancellationToken);

        IArangoDbEnumerable<TicketBase> enumerable = new ArangoDbEnumerable<TicketBase>(_modelsInfo);

        if (filter != null)
        {
            enumerable = enumerable.Where(filter);
        }

        IArangoDbQueryResult query = enumerable.UsingOptions(
                new QueryObjectBase
                {
                    Limit = pageSize,
                    Offset = page * pageSize
                })
            .Compile(CollectionScope.Command);

        MultiApiResponse<TicketBase> response = await SendRequestAsync(
            client =>
                client.ExecuteQueryAsync<TicketBase>(query.GetQueryString(), cancellationToken: cancellationToken),
            true,
            true,
            CallingServiceContext.CreateNewOf<ArangoTicketStore>(),
            cancellationToken);

        return Logger.ExitMethod(response.QueryResult.ToList());
    }
}
