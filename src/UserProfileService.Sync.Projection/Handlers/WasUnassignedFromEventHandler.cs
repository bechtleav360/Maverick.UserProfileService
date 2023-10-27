using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Sync.Abstraction.Extensions;
using UserProfileService.Sync.Abstraction.Models.Entities;
using UserProfileService.Sync.Projection.Abstractions;
using ObjectType = Maverick.UserProfileService.Models.EnumModels.ObjectType;
using RangeCondition = Maverick.UserProfileService.AggregateEvents.Common.Models.RangeCondition;

namespace UserProfileService.Sync.Projection.Handlers;

internal class WasUnassignedFromEventHandler : SyncBaseEventHandler<WasUnassignedFrom>
{
    private readonly IStreamNameResolver _streamNameResolver;

    /// <summary>
    ///     Create a new instance of <see cref="WasUnassignedFromEventHandler" />
    /// </summary>
    /// <param name="logger">The logger <see cref="ILogger" /></param>
    /// <param name="profileService">An instance of <see cref="IProfileService" /> used to handle user operations.</param>
    /// <param name="streamNameResolver">
    ///     Object used to resolve object information through the stream name
    ///     <see cref="IStreamNameResolver" />
    /// </param>
    public WasUnassignedFromEventHandler(
        ILogger<WasUnassignedFromEventHandler> logger,
        IProfileService profileService,
        IStreamNameResolver streamNameResolver) : base(logger, profileService)
    {
        _streamNameResolver = streamNameResolver;
    }

    /// <inheritdoc />
    protected override async Task HandleInternalAsync(
        WasUnassignedFrom eventObject,
        StreamedEventHeader eventHeader,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (eventObject == null)
        {
            throw new ArgumentNullException(nameof(eventObject));
        }

        if (string.IsNullOrWhiteSpace(eventObject.ParentId))
        {
            throw new ArgumentException("Parent id should not be null or whitespace", nameof(eventObject.ParentId));
        }

        if (string.IsNullOrWhiteSpace(eventObject.ChildId))
        {
            throw new ArgumentNullException(nameof(eventObject.ChildId));
        }

        ObjectIdent relatedObjectIdent =
            _streamNameResolver.GetObjectIdentUsingStreamName(eventHeader.EventStreamId);

        if (eventObject.ParentType == ContainerType.Organization
            && relatedObjectIdent.Id == eventObject.ChildId
            && relatedObjectIdent.Type == ObjectType.Organization)
        {
            Logger.LogInfoMessage(
                "Getting organization with the id: {orgaId} in the sync database",
                LogHelpers.Arguments(eventObject.ChildId));

            var organization = await ProfileService.GetProfileAsync<OrganizationSync>(
                eventObject.ChildId,
                cancellationToken);

            Logger.LogInfoMessage(
                "Unassigning child with the id: {childId} from the organization with the id: {orgaId} in the sync database",
                LogHelpers.Arguments(eventObject.ChildId, eventObject.ParentId));

            List<RangeCondition> conditions = organization
                ?.RelatedObjects?.FirstOrDefault(r => r.MaverickId == eventObject.ParentId)
                ?.Conditions.ToList();

            if (conditions == null)
            {
                Logger.LogWarnMessage(
                    "No range conditions have been found for the parent with the id: {parentId} inside the organization with the id:{orgaId} in the sync database",
                    LogHelpers.Arguments(eventObject.ParentId, eventObject.ChildId));

                return;
            }

            foreach (RangeCondition eventObjectCondition in eventObject.Conditions)
            {
                conditions.RemoveAll(c => c.Start == eventObjectCondition.Start && c.End == eventObjectCondition.End);
            }

            organization.RelatedObjects.First(r => r.MaverickId == eventObject.ParentId).Conditions = conditions;
            organization.CleanProfileAfterDeleteAssignments();

            Logger.LogInfoMessage(
                "Updating organization with the id: {orgaId} in the sync database",
                LogHelpers.Arguments(eventObject.ChildId));

            if (Logger.IsEnabledForTrace())
            {
                Logger.LogInfoMessage(
                    "Updating assignment conditions : {conditions} between child with the id: {childId} and the organization with the id: {orgaId} in the sync database",
                    LogHelpers.Arguments(
                        string.Join(',', conditions.Select(c => $"Start: {c.Start}; End: {c.End}")),
                        eventObject.ChildId,
                        eventObject.ParentId));
            }

            await ProfileService.UpdateProfileAsync(organization, cancellationToken);

            Logger.LogInfoMessage(
                "The organization (Id = {orgaId}) has been successfully updated",
                LogHelpers.Arguments(eventObject.ChildId));

            Logger.ExitMethod();
        }
    }
}
