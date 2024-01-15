using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Sync.Abstraction;
using UserProfileService.Sync.Abstraction.Configurations;
using UserProfileService.Sync.Abstraction.Configurations.Implementations;
using UserProfileService.Sync.Abstraction.Contracts;
using UserProfileService.Sync.Abstraction.Converters;
using UserProfileService.Sync.Abstraction.Factories;
using UserProfileService.Sync.Abstraction.Models;
using UserProfileService.Sync.Abstraction.Models.Entities;
using UserProfileService.Sync.Abstraction.Systems;
using UserProfileService.Sync.Projection.Abstractions;

//using UserProfileService.Sync.Common.Converter;

namespace UserProfileService.Sync.Projection.Handlers;

/// <summary>
///     This handler is used to process <see cref="GroupCreated" />.
/// </summary>
internal class GroupCreatedEventHandler : SyncBaseEventHandler<GroupCreated>
{
    private readonly IConverterFactory<ISyncModel> _converterFactory;
    private readonly ISynchronizationWriteDestination<GroupSync> _groupWriteDestination;
    private readonly ILogger<GroupCreatedEventHandler> _logger;
    private readonly IMapper _mapper;
    private readonly ISyncSourceSystemFactory _sourceSystemFactory;
    private readonly SyncConfiguration _syncConfiguration;

    /// <summary>
    ///     Creates a new instance of <see cref="GroupCreatedEventHandler" />
    /// </summary>
    /// <param name="syncConfigOptions"> Options to configure synchronization. </param>
    /// <param name="sourceSystemFactory"> Factory to create source system to sync groups to. </param>
    /// <param name="mapper"> Mapper to convert objects. </param>
    /// <param name="logger">
    ///     <see cref="ILogger{GroupCreatedEventHandler}" />
    /// </param>
    /// <param name="groupWriteDestination"> Write service to persist groups in destination system. </param>
    /// <param name="stateProfileService">Service to handle projection state, users and groups.</param>
    /// <param name="converterFactory">
    ///     A Factory used to generate converter which convert external Ids of entities if
    ///     necessary.
    /// </param>
    public GroupCreatedEventHandler(
        IOptions<SyncConfiguration> syncConfigOptions,
        ISyncSourceSystemFactory sourceSystemFactory,
        IMapper mapper,
        ILogger<GroupCreatedEventHandler> logger,
        ISynchronizationWriteDestination<GroupSync> groupWriteDestination,
        IProfileService stateProfileService,
        IConverterFactory<ISyncModel> converterFactory) : base(logger, stateProfileService)
    {
        _syncConfiguration = syncConfigOptions.Value;
        _sourceSystemFactory = sourceSystemFactory;
        _logger = logger;
        _mapper = mapper;
        _groupWriteDestination = groupWriteDestination;
        _converterFactory = converterFactory;
    }

    private async Task<GroupSync> CreateGroupInRepoAsync(GroupCreated group, CancellationToken cancellationToken)
    {
        _logger.LogInfoMessage(
            "Create group with id '{Id}' in sync profile repository.",
            LogHelpers.Arguments(group.Id));

        var groupCreated = _mapper.Map<GroupSync>(group);

        return await ProfileService.CreateProfileAsync(groupCreated, cancellationToken);
    }

    protected override async Task HandleInternalAsync(
        GroupCreated eventObject,
        StreamedEventHeader eventHeader,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (eventObject == null)
        {
            _logger.LogWarnMessage(
                "The variable with the name eventObject is null.",
                LogHelpers.Arguments());

            throw new ArgumentNullException($"The variable with the name {nameof(eventObject)} is null.");
        }

        _logger.LogInfoMessage(
            "Create group with id '{Id}' in sync profile repository.",
            LogHelpers.Arguments(eventObject.Id));

        GroupSync createdGroup = await CreateGroupInRepoAsync(eventObject, cancellationToken);

        // If several systems are configured per entity,
        // the source of the entity must be checked here. 
        if (eventObject.MetaData.Initiator?.Id == SyncConstants.System.InitiatorId)
        {
            _logger.LogDebugMessage(
                "Initiator of the event is the same as the current system. The event can be ignored after stored the group in the sync repository.",
                LogHelpers.Arguments());

            return;
        }

        foreach (KeyValuePair<string, SourceSystemConfiguration> sourceSystemConfiguration in _syncConfiguration
                     .SourceConfiguration.Systems)
        {
            string sourceSystemKey = sourceSystemConfiguration.Key;

            if (sourceSystemConfiguration.Value.Destination.TryGetValue(
                    SyncConstants.SagaStep.GroupStep,
                    out SynchronizationOperations groupSynchronizationOperations))
            {
                if (groupSynchronizationOperations.Operations.HasFlag(SynchronizationOperation.Add))
                {
                    ISynchronizationSourceSystem<GroupSync> groupSourceSystem =
                        _sourceSystemFactory.Create<GroupSync>(sourceSystemKey);

                    var groupSync = _mapper.Map<GroupSync>(eventObject);

                    // if the group was created by the system, the event can be ignored.
                    if (eventObject.Source == sourceSystemKey)
                    {
                        _logger.LogDebugMessage(
                            "Group with id {groupSync.Id} was created by the source system, so the event will be skipped.",
                            LogHelpers.Arguments(groupSync.Id));

                        continue;
                    }

                    _logger.LogInfoMessage(
                        "Create group with id '{@event.Payload.Id}' in source system with key '{sourceSystemKey}'.",
                        LogHelpers.Arguments(eventObject.Id, sourceSystemKey));

                    GroupSync createdSourceGroup =
                        await groupSourceSystem.CreateEntity(groupSync, cancellationToken);

                    groupSync.ExternalIds = createdSourceGroup.ExternalIds;

                    IConverter<ISyncModel> converter = _converterFactory.CreateConverter(
                        sourceSystemConfiguration.Value,
                        SyncConstants.SagaStep.GroupStep);

                    if (converter != null)
                    {
                        _logger.LogInfoMessage(
                            "Start converting external ids of entities with the converter.",
                            LogHelpers.Arguments());

                        groupSync = (GroupSync)converter.Convert(groupSync);
                    }

                    var modifiedProperties = new HashSet<string>
                    {
                        nameof(GroupSync.ExternalIds)
                    };

                    _logger.LogInfoMessage(
                        "Update group with id '{Id}' in sync profile repository.",
                        LogHelpers.Arguments(groupSync.Id));

                    await ProfileService.UpdateProfileAsync(createdGroup, cancellationToken);

                    _logger.LogDebugMessage(
                        "Update external id '{externalIds}' for group with id '{groupSyncId}'.",
                        LogHelpers.Arguments(string.Join(",", groupSync.ExternalIds), groupSync.Id));

                    await _groupWriteDestination.UpdateObjectAsync(
                        Guid.NewGuid(),
                        groupSync,
                        modifiedProperties,
                        cancellationToken);
                }
            }
        }

        _logger.ExitMethod();
    }
}
