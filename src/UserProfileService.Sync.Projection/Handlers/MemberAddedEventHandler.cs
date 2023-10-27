using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models;
using Maverick.UserProfileService.Models.EnumModels;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Sync.Abstraction.Extensions;
using UserProfileService.Sync.Abstraction.Models;
using UserProfileService.Sync.Abstraction.Models.Entities;
using UserProfileService.Sync.Projection.Abstractions;

namespace UserProfileService.Sync.Projection.Handlers;

internal class MemberAddedEventHandler : SyncBaseEventHandler<MemberAdded>
{
    private readonly IMapper _mapper;

    /// <summary>
    ///     Creates a new instance of <see cref="MemberAddedEventHandler" />.
    /// </summary>
    /// <param name="logger">   A logger instance</param>
    /// <param name="mapper">   An instance of <see cref="IMapper" /> to convert some object to other types</param>
    /// <param name="profileService"> A service used to communicate with the ups-sync database.</param>
    public MemberAddedEventHandler(
        ILogger<MemberAddedEventHandler> logger,
        IMapper mapper,
        IProfileService profileService) : base(
        logger,
        profileService)
    {
        _mapper = mapper;
    }

    /// <inheritdoc />
    protected override async Task HandleInternalAsync(
        MemberAdded eventObject,
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

        if (string.IsNullOrWhiteSpace(eventObject.ParentId))
        {
            throw new ArgumentException(
                "The parent Id should not be null or whitespace",
                nameof(
                    eventObject.ParentId));
        }

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTraceMessage(
                "Input parameter domainEvent: {domainEvent}",
                LogHelpers.Arguments(eventObject.ToLogString()));
        }

        if (eventObject.ParentType == ContainerType.Organization)
        {
            Member member = eventObject.Member;
            ContainerType containerType = eventObject.ParentType;
            string containerId = eventObject.ParentId;

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

            if (Logger.IsEnabledForTrace())
            {
                Logger.LogInfoMessage(
                    "Adding member:  {member} to container of type {containerType} with (Id = {containerId}",
                    LogHelpers.Arguments(member.ToLogString(), containerType.ToString(), containerId));
            }
            else
            {
                Logger.LogInfoMessage(
                    "Adding member  (Id = {memberId}) to organization with (Id = {containerId}",
                    LogHelpers.Arguments(member?.Id, containerId));
            }

            var objectRelation = _mapper.Map<ObjectRelation>(member);
            objectRelation.AssignmentType = AssignmentType.ChildrenToParent;

            organization.AddObjectRelation(objectRelation);

            Logger.LogInfoMessage(
                "Updating organization (Id = {containerId}) with a new member  (Id = {memberId}).",
                LogHelpers.Arguments(containerId, member?.Id));

            try
            {
                await ProfileService.UpdateProfileAsync(organization, cancellationToken);
            }
            catch (Exception e)
            {
                Logger.LogErrorMessage(
                    e,
                    "Error happened by updating organization (Id = {containerId}) with a new member  (Id = {memberId}).",
                    LogHelpers.Arguments(containerId, member?.Id));

                throw;
            }

            Logger.LogInfoMessage(
                "Organization (Id = {containerId})  has been successfully updated with a new member  (Id = {memberId}).",
                LogHelpers.Arguments(containerId, member?.Id));
        }
    }
}
