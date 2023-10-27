using System;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Contracts;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Sync.Abstraction.Extensions;
using UserProfileService.Sync.Abstraction.Models.Entities;
using UserProfileService.Sync.Projection.Abstractions;
using ObjectType = Maverick.UserProfileService.Models.EnumModels.ObjectType;

namespace UserProfileService.Sync.Projection.Handlers;

internal class ContainerDeletedEventHandler : SyncBaseEventHandler<ContainerDeleted>
{
    private readonly IStreamNameResolver _streamNameResolver;

    /// <summary>
    ///     Creates an instance of <see cref="ContainerDeletedEventHandler" />
    /// </summary>
    /// <param name="logger">   The logger</param>
    /// <param name="profileService">   The service used to perfom operations on sync database</param>
    /// <param name="streamNameResolver">
    ///     Object used to resolve object information through the stream name
    ///     <see cref="IStreamNameResolver" />
    /// </param>
    public ContainerDeletedEventHandler(
        ILogger<ContainerDeletedEventHandler> logger,
        IProfileService profileService,
        IStreamNameResolver streamNameResolver) : base(logger, profileService)
    {
        _streamNameResolver = streamNameResolver;
    }

    /// <summary>
    ///     Attempts to delete the relation link of an organization to it's parent organization in parameter
    ///     <paramref name="containerToBeDeleted" />
    /// </summary>
    /// <remarks>
    ///     This won't change the object to be deleted itself. It will remove the relation information in the child object
    ///     itself.<br />
    ///     If the child object could no tbe found, it will return a boolean <c>false</c>.<br />
    ///     If an error occurs, it will throw the transaction instead of returning a boolean value.
    /// </remarks>
    private async Task<bool> TryDeleteOrganizationRelationAsync(
        ContainerDeleted containerToBeDeleted,
        ObjectIdent relatedObjectIdent,
        CancellationToken cancellationToken)
    {
        try
        {
            var organization = await ProfileService.GetProfileAsync<OrganizationSync>(
                relatedObjectIdent.Id,
                cancellationToken);

            if (organization == null)
            {
                return false;
            }

            // update the instance in-memory
            organization.DeleteObjectRelation(containerToBeDeleted.ContainerId, AssignmentType.ParentsToChild);

            // update the entity in backend
            await ProfileService.UpdateProfileAsync(organization, cancellationToken);
        }
        catch (Exception e)
        {
            Logger.LogErrorMessage(
                e,
                "Error happened by deleting container (organization) with the id: {profileId} from the organization (id: {organizationId}) in the sync database",
                LogHelpers.Arguments(containerToBeDeleted.ContainerId, relatedObjectIdent.Id));

            throw;
        }

        return true;
    }

    /// <inheritdoc />
    protected override async Task HandleInternalAsync(
        ContainerDeleted eventObject,
        StreamedEventHeader eventHeader,
        CancellationToken cancellationToken = default)
    {
        if (eventObject == null)
        {
            throw new ArgumentNullException(nameof(eventObject));
        }

        if (string.IsNullOrWhiteSpace(eventObject.ContainerId))
        {
            throw new InvalidDomainEventException(
                "Could not remove memberOf: Resource containerId is missing.",
                eventObject);
        }

        Logger.LogTraceMessage(
            "Extracting object ident from event stream id {eventStreamId}.",
            eventHeader.EventStreamId.AsArgumentList());

        ObjectIdent relatedObjectIdent =
            _streamNameResolver.GetObjectIdentUsingStreamName(eventHeader.EventStreamId);

        if (relatedObjectIdent.Type == ObjectType.Organization
            && eventObject.ContainerType == ContainerType.Organization
            && relatedObjectIdent.Id == eventObject.MemberId)
        {
            if (string.IsNullOrWhiteSpace(relatedObjectIdent.Id))
            {
                throw new ArgumentException(
                    "The related object Id should not be null or whitespace",
                    nameof(relatedObjectIdent.Id));
            }

            Logger.LogInfoMessage(
                "Deleting container (organization) with the id: {profileId} from the organization (Id = relatedObjectId) in the sync database",
                LogHelpers.Arguments(eventObject.ContainerId, relatedObjectIdent.Id));

            if (!await TryDeleteOrganizationRelationAsync(eventObject, relatedObjectIdent, cancellationToken))
            {
                throw new InstanceNotFoundException(
                    WellKnownErrorCodes.ChildNotFound,
                    $"Cannot find child organization '{eventObject.MemberId}' of deleted parent '{eventObject.ContainerId}'");
            }

            Logger.LogInfoMessage(
                "The container (organization) with the id: {profileId} has been successfully deleted from the organization (id: {organizationId})",
                LogHelpers.Arguments(eventObject.ContainerId, relatedObjectIdent.Id));
        }
    }
}
