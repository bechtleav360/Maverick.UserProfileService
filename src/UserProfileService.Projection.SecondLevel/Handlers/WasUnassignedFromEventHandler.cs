using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Informer.Abstraction;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.SecondLevel.Abstractions;
using RangeCondition = Maverick.UserProfileService.AggregateEvents.Common.Models.RangeCondition;

namespace UserProfileService.Projection.SecondLevel.Handlers;

/// <summary>
///     Processes an <see cref="WasUnassignedFrom" /> event.
/// </summary>
internal class WasUnassignedFromEventHandler : SecondLevelEventHandlerBase<WasUnassignedFrom>
{
    /// <summary>
    ///     Initializes a new instance of an <see cref="WasUnassignedFromEventHandler" />.
    /// </summary>
    /// <param name="repository"> The repository to be used. </param>
    /// <param name="mapper"> The used mapped instance. </param>
    /// <param name="streamNameResolver"> The resolver that will convert from or to stream names. </param>
    /// <param name="messageInformer">
    ///     The message informer is used to notify consumer (if present) when
    ///     an event message type appears.
    /// </param>
    /// <param name="logger"> the logger to be used. </param>
    public WasUnassignedFromEventHandler(
        ISecondLevelProjectionRepository repository,
        IMapper mapper,
        IStreamNameResolver streamNameResolver,
        IMessageInformer messageInformer,
        ILogger<WasUnassignedFromEventHandler> logger) : base(repository, mapper, streamNameResolver, messageInformer, logger)
    {
    }

    /// <inheritdoc />
    protected override async Task HandleEventAsync(
        WasUnassignedFrom domainEvent,
        StreamedEventHeader eventHeader,
        ObjectIdent relatedEntityIdent,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTraceMessage(
                "Input parameter domainEvent: {domainEvent}",
                LogHelpers.Arguments(domainEvent.ToLogString()));
        }

        if (relatedEntityIdent == null)
        {
            throw new ArgumentNullException(nameof(relatedEntityIdent));
        }

        if (string.IsNullOrWhiteSpace(relatedEntityIdent.Id))
        {
            throw new ArgumentNullException(nameof(relatedEntityIdent.Id));
        }

        if (domainEvent == null)
        {
            throw new ArgumentNullException(nameof(domainEvent));
        }

        if (string.IsNullOrWhiteSpace(domainEvent.ChildId))
        {
            throw new InvalidDomainEventException(
                "Could not delete container: Resource ChildId is missing.",
                domainEvent);
        }

        if (string.IsNullOrWhiteSpace(domainEvent.ParentId))
        {
            throw new InvalidDomainEventException(
                "Could not delete container: Resource ParentId is missing.",
                domainEvent);
        }

        string relatedProfileId = relatedEntityIdent.Id;
        string memberId = domainEvent.ChildId;
        ContainerType containerType = domainEvent.ParentType;
        string containerId = domainEvent.ParentId;
        IList<RangeCondition> conditions = domainEvent.Conditions?.ToList();

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogDebugMessage(
                "Deleting conditions :{conditions} on container with type: {containerType} ,Id: {containerId} from the member (Id = {memberId}))",
                LogHelpers.Arguments(conditions.ToLogString(), containerType.ToString(), containerId, memberId));
        }
        else
        {
            Logger.LogDebugMessage(
                "Unassigning container (only with given conditions) with type: {containerType} and Id: {containerId} from the member (Id = {memberId}))",
                LogHelpers.Arguments(containerType.ToString(), containerId, memberId));
        }

        await ExecuteInsideTransactionAsync(
            (repo, t, ct)
                => repo.RemoveMemberOfAsync(
                    relatedProfileId,
                    memberId,
                    containerType,
                    containerId,
                    conditions,
                    t,
                    ct),
            eventHeader,
            cancellationToken);

        Logger.ExitMethod();
    }
}
