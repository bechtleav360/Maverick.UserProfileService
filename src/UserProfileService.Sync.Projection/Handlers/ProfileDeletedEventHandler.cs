using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Extensions;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Sync.Abstraction.Configurations;
using UserProfileService.Sync.Abstraction.Configurations.Implementations;
using UserProfileService.Sync.Abstraction.Contracts;
using UserProfileService.Sync.Abstraction.Factories;
using UserProfileService.Sync.Abstraction.Models.Entities;
using UserProfileService.Sync.Abstraction.Systems;
using UserProfileService.Sync.Projection.Abstractions;

namespace UserProfileService.Sync.Projection.Handlers;

/// <summary>
///     This handler is used to process <see cref="ProfileDeletedEvent" />.
/// </summary>
internal class ProfileDeletedEventHandler : SyncBaseEventHandler<ProfileDeletedEvent>
{
    private readonly ISyncSourceSystemFactory _sourceSystemFactory;
    private readonly SyncConfiguration _syncConfiguration;

    /// <summary>
    ///     Creates a new instance of <see cref="ProfileDeletedEventHandler" />
    /// </summary>
    /// <param name="syncConfigOptions">Options to configure synchronization.</param>
    /// <param name="sourceSystemFactory">Factory to create source system to sync groups to.</param>
    /// <param name="logger">
    ///     <see cref="ILogger{ProfileDeletedEventHandler}" />
    /// </param>
    /// <param name="stateProfileService">Repository used to handle user operations and projection state.</param>
    public ProfileDeletedEventHandler(
        IOptions<SyncConfiguration> syncConfigOptions,
        ISyncSourceSystemFactory sourceSystemFactory,
        ILogger<ProfileDeletedEventHandler> logger,
        IProfileService stateProfileService) : base(logger, stateProfileService)
    {
        _syncConfiguration = syncConfigOptions.Value;
        _sourceSystemFactory = sourceSystemFactory;
    }

    /// <inheritdoc />
    protected override async Task HandleInternalAsync(
        ProfileDeletedEvent eventObject,
        StreamedEventHeader eventHeader,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (eventObject == null)
        {
            Logger.LogWarnMessage(
                $"The variable with the name {nameof(eventObject)} is null.",
                LogHelpers.Arguments(nameof(eventObject)));

            throw new ArgumentNullException($"The variable with the name {nameof(eventObject)} is null.");
        }

        // If several systems are configured per entity,
        // the source of the entity must be checked here. 
        if (eventObject.MetaData.Initiator?.Id == SyncConstants.System.InitiatorId)
        {
            Logger.LogDebugMessage(
                "Initiator of the event is the same as the current system. The event can be ignored.",
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
                if (groupSynchronizationOperations.Operations.HasFlag(SynchronizationOperation.Delete))
                {
                    ISynchronizationSourceSystem<GroupSync> groupSourceSystem =
                        _sourceSystemFactory.Create<GroupSync>(sourceSystemKey);

                    ExternalIdentifier externalId =
                        eventObject.Payload.ExternalIds.FirstOrDefaultUnconverted(sourceSystemKey);

                    if (externalId == null)
                    {
                        Logger.LogErrorMessage(
                            null,
                            "No matching identifier was found for the source system ('{sourceSystemKey}'). Internal entity is is '{@event.Payload.Id}'.",
                            LogHelpers.Arguments(sourceSystemKey, eventObject.Payload.Id));

                        continue;
                    }

                    Logger.LogInfoMessage(
                        "Delete group with id '{@event.Payload.Id}' and external id '{externalId.Id}' in source system with key '{sourceSystemKey}'.",
                        LogHelpers.Arguments(eventObject.Payload.Id, externalId.Id, sourceSystemKey));

                    await groupSourceSystem.DeleteEntity(externalId.Id, cancellationToken);
                }
            }
        }

        Logger.ExitMethod();
    }
}
