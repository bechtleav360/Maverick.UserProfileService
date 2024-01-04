using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
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

namespace UserProfileService.Projection.SecondLevel.Handlers;

/// <summary>
///     Processes an <see cref="ClientSettingsCalculated" /> event.
/// </summary>
internal class AssignmentConditionTriggeredEventHandler : SecondLevelEventHandlerBase<AssignmentConditionTriggered>
{
    /// <summary>
    ///     Initializes a new instance of an <see cref="ClientSettingsCalculatedEventHandler" />.
    /// </summary>
    /// <param name="repository">The repository to be used.</param>
    /// <param name="mapper">The mapper is used to map objects from one type, to another.</param>
    /// <param name="streamNameResolver">The resolver that will convert from or to stream names.</param>
    /// <param name="messageInformer">
    ///     The message informer is used to notify consumer (if present) when
    ///     an event message type appears.
    /// </param>
    /// <param name="logger">The logger to be used.</param>
    public AssignmentConditionTriggeredEventHandler(
        ISecondLevelProjectionRepository repository,
        IMapper mapper,
        IStreamNameResolver streamNameResolver,
        IMessageInformer messageInformer,
        ILogger<AssignmentConditionTriggeredEventHandler> logger) : base(
        repository,
        mapper,
        streamNameResolver,
        messageInformer,
        logger)
    {
    }

    /// <inheritdoc />
    protected override async Task HandleEventAsync(
        AssignmentConditionTriggered domainEvent,
        StreamedEventHeader eventHeader,
        ObjectIdent relatedEntityIdent,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (string.IsNullOrWhiteSpace(domainEvent.ProfileId))
        {
            throw new ArgumentException(
                "The profile id of the assignment condition triggered event is null or empty.",
                nameof(domainEvent));
        }

        if (string.IsNullOrWhiteSpace(domainEvent.TargetId))
        {
            throw new ArgumentException(
                "The target id of the assignment condition triggered event is null or empty.",
                nameof(domainEvent));
        }

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTraceMessage(
                "Input parameter domainEvent: {domainEvent}, relatedEntityIdent: {relatedEntityIdent}, eventHeader: {eventHeader }",
                LogHelpers.Arguments(
                    domainEvent.ToLogString(),
                    relatedEntityIdent.ToLogString(),
                    eventHeader.ToLogString()));
        }

        Logger.LogDebugMessage(
            "The conditional assignment of the profile with the id {id} has been triggered (target id {targetId}; target type {targetObjectType}) and is now {active}",
            LogHelpers.Arguments(
                domainEvent.ProfileId.ToLogString(),
                domainEvent.TargetId.ToLogString(),
                domainEvent.TargetObjectType.ToLogString(),
                domainEvent.IsActive ? "active" : "inactive"));

        cancellationToken.ThrowIfCancellationRequested();

        // case 1d           case 1c           case 1b           case 1a
        //   |                 |                 |                 |
        //   V                 V                 V                 V
        //
        //  (G) - [forever] - (G) - [changed] - (G) - [forever] - (U)

        //   case 2c           case 2b           case 2a
        //     |                 |                 |
        //     V                 V                 V
        //
        //    (F) - [changed] - (G) - [forever] - (U)
        //   /   \
        //  (O)  (R)
        //   ^    ^
        //   |     \
        //   |      \
        // case 2e  case 2d

        //  case 3c           case 3b           case 3a
        //    |                 |                 |
        //    V                 V                 V
        //
        //   (R) - [changed] - (G) - [forever] - (U)

        await ExecuteInsideTransactionAsync(
            (repo, t, ct)
                => repo.RecalculateAssignmentsAsync(
                    relatedEntityIdent,
                    domainEvent.ProfileId,
                    domainEvent.TargetId,
                    domainEvent.TargetObjectType,
                    domainEvent.IsActive,
                    t,
                    ct),
            eventHeader,
            cancellationToken);
        
        Logger.ExitMethod();
    }
}
