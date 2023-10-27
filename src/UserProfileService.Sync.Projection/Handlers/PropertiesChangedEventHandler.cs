using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Common.V2.Extensions;
using UserProfileService.EventSourcing.Abstractions;
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
using AVObjectType = Maverick.UserProfileService.Models.EnumModels.ObjectType;

namespace UserProfileService.Sync.Projection.Handlers;

internal class PropertiesChangedEventHandler : SyncBaseEventHandler<PropertiesChanged>
{
    private readonly ISynchronizationReadDestination<GroupSync> _groupReadDestination;
    private readonly ISyncSourceSystemFactory _sourceSystemFactory;
    private readonly IStreamNameResolver _streamNameResolver;
    private readonly SyncConfiguration _syncConfiguration;

    /// <summary>
    ///     Creates a new instance of <see cref="PropertiesChangedEventHandler" />
    /// </summary>
    /// <param name="syncConfigOptions">Options to configure synchronization.</param>
    /// <param name="sourceSystemFactory">Factory to create source system to sync groups to.</param>
    /// <param name="logger">
    ///     <see cref="ILogger{ProfilePropertiesChangedEventHandler}" />
    /// </param>
    /// <param name="streamNameResolver">The resolver that will convert from or to stream names.</param>
    /// <param name="groupReadDestination">Service to read group related entities in destination system.</param>
    /// <param name="stateProfileService">Repository to handle user operation and save projection state.</param>
    public PropertiesChangedEventHandler(
        IOptions<SyncConfiguration> syncConfigOptions,
        ISyncSourceSystemFactory sourceSystemFactory,
        ILogger<PropertiesChangedEventHandler> logger,
        IStreamNameResolver streamNameResolver,
        ISynchronizationReadDestination<GroupSync> groupReadDestination,
        IProfileService stateProfileService) : base(logger, stateProfileService)
    {
        _syncConfiguration = syncConfigOptions.Value;
        _sourceSystemFactory = sourceSystemFactory;
        _groupReadDestination = groupReadDestination;
        _streamNameResolver = streamNameResolver;
    }

    private async Task ApplyChangesOfObjectAsync(PropertiesChanged eventObject, CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        switch (eventObject.ObjectType)
        {
            case ObjectType.User:
            {
                await ApplyChangesOnUserAsync(eventObject.Id, eventObject.Properties, cancellationToken);

                break;
            }
            case ObjectType.Group:
            {
                await ApplyChangesOnGroupAsync(eventObject, eventObject.Properties, cancellationToken);

                break;
            }
            case ObjectType.Organization:
            {
                await ApplyChangesOnOrganizationAsync(eventObject.Id, eventObject.Properties, cancellationToken);

                break;
            }
            case ObjectType.Role:
            {
                await ApplyChangesOnRoleAsync(eventObject.Id, eventObject.Properties, cancellationToken);

                break;
            }
            case ObjectType.Function:
            {
                await ApplyChangesOnFunctionAsync(eventObject.Id, eventObject.Properties, cancellationToken);

                break;
            }
        }

        Logger.EnterMethod();
    }

    private async Task ApplyChangesOnOrganizationMembersAsync(
        PropertiesChanged eventObject,
        StreamedEventHeader eventHeader,
        string relatedProfileId,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        if (eventObject.ObjectType == ObjectType.Organization)
        {
            Logger.LogInfoMessage(
                "Loading organization from sync repository with id: {organizationId}.",
                eventHeader.EventStreamId.AsArgumentList());

            OrganizationSync organization;

            try
            {
                organization = await ProfileService.GetProfileAsync<OrganizationSync>(
                    relatedProfileId,
                    cancellationToken);
            }
            catch (InstanceNotFoundException e)
            {
                Logger.LogErrorMessage(
                    e,
                    "Organization with the id: {id} could not be found",
                    LogHelpers.Arguments(relatedProfileId));

                throw;
            }

            Func<ObjectRelation, bool> organizationRetrievePredicate = objectRelation => objectRelation.ObjectType
                == AVObjectType.Organization
                && objectRelation.MaverickId == eventObject.Id;

            ObjectRelation orgaToUpdate = organization.RelatedObjects?.Where(organizationRetrievePredicate)
                .FirstOrDefault();

            if (orgaToUpdate == null)
            {
                Logger.LogWarnMessage(
                    "Organization with the id can not be updated ",
                    LogHelpers.Arguments(organization.Id));

                return;
            }

            Logger.LogInfoMessage(
                "Updating properties of organization with the id: {id}",
                LogHelpers.Arguments(organization.Id));

            if (Logger.IsEnabledForTrace())
            {
                Logger.LogTraceMessage(
                    "Updating  following properties :{properties} of organization with : {id}",
                    LogHelpers.Arguments(eventObject.Properties.ToLogString(), organization.Id));
            }

            orgaToUpdate.UpdateProperties(eventObject.Properties);
            organization.RelatedObjects.RemoveAll(new Predicate<ObjectRelation>(organizationRetrievePredicate));
            organization.RelatedObjects.Add(orgaToUpdate);

            try
            {
                await ProfileService.UpdateProfileAsync(organization, cancellationToken);
                Logger.ExitMethod();
            }
            catch (Exception e)
            {
                Logger.LogErrorMessage(
                    e,
                    "Error happened by updating organization with the id: {id}",
                    LogHelpers.Arguments(organization.Id));

                throw;
            }
        }
    }

