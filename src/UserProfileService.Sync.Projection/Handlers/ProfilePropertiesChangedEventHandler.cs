using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Extensions;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Sync.Abstraction;
using UserProfileService.Sync.Abstraction.Configurations;
using UserProfileService.Sync.Abstraction.Configurations.Implementations;
using UserProfileService.Sync.Abstraction.Contracts;
using UserProfileService.Sync.Abstraction.Factories;
using UserProfileService.Sync.Abstraction.Models;
using UserProfileService.Sync.Abstraction.Models.Entities;
using UserProfileService.Sync.Abstraction.Systems;
using UserProfileService.Sync.Common.Extensions;
using UserProfileService.Sync.Projection.Abstractions;
using InitiatorType = Maverick.UserProfileService.AggregateEvents.Common.InitiatorType;

namespace UserProfileService.Sync.Projection.Handlers;

/// <summary>
///     This handler is used to process <see cref="ProfilePropertiesChangedEvent" />.
/// </summary>
internal class ProfilePropertiesChangedEventHandler : SyncBaseEventHandler<PropertiesChanged>
{
    private readonly ISynchronizationReadDestination<GroupSync> _groupReadDestination;
    private readonly ISyncSourceSystemFactory _sourceSystemFactory;
    private readonly SyncConfiguration _syncConfiguration;

    /// <summary>
    ///     Creates a new instance of <see cref="ProfilePropertiesChangedEventHandler" />
    /// </summary>
    /// <param name="syncConfigOptions">Options to configure synchronization.</param>
    /// <param name="sourceSystemFactory">Factory to create source system to sync groups to.</param>
    /// <param name="logger">
    ///     <see cref="ILogger{ProfilePropertiesChangedEventHandler}" />
    /// </param>
    /// <param name="groupReadDestination">Service to read group related entities in destination system.</param>
    /// <param name="stateProfileService">Repository to handle user operation and save projection state.</param>
    public ProfilePropertiesChangedEventHandler(
        IOptions<SyncConfiguration> syncConfigOptions,
        ISyncSourceSystemFactory sourceSystemFactory,
        ILogger<ProfilePropertiesChangedEventHandler> logger,
        ISynchronizationReadDestination<GroupSync> groupReadDestination,
        IProfileService stateProfileService) : base(logger, stateProfileService)
    {
        _syncConfiguration = syncConfigOptions.Value;
        _sourceSystemFactory = sourceSystemFactory;
        _groupReadDestination = groupReadDestination;
    }

    /// <inheritdoc />
    protected override async Task HandleInternalAsync(
        PropertiesChanged eventObject,
        StreamedEventHeader eventHeader,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (eventObject == null)
        {
            Logger.LogWarnMessage(
                "The variable with the name {nameof(@event)} is null.",
                LogHelpers.Arguments(nameof(eventObject)));

            throw new ArgumentNullException(
                nameof(eventObject),
                $"The variable with the name {nameof(eventObject)} is null.");
        }

        foreach (KeyValuePair<string, SourceSystemConfiguration> sourceSystemConfiguration in _syncConfiguration
                     .SourceConfiguration.Systems)
        {
            string sourceSystemKey = sourceSystemConfiguration.Key;

            if (eventObject.ObjectType == ObjectType.Group
                && eventObject.MetaData?.Initiator.Type == InitiatorType.System)
            {
                if (sourceSystemConfiguration.Value.Destination.TryGetValue(
                        SyncConstants.SagaStep.GroupStep,
                        out SynchronizationOperations groupSynchronizationOperations))
                {
                    if (groupSynchronizationOperations.Operations.HasFlag(SynchronizationOperation.Update))
                    {
                        GroupSync groupSync =
                            await _groupReadDestination.GetObjectAsync(eventObject.Id, cancellationToken);

                        if (groupSync == null)
                        {
                            Logger.LogErrorMessage(
                                null,
                                "Group with id {@event.Payload.Id} could not be updated in source system, because it does not exist in destination system.",
                                LogHelpers.Arguments(eventObject.Id));

                            return;
                        }

                        Logger.LogDebugMessage(
                            "Found profile with id {profileId} in destination system.",
                            LogHelpers.Arguments(eventObject.Id));

                        // Actually, the properties should have already been updated by the previous handler.
                        // However, there may be delays in cluster operation.
                        // Therefore, it is ensured that the properties are up to date.
                        groupSync.UpdateProperties(eventObject.Properties);

                        KeyProperties externalId = groupSync.ExternalIds.FirstOrDefaultUnconverted(sourceSystemKey);

                        if (externalId == null)
                        {
                            Logger.LogErrorMessage(
                                null,
                                "No matching identifier was found for the source system ('{sourceSystemKey}'). Internal entity is is '{@event.Payload.Id}'.",
                                LogHelpers.Arguments(sourceSystemKey, eventObject.Id));

                            continue;
                        }

                        ISynchronizationSourceSystem<GroupSync> groupSourceSystem =
                            _sourceSystemFactory.Create<GroupSync>(_syncConfiguration, sourceSystemKey);

                        Logger.LogInfoMessage(
                            "Update profile with id '{payloadId}' in source system with key '{sourceSystemKey}'.",
                            LogHelpers.Arguments(eventObject.Id, sourceSystemKey));

                        await groupSourceSystem.UpdateEntity(externalId.Id, groupSync, cancellationToken);
                    }
                }
            }
        }

        Logger.ExitMethod();
    }
}
