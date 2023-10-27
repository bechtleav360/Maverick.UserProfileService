using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Informer.Abstraction;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.SecondLevel.Abstractions;
using RangeCondition = Maverick.UserProfileService.AggregateEvents.Common.Models.RangeCondition;

namespace UserProfileService.Projection.SecondLevel.Handlers;

/// <summary>
///     Processes an <see cref="MemberAdded" /> event.
/// </summary>
internal class MemberRemovedEventHandler : SecondLevelEventHandlerBase<MemberRemoved>
{
    /// <summary>
    ///     Initializes a new instance of an
    ///     <see cref="UserProfileService.Projection.SecondLevel.Handlers.MemberRemovedEventHandler" />.
    /// </summary>
    /// <param name="repository"> The repository to be used. </param>
    /// <param name="mapper"> </param>
    /// <param name="streamNameResolver"> The resolver that will convert from or to stream names. </param>
    /// <param name="messageInformer">
    ///     The message informer is used to notify consumer (if present) when
    ///     an event message type appears.
    /// </param>
    /// <param name="logger"> the logger to be used. </param>
    public MemberRemovedEventHandler(
        ISecondLevelProjectionRepository repository,
        IMapper mapper,
        IStreamNameResolver streamNameResolver,
        IMessageInformer messageInformer,
        // ReSharper disable once SuggestBaseTypeForParameterInConstructor
        ILogger<MemberAddedEventHandler> logger) : base(repository, mapper, streamNameResolver, messageInformer, logger)
    {
    }

    /// <inheritdoc />
    protected override async Task HandleEventAsync(
        MemberRemoved domainEvent,
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

        string memberId = domainEvent.MemberId;
        ContainerType containerType = domainEvent.ParentType;
        string containerId = domainEvent.ParentId;
        IList<RangeCondition> conditions = domainEvent.Conditions;

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogInfoMessage(
                "Removing assignments conditions: {conditions} for member:  {memberId} to container of type {containerType} with (Id = {containerId}",
                LogHelpers.Arguments(conditions.ToLogString(), memberId, containerType.ToString(), containerId));
        }
        else
        {
            Logger.LogInfoMessage(
                "Removing assignments conditions for member:  {memberId} to container of type {containerType} with (Id = {containerId}",
                LogHelpers.Arguments(memberId, containerType.ToString(), containerId));
        }

        await ExecuteInsideTransactionAsync(
            (repo, t, ct)
                => repo.RemoveMemberAsync(
                    containerId,
                    containerType,
                    memberId,
                    conditions,
                    t,
                    ct),
            eventHeader,
            cancellationToken);

        Logger.ExitMethod();
    }
}