    private async Task ApplyChangesOnUserAsync(
        string userId,
        IDictionary<string, object> changedProperties,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("The Id of the user should be null or white space", nameof(userId));
        }

        if (changedProperties == null)
        {
            throw new ArgumentNullException(nameof(changedProperties));
        }

        Logger.LogInfoMessage(
            "Getting the user from sync repository with id: {id}, that should be updated",
            LogHelpers.Arguments(userId));

        UserSync oldUser;

        try
        {
            oldUser =
                await ProfileService.GetProfileAsync<UserSync>(userId, cancellationToken);
        }
        catch (Exception e)
        {
            Logger.LogErrorMessage(
                e,
                "User profile with id {userId} could not be updated in source system, because it does not exist.",
                LogHelpers.Arguments(userId));

            throw;
        }

        try
        {
            oldUser.UpdateProperties(changedProperties);
        }
        catch (Exception e)
        {
            Logger.LogErrorMessage(
                e,
                "Error happened by updating user with the id: {id}",
                LogHelpers.Arguments(userId));

            throw;
        }

        Logger.LogInfoMessage(
            "Updating the user in sync repository with id: {id}",
            LogHelpers.Arguments(userId));

        try
        {
            await ProfileService.UpdateProfileAsync(oldUser, cancellationToken);
        }
        catch (Exception e)
        {
            Logger.LogErrorMessage(
                e,
                "Error happened by updating user with the id: {id} inside the sync database",
                LogHelpers.Arguments(userId));

            throw;
        }

        Logger.LogInfoMessage(
            "The user in sync repository with id: {id} has been successfully updated",
            LogHelpers.Arguments(userId));

