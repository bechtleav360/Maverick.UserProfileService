using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Extensions;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Sync.Abstraction.Configurations;
using UserProfileService.Sync.Abstraction.Configurations.Implementations;
using UserProfileService.Sync.Abstraction.Contracts;
using UserProfileService.Sync.Abstraction.Factories;
using UserProfileService.Sync.Abstraction.Models;
using UserProfileService.Sync.Abstraction.Models.Entities;
using UserProfileService.Sync.Abstraction.Systems;
using UserProfileService.Sync.Projection.Abstractions;

namespace UserProfileService.Sync.Projection.Handlers;

internal class EntityDeletedEventHandler : SyncBaseEventHandler<EntityDeleted>
{
    private readonly ISyncSourceSystemFactory _sourceSystemFactory;
    private readonly IStreamNameResolver _streamNameResolver;
    private readonly SyncConfiguration _syncConfiguration;

    public EntityDeletedEventHandler(
        ILogger<EntityDeletedEventHandler> logger,
        IOptions<SyncConfiguration> syncConfigOptions,
        ISyncSourceSystemFactory sourceSystemFactory,
        IStreamNameResolver streamNameResolver,
        IProfileService profileService) : base(logger, profileService)
    {
        _streamNameResolver = streamNameResolver;
        _syncConfiguration = syncConfigOptions.Value;
        _sourceSystemFactory = sourceSystemFactory;
    }

    private async Task DeleteUserOrOrganizationAsync(
        ObjectIdent relatedObjectIdent,
        string entityId,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        if (relatedObjectIdent == null)
        {
            throw new ArgumentNullException(nameof(relatedObjectIdent));
        }

        if (string.IsNullOrWhiteSpace(entityId))
        {
            throw new ArgumentException("Entity Id can't be null", nameof(entityId));
        }

        Logger.LogInfoMessage(
            "Deleting profile of type: {type} with the id: {profileId} in the sync database",
            LogHelpers.Arguments(relatedObjectIdent.Type.ToString(), entityId));

        try
        {
            await ProfileService.DeleteProfileAsync<ISyncProfile>(
                entityId,
                cancellationToken);

            Logger.LogInfoMessage(
                "The profile with the id: {profileId}  has been deleted in the sync database",
                LogHelpers.Arguments(entityId));
        }
        catch (Exception e)
        {
            Logger.LogErrorMessage(
                e,
                "Error happened by deleting profile with the id: {profileId} in the sync database",
                LogHelpers.Arguments(entityId));

            throw;
        }

        Logger.ExitMethod();
    }

