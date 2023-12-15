using System;
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
using Member = Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models.Member;

namespace UserProfileService.Projection.SecondLevel.Handlers;

/// <summary>
///     Processes an <see cref="MemberAdded" /> event.
/// </summary>
internal class MemberAddedEventHandler : SecondLevelEventHandlerBase<MemberAdded>
{
    /// <summary>
    ///     Initializes a new instance of an <see cref="MemberAddedEventHandler" />.
    /// </summary>
    /// <param name="repository"> The repository to be used. </param>
    /// <param name="mapper"> </param>
    /// <param name="streamNameResolver"> The resolver that will convert from or to stream names. </param>
    /// <param name="messageInformer">
    ///     The message informer is used to notify consumer (if present) when
    ///     an event message type appears.
    /// </param>
    /// <param name="logger"> the logger to be used. </param>
    public MemberAddedEventHandler(
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
        MemberAdded domainEvent,
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

        Member member = domainEvent.Member;
        ContainerType containerType = domainEvent.ParentType;
        string containerId = domainEvent.ParentId;

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogInfoMessage(
                "Adding member:  {member} to container of type {containerType} with (Id = {containerId}",
                LogHelpers.Arguments(member.ToLogString(), containerType.ToString(), containerId));
        }
        else
        {
            Logger.LogInfoMessage(
                "Adding member  (Id = {memberId}) to container of type {containerType} with (Id = {containerId}",
                LogHelpers.Arguments(member?.Id, containerType.ToString(), containerId));
        }

        await ExecuteInsideTransactionAsync(
            async (repo, t, ct) =>
            {
                await repo.AddMemberAsync(
                    containerId,
                    containerType,
                    member,
                    t,
                    ct);

                await UpdateProfileTimestampAsync(
                    containerId,
                    domainEvent.MetaData.Timestamp,
                    repo,
                    t,
                    ct);
            },
            eventHeader,
            cancellationToken);

        Logger.ExitMethod();
    }
}
