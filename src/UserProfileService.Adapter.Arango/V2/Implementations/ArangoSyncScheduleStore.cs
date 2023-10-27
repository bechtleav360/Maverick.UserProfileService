using System;
using System.Threading;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.Public;
using Maverick.Client.ArangoDb.Public.Extensions;
using Maverick.Client.ArangoDb.Public.Models.Document;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UserProfileService.Adapter.Arango.V2.Abstractions;
using UserProfileService.Adapter.Arango.V2.Contracts;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Sync.Abstraction.Models;
using UserProfileService.Sync.Abstraction.Stores;

namespace UserProfileService.Adapter.Arango.V2.Implementations;

internal class ArangoSyncScheduleStore : ArangoRepositoryBase, IScheduleStore
{
    private const string ScheduleKey = "current-schedule";
    private readonly IDbInitializer _initializer;
    private readonly ModelBuilderOptions _modelsInfo;
    private readonly JsonSerializerSettings _serializerSettings;

    /// <inheritdoc cref="ArangoRepositoryBase.ArangoDbClientName" />
    protected override string ArangoDbClientName { get; }

    [ActivatorUtilitiesConstructor]
    public ArangoSyncScheduleStore(
        ILogger<ArangoSyncScheduleStore> logger,
        IServiceProvider serviceProvider,
        IJsonSerializerSettingsProvider jsonSerializerSettings,
        IDbInitializer initializer)
        : this(
            logger,
            serviceProvider,
            jsonSerializerSettings?.GetNewtonsoftSettings(),
            initializer,
            ArangoConstants.DatabaseClientNameSync,
            WellKnownDatabaseKeys.CollectionPrefixSync)
    {
    }

    public ArangoSyncScheduleStore(
        ILogger<ArangoSyncScheduleStore> logger,
        IServiceProvider serviceProvider,
        JsonSerializerSettings serializerSettings,
        IDbInitializer initializer,
        string clientName,
        string collectionPrefix) : base(logger, serviceProvider)
    {
        ArangoDbClientName = clientName;
        _modelsInfo = DefaultModelConstellation.CreateSyncScheduleStore(collectionPrefix).ModelsInfo;
        _serializerSettings = serializerSettings;
        _initializer = initializer;
    }

    public async Task<SyncSchedule> GetScheduleAsync(CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        await _initializer.EnsureDatabaseAsync(false, cancellationToken);

        var dbKey = $"{_modelsInfo.GetCollectionName<SyncSchedule>()}/{ScheduleKey}";

        Logger.LogDebugMessage("Try to get sync schedule for database key: {key}", dbKey.AsArgumentList());

        GetDocumentResponse<SyncSchedule> result = await base.SendRequestAsync(
            client =>
                client.GetDocumentAsync<SyncSchedule>(dbKey),
            true,
            false,
            CallingServiceContext.CreateNewOf<ArangoSyncScheduleStore>(),
            cancellationToken);

        return Logger.ExitMethod(result.Result);
    }

    public async Task<SyncSchedule> SaveScheduleAsync(
        SyncSchedule schedule,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (schedule == null)
        {
            throw new ArgumentNullException(nameof(schedule));
        }

        await _initializer.EnsureDatabaseAsync(false, cancellationToken);

        await base.SendRequestAsync(
            client =>
                client.CreateDocumentAsync(
                    _modelsInfo.GetCollectionName<SyncSchedule>(),
                    schedule.InjectDocumentKey(_ => ScheduleKey, _serializerSettings),
                    new CreateDocumentOptions
                    {
                        Overwrite = true,
                        OverWriteMode = AOverwriteMode.Replace
                    }),
            cancellationToken: cancellationToken);

        return Logger.ExitMethod(schedule);
    }
}