    private async Task DeleteGroupAsync(EntityDeleted eventObject, CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        if (eventObject == null)
        {
            throw new ArgumentNullException(nameof(eventObject));
        }

        if (string.IsNullOrWhiteSpace(eventObject.Id))
        {
            throw new ArgumentException("The entity Id should not be null", nameof(eventObject.Id));
        }

        GroupSync group;

        try
        {
            group = await ProfileService.GetProfileAsync<GroupSync>(eventObject.Id, cancellationToken);
        }
        catch (Exception e)
        {
            Logger.LogErrorMessage(
                e,
                "Group with id {groupId} could not be deleted in source system, because it does not exist in destination system.",
                LogHelpers.Arguments(eventObject.Id));

            throw;
        }

        Logger.LogInfoMessage(
            "Delete group with id: {Id} in sync profile repo.",
            LogHelpers.Arguments(eventObject.Id));

        try
        {
            await ProfileService.DeleteProfileAsync<GroupSync>(eventObject.Id, cancellationToken);
        }
        catch (Exception e)
        {
            Logger.LogErrorMessage(
                e,
                "Error happened by deleting of group with id {groupId}.",
                LogHelpers.Arguments(eventObject.Id));

            throw;
        }

        // If several systems are configured per entity,
        // the source of the entity must be checked here. 
        if (eventObject.MetaData.Initiator?.Id == SyncConstants.System.InitiatorId)
        {
            Logger.LogDebugMessage(
                "Initiator of the event is the same as the current system. The event can be ignored after the deletion of the group in sync repository.",
                LogHelpers.Arguments());

            Logger.ExitMethod();

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
                        _sourceSystemFactory.Create<GroupSync>(_syncConfiguration, sourceSystemKey);

                    Logger.LogDebugMessage(
                        "Found profile with id {profileId} in destination system.",
                        LogHelpers.Arguments(eventObject.Id));

                    ExternalIdentifier externalId =
                        group?.ExternalIds?.FirstOrDefaultUnconverted(sourceSystemKey);

                    if (externalId == null)
                    {
                        Logger.LogErrorMessage(
                            null,
                            "No matching identifier was found for the source system ('{sourceSystemKey}'). Internal entity is '{eventPayloadId}'.",
                            LogHelpers.Arguments(sourceSystemKey, eventObject.Id));

                        continue;
                    }

                    Logger.LogInfoMessage(
                        "Delete group with id '{@event.Payload.Id}' and external id '{externalId}' in source system with key '{sourceSystemKey}'.",
                        LogHelpers.Arguments(eventObject.Id, externalId.Id, sourceSystemKey));

                    await groupSourceSystem.DeleteEntity(externalId.Id, cancellationToken);
                }
            }
        }

        Logger.ExitMethod();
    }

    private async Task DeleteFunctionAsync(string functionId, CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        if (string.IsNullOrWhiteSpace(functionId))
        {
            throw new ArgumentException("The function id should not be null or whitespace", nameof(functionId));
        }

        Logger.LogInfoMessage(
            "Deleting function with the id: {functionId} in the sync database",
            LogHelpers.Arguments(functionId));

        try
        {
            await ProfileService.DeleteFunctionAsync(
                functionId,
                cancellationToken);

            Logger.LogInfoMessage(
                "The function with the id: {functionId}  has been deleted in the sync database",
                LogHelpers.Arguments(functionId));
        }
        catch (Exception e)
        {
            Logger.LogErrorMessage(
                e,
                "Error happened by deleting function with the id: {functionId} in the sync database",
                LogHelpers.Arguments(functionId));

            throw;
        }
    }

    private async Task DeleteRoleAsync(string roleId, CancellationToken cancellationToken)
    {
        Logger.LogInfoMessage(
            "Deleting role with the id: {roleId} in the sync database",
            LogHelpers.Arguments(roleId));

        try
        {
            await ProfileService.DeleteRoleAsync(
                roleId,
                cancellationToken);

            Logger.LogInfoMessage(
                "The role with the id: {roleId}  has been deleted in the sync database",
                LogHelpers.Arguments(roleId));
        }
        catch (Exception e)
        {
            Logger.LogErrorMessage(
                e,
                "Error happened by deleting role with the id: {roleId} in the sync database",
                LogHelpers.Arguments(roleId));

            throw;
        }
    }

    /// <inheritdoc />
    protected override async Task HandleInternalAsync(
        EntityDeleted eventObject,
        StreamedEventHeader eventHeader,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (eventObject == null)
        {
            throw new ArgumentNullException(nameof(eventObject));
        }

        if (string.IsNullOrWhiteSpace(eventObject.Id))
        {
            throw new ArgumentException("The Id should not be empty or whitespace", nameof(eventObject));
        }

        Logger.LogTraceMessage(
            "Extracting object ident from event stream id {eventStreamId}.",
            eventHeader.EventStreamId.AsArgumentList());

        ObjectIdent relatedObjectIdent =
            _streamNameResolver.GetObjectIdentUsingStreamName(eventHeader.EventStreamId);

        switch (relatedObjectIdent.Type)
        {
            case ObjectType.User:
            case ObjectType.Organization:

                await DeleteUserOrOrganizationAsync(relatedObjectIdent, relatedObjectIdent.Id, cancellationToken);

                break;

            case ObjectType.Group:

                await DeleteGroupAsync(eventObject, cancellationToken);

                break;

            case ObjectType.Function:

                await DeleteFunctionAsync(eventObject.Id, cancellationToken);

                break;

            case ObjectType.Role:

                await DeleteRoleAsync(eventObject.Id, cancellationToken);

                break;
            case ObjectType.Unknown:
            case ObjectType.Profile:
            case ObjectType.Tag:
                break;
        }

        Logger.ExitMethod();
    }
}
