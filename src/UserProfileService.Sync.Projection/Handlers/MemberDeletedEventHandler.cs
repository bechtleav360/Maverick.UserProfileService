using System;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Sync.Abstraction.Extensions;
using UserProfileService.Sync.Abstraction.Models.Entities;
using UserProfileService.Sync.Projection.Abstractions;

namespace UserProfileService.Sync.Projection.Handlers;

internal class MemberDeletedEventHandler : SyncBaseEventHandler<MemberDeleted>
{
    private readonly IStreamNameResolver _streamNameResolver;

    /// <summary>
    ///     Creates a new instance of <see cref="MemberAddedEventHandler" />.
    /// </summary>
    /// <param name="logger">   A logger instance</param>
    /// <param name="streamNameResolver"> The resolver that will convert from or to stream names. </param>
    /// <param name="profileService"> A service used to communicate with the ups-sync database.</param>
    public MemberDeletedEventHandler(
        ILogger<MemberDeletedEventHandler> logger,
        IStreamNameResolver streamNameResolver,
        IProfileService profileService) : base(
        logger,
        profileService)
    {
        _streamNameResolver = streamNameResolver;
    }

    /// <inheritdoc />
    protected override async Task HandleInternalAsync(
        MemberDeleted eventObject,
        StreamedEventHeader eventHeader,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (eventObject == null)
        {
            throw new ArgumentNullException(nameof(eventObject));
        }

        if (eventHeader == null)
        {
            throw new ArgumentNullException(nameof(eventHeader));
        }

        if (string.IsNullOrWhiteSpace(eventObject.ContainerId))
        {
            throw new ArgumentException(
                "The container Id should not be null or whitespace",
                nameof(
                    eventObject.ContainerId));
        }

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTraceMessage(
                "Input parameter domainEvent: {domainEvent}",
                LogHelpers.Arguments(eventObject.ToLogString()));
        }

        Logger.LogTraceMessage(
            "Extracting object ident from event stream id {eventStreamId}.",
            eventHeader.EventStreamId.AsArgumentList());

        ObjectIdent relatedObjectIdent =
            _streamNameResolver.GetObjectIdentUsingStreamName(eventHeader.EventStreamId);

        if (relatedObjectIdent.Type == ObjectType.Organization)
        {
            string containerId = eventObject.ContainerId;
            string childId = eventObject.MemberId;

            Logger.LogInfoMessage(
                "Getting organization corresponding to the (Id = {containerId})",
                LogHelpers.Arguments(containerId));

            OrganizationSync organization;

            try
            {
                organization =
                    await ProfileService.GetProfileAsync<OrganizationSync>(containerId, cancellationToken);
            }
            catch (Exception e)
            {
                Logger.LogErrorMessage(
                    e,
                    "Error happened by loading organization corresponding to the (Id = {containerId})",
                    LogHelpers.Arguments(containerId));

                throw;
            }

            Logger.LogInfoMessage(
                "Removing member  (Id = {memberId}) to organization with (Id = {containerId}",
                LogHelpers.Arguments(childId, containerId));

            organization.DeleteObjectRelation(childId);

            try
            {
                await ProfileService.UpdateProfileAsync(organization, cancellationToken);
            }
            catch (Exception e)
            {
                Logger.LogErrorMessage(
                    e,
                    "Error happened by updating organization (Id = {containerId}) after removed old member  (Id = {memberId}).",
                    LogHelpers.Arguments(containerId, childId));

                throw;
            }

            Logger.LogInfoMessage(
                "Organization (Id = {containerId})  has been successfully updated.",
                LogHelpers.Arguments(containerId));

            Logger.ExitMethod();
        }
    }
}
