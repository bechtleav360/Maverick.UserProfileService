using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
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

namespace UserProfileService.Projection.SecondLevel.Handlers;

/// <summary>
///     Processes an <see cref="ContainerDeleted" /> event.
/// </summary>
internal class ContainerDeletedEventHandler : SecondLevelEventHandlerBase<ContainerDeleted>
{
    /// <summary>
    ///     Initializes a new instance of an <see cref="ContainerDeletedEventHandler" />.
    /// </summary>
    /// <param name="repository">The repository to be used.</param>
    /// <param name="mapper">The mapper instance used by the handler.</param>
    /// <param name="streamNameResolver">The resolver that will convert from or to stream names.</param>
    /// <param name="messageInformer">
    ///     The message informer is used to notify consumer (if present) when
    ///     an event message type appears.
    /// </param>
    /// <param name="logger">the logger to be used.</param>
    public ContainerDeletedEventHandler(
        ISecondLevelProjectionRepository repository,
        IMapper mapper,
        IStreamNameResolver streamNameResolver,
        IMessageInformer messageInformer,
        // ReSharper disable once SuggestBaseTypeForParameterInConstructor
        ILogger<ContainerDeletedEventHandler> logger) : base(repository, mapper, streamNameResolver, messageInformer, logger)
    {
    }

    /// <inheritdoc />
    protected override async Task HandleEventAsync(
        ContainerDeleted domainEvent,
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

        if (string.IsNullOrWhiteSpace(domainEvent.MemberId))
        {
            throw new InvalidDomainEventException(
                "Could not remove memberOf: Resource memberId is missing.",
                domainEvent);
        }

        if (string.IsNullOrWhiteSpace(domainEvent.ContainerId))
        {
            throw new InvalidDomainEventException(
                "Could not remove memberOf: Resource containerId is missing.",
                domainEvent);
        }

        string relatedProfileId = relatedEntityIdent.Id;
        string memberId = domainEvent.MemberId;
        string containerId = domainEvent.ContainerId;

        Logger.LogInfoMessage(
            "Removing container of type {containerType} with (Id = {containerId} from the entity (memberId = {memberId})",
            LogHelpers.Arguments(domainEvent.ContainerType.ToString(), containerId, memberId));

        await ExecuteInsideTransactionAsync(
            async (repo, t, ct) =>
            {
                await repo.RemoveMemberOfAsync(
                    relatedProfileId,
                    memberId,
                    domainEvent.ContainerType,
                    containerId,
                    null,
                    t,
                    ct);

                await UpdateProfileTimestampAsync(
                    memberId,
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