        Logger.ExitMethod();
    }

    private async Task ApplyChangesOnGroupAsync(
        PropertiesChanged eventObject,
        IDictionary<string, object> changedProperties,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        if (eventObject == null)
        {
            throw new ArgumentNullException(nameof(eventObject));
        }

        string groupId = eventObject.Id;

        if (string.IsNullOrWhiteSpace(groupId))
        {
            throw new ArgumentException("The Id of the user should be null or white space", nameof(groupId));
        }

        if (changedProperties == null)
        {
            throw new ArgumentNullException(nameof(changedProperties));
        }

        Logger.LogInfoMessage(
            "Getting the group from sync repository with id: {id}, that should be updated",
            LogHelpers.Arguments(groupId));

        GroupSync oldGroup;

        try
        {
            oldGroup =
                await ProfileService.GetProfileAsync<GroupSync>(groupId, cancellationToken);
        }
        catch (Exception e)
        {
            Logger.LogErrorMessage(
                e,
                "Group with id {groupId} could not be updated in source system, because it does not exist.",
                LogHelpers.Arguments(groupId));

            throw;
        }

        try
        {
            oldGroup.UpdateProperties(changedProperties);
        }
        catch (Exception e)
        {
            Logger.LogErrorMessage(
                e,
                "Error happened by updating group with id: {groupId}.",
                LogHelpers.Arguments(groupId));

            throw;
        }

        Logger.LogInfoMessage(
            "Updating the group in sync repository with id: {id}",
            LogHelpers.Arguments(groupId));

        try
        {
            await ProfileService.UpdateProfileAsync(oldGroup, cancellationToken);
        }
        catch (Exception e)
        {
            Logger.LogErrorMessage(
                e,
                "Error happened by updating group with id: {groupId}.",
                LogHelpers.Arguments(groupId));

            throw;
        }

        foreach (KeyValuePair<string, SourceSystemConfiguration> sourceSystemConfiguration in
                 _syncConfiguration
                     .SourceConfiguration.Systems)
        {
            string sourceSystemKey = sourceSystemConfiguration.Key;

            if (eventObject.MetaData?.Initiator?.Id != SyncConstants.System.InitiatorId)
            {
                if (sourceSystemConfiguration.Value.Destination.TryGetValue(
                        SyncConstants.SagaStep.GroupStep,
                        out SynchronizationOperations groupSynchronizationOperations))
                {
                    if (groupSynchronizationOperations.Operations.HasFlag(SynchronizationOperation.Update))
                    {
                        GroupSync groupSync =
                            await _groupReadDestination.GetObjectAsync(groupId, cancellationToken);

                        if (groupSync == null)
                        {
                            Logger.LogErrorMessage(
                                null,
                                "Group with id: {groupId} could not be updated in source system, because it does not exist in destination system.",
                                LogHelpers.Arguments(groupId));

                            return;
                        }

                        Logger.LogDebugMessage(
                            "Found profile with id {profileId} in destination system.",
                            LogHelpers.Arguments(groupId));

                        // Actually, the properties should have already been updated by the previous handler.
                        // However, there may be delays in cluster operation.
                        // Therefore, it is ensured that the properties are up to date.
                        groupSync.UpdateProperties(changedProperties);

                        KeyProperties externalId =
                            groupSync.ExternalIds.FirstOrDefaultUnconverted(sourceSystemKey);

                        Logger.LogInfoMessage(
                            "Create group with id '{Id}' in sync profile repository.",
                            LogHelpers.Arguments(groupId));

                        try
                        {
                            await ProfileService.UpdateProfileAsync(
                                groupSync,
                                cancellationToken);
                        }
                        catch (Exception)
                        {
                            Logger.LogInfoMessage(
                                "Expected Error happened by updating group with id: {groupId}.",
                                LogHelpers.Arguments(groupId));
                        }

                        if (externalId == null)
                        {
                            Logger.LogErrorMessage(
                                null,
                                "No matching identifier was found for the source system ('{sourceSystemKey}'). Internal entity is is '{@event.Payload.Id}'.",
                                LogHelpers.Arguments(sourceSystemKey, groupId));

                            continue;
                        }

                        ISynchronizationSourceSystem<GroupSync> groupSourceSystem =
                            _sourceSystemFactory.Create<GroupSync>(_syncConfiguration, sourceSystemKey);

                        Logger.LogInfoMessage(
                            "Update profile with id '{payloadId}' in source system with key '{sourceSystemKey}'.",
                            LogHelpers.Arguments(groupId, sourceSystemKey));

                        await groupSourceSystem.UpdateEntity(externalId.Id, groupSync, cancellationToken);
                    }
                }
            }
        }

        Logger.ExitMethod();
    }

    private async Task ApplyChangesOnOrganizationAsync(
        string organizationId,
        IDictionary<string, object> changedProperties,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        Logger.LogInfoMessage(
            "Getting the organization from sync repository with id: {id}, that should be updated",
            LogHelpers.Arguments(organizationId));

        OrganizationSync oldOrganization;

        try
        {
            oldOrganization =
                await ProfileService.GetProfileAsync<OrganizationSync>(
                    organizationId,
                    cancellationToken);
        }
        catch (Exception e)
        {
            Logger.LogErrorMessage(
                e,
                "Organization with id {orgaId} could not be updated in source system, because it does not exist.",
                LogHelpers.Arguments(organizationId));

            throw;
        }

        oldOrganization.UpdateProperties(changedProperties);

        Logger.LogInfoMessage(
            "Updating the organization in sync repository with id: {id}",
            LogHelpers.Arguments(organizationId));

        try
        {
            await ProfileService.UpdateProfileAsync(
                oldOrganization,
                cancellationToken);
        }
        catch (Exception e)
        {
            Logger.LogErrorMessage(
                e,
                "Error happened by updating organization with id: {id} in sync repository",
                LogHelpers.Arguments(organizationId));

            throw;
        }

        Logger.LogInfoMessage(
            "The organization in sync repository with id: {id} has been successfully updated",
            LogHelpers.Arguments(organizationId));

        Logger.ExitMethod();
    }

    private async Task ApplyChangesOnRoleAsync(
        string roleId,
        IDictionary<string, object> changedProperties,
        CancellationToken cancellationToken)
    {
        Logger.LogInfoMessage(
            "Getting the role from sync repository with id: {id}, that should be updated",
            LogHelpers.Arguments(roleId));

        RoleSync roleToUpdate;

        try
        {
            roleToUpdate =
                await ProfileService.GetRoleAsync(roleId, cancellationToken);
        }
        catch (Exception e)
        {
            Logger.LogErrorMessage(
                e,
                "Error happened by loading the role from sync repository with id: {id}, that should be updated",
                LogHelpers.Arguments(roleId));

            throw;
        }

        if (roleToUpdate == null)
        {
            Logger.LogErrorMessage(
                null,
                "Role with id {userId} could not be updated in source system, because it does not exist.",
                LogHelpers.Arguments(roleId));

            Logger.ExitMethod();

            return;
        }

        roleToUpdate.UpdateProperties(changedProperties);

        Logger.LogInfoMessage(
            "Updating the role in sync repository with id: {id}",
            LogHelpers.Arguments(roleId));

        try
        {
            await ProfileService.UpdateRoleAsync(roleToUpdate, cancellationToken);
        }
        catch (Exception e)
        {
            Logger.LogErrorMessage(
                e,
                "Error happened by updating role with the id: {id}",
                LogHelpers.Arguments(roleToUpdate.Id));

            throw;
        }

        Logger.LogInfoMessage(
            "The role in sync repository with id: {id} has been successfully updated",
            LogHelpers.Arguments(roleId));

        Logger.ExitMethod();
    }

    private async Task ApplyChangesOnFunctionAsync(
        string functionId,
        IDictionary<string, object> changedProperties,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        if (string.IsNullOrWhiteSpace(functionId))
        {
            throw new ArgumentNullException(nameof(functionId));
        }

        if (changedProperties == null)
        {
            throw new ArgumentException("Changed properties can not be null empty", nameof(changedProperties));
        }

        Logger.LogInfoMessage(
            "Getting the function from sync repository with id: {id}, that should be updated",
            LogHelpers.Arguments(functionId));

        FunctionSync functionToUpdate;

        try
        {
            functionToUpdate =
                await ProfileService.GetFunctionAsync(functionId, cancellationToken);
        }
        catch (Exception e)
        {
            Logger.LogErrorMessage(
                e,
                "Error happened by loading the function from sync repository with id: {id}, that should be updated",
                LogHelpers.Arguments(functionId));

            throw;
        }

        functionToUpdate.UpdateProperties(changedProperties);

        Logger.LogInfoMessage(
            "Updating the function in sync repository with id: {id}",
            LogHelpers.Arguments(functionId));

        try
        {
            await ProfileService.UpdateFunctionAsync(functionToUpdate, cancellationToken);
        }
        catch (Exception e)
        {
            Logger.LogErrorMessage(
                e,
                "Error happened by updating function with the id: {id}",
                LogHelpers.Arguments(functionToUpdate.Id));

            throw;
        }

        Logger.LogInfoMessage(
            "The function in sync repository with id: {id} has been successfully updated",
            LogHelpers.Arguments(functionId));

        Logger.ExitMethod();
    }

    protected override async Task HandleInternalAsync(
        PropertiesChanged eventObject,
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
            throw new InvalidDomainEventException(
                "Could update changed properties: Resource Id is missing.",
                eventObject);
        }

        if (eventObject.Properties == null || eventObject.Properties.Count == 0)
        {
            Logger.LogWarnMessage(
                "Could not update object of type: {objectType} with the id : {id}, properties to update are null or empty ",
                LogHelpers.Arguments(eventObject.ObjectType.ToString().AsArgumentList(), eventObject.Id));
        }

        Logger.LogTraceMessage(
            "Extracting object ident from event stream id {eventStreamId}.",
            eventHeader.EventStreamId.AsArgumentList());

        ObjectIdent relatedObjectIdent =
            _streamNameResolver.GetObjectIdentUsingStreamName(eventHeader.EventStreamId);

        if (relatedObjectIdent == null)
        {
            throw new ArgumentNullException(nameof(relatedObjectIdent));
        }

        if (string.IsNullOrWhiteSpace(relatedObjectIdent.Id))
        {
            throw new ArgumentException(
                "Id of related object ident should not be null or whitespace",
                nameof(relatedObjectIdent.Id));
        }

        //For better readability, the switch case statement has been omitted at this point.
        if (eventObject.RelatedContext == PropertiesChangedContext.Self)
        {
            await ApplyChangesOfObjectAsync(eventObject, cancellationToken);
        }
        else if (eventObject.RelatedContext is PropertiesChangedContext.Members or PropertiesChangedContext.MemberOf)
        {
            await ApplyChangesOnOrganizationMembersAsync(
                eventObject,
                eventHeader,
                relatedObjectIdent.Id,
                cancellationToken);
        }
    }
}
