﻿using System;
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
using UserProfileService.Projection.SecondLevel.Utilities;

namespace UserProfileService.Projection.SecondLevel.Handlers;

/// <summary>
///     Processes an <see cref="MemberAdded" /> event.
/// </summary>
internal class MemberDeletedEventHandler : SecondLevelEventHandlerBase<MemberDeleted>
{
    /// <summary>
    ///     Initializes a new instance of an
    ///     <see cref="UserProfileService.Projection.SecondLevel.Handlers.MemberDeletedEventHandler" />.
    /// </summary>
    /// <param name="repository"> The repository to be used. </param>
    /// <param name="mapper"> </param>
    /// <param name="streamNameResolver"> The resolver that will convert from or to stream names. </param>
    /// <param name="messageInformer">
    ///     The message informer is used to notify consumer (if present) when
    ///     an event message type appears.
    /// </param>
    /// <param name="logger"> the logger to be used. </param>
    public MemberDeletedEventHandler(
        ISecondLevelProjectionRepository repository,
        IMapper mapper,
        IStreamNameResolver streamNameResolver,
        IMessageInformer messageInformer,
        // ReSharper disable once SuggestBaseTypeForParameterInConstructor
        ILogger<MemberDeletedEventHandler> logger) : base(repository, mapper, streamNameResolver, messageInformer, logger)
    {
    }

    /// <inheritdoc />
    protected override async Task HandleEventAsync(
        MemberDeleted domainEvent,
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
        ContainerType containerType = EnumConverter.ConvertObjectTypeToContainerType(relatedEntityIdent.Type);
        string containerId = domainEvent.ContainerId;

        Logger.LogInfoMessage(
            "Removing member:  {memberId} to container of type {containerType} with (Id = {containerId}",
            LogHelpers.Arguments(memberId, containerType.ToString(), containerId));

        await ExecuteInsideTransactionAsync(
            async (repo, t, ct) =>
            {
                await repo.RemoveMemberAsync(
                    containerId,
                    containerType,
                    memberId,
                    null,
                    t,
                    ct);

                // set new UpdateAt date depending on object type
                switch (containerType)
                {
                    case ContainerType.Group:
                    case ContainerType.Organization:

                        await UpdateProfileTimestampAsync(
                            containerId,
                            domainEvent.MetaData.Timestamp,
                            repo,
                            t,
                            ct);

                        break;
                    case ContainerType.Role:

                        await UpdateRoleTimestampAsync(
                            containerId,
                            domainEvent.MetaData.Timestamp,
                            repo,
                            t,
                            ct);

                        break;
                    case ContainerType.Function:

                        await UpdateFunctionTimestampAsync(
                            containerId,
                            domainEvent.MetaData.Timestamp,
                            repo,
                            t,
                            ct);

                        break;
                }
            },
            eventHeader,
            cancellationToken);

        Logger.ExitMethod();
    }
}
