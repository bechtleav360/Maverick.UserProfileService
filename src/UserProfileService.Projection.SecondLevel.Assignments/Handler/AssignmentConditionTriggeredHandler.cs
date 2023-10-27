using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.SecondLevel.Assignments.Abstractions;
using UserProfileService.Projection.SecondLevel.Assignments.Utilities;

namespace UserProfileService.Projection.SecondLevel.Assignments.Handler;

/// <summary>
///     Handles <see cref="AssignmentConditionTriggered" /> regarding assignments.
/// </summary>
internal class AssignmentConditionTriggeredHandler : SecondLevelAssignmentEventHandlerBase<AssignmentConditionTriggered>
{
    /// <inheritdoc />
    public AssignmentConditionTriggeredHandler(
        ISecondLevelAssignmentRepository repository,
        IMapper mapper,
        IStreamNameResolver streamNameResolver,
        ILogger<AssignmentConditionTriggered> logger) : base(repository, mapper, streamNameResolver, logger)
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

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTraceMessage(
                "Input parameter domainEvent: {domainEvent}",
                LogHelpers.Arguments(domainEvent.ToLogString()));
        }

        if (relatedEntityIdent.Type != ObjectType.User)
        {
            Logger.LogInfoMessage(
                "Ignoring assignment condition triggered event for object {objectType} as it is no user",
                LogHelpers.Arguments(relatedEntityIdent.Type));

            Logger.ExitMethod();

            return;
        }

        await ExecuteInsideTransactionAsync(
            (repo, transaction, ct)
                => HandleInternalAsync(repo, transaction, relatedEntityIdent, ct),
            eventHeader,
            cancellationToken);

        Logger.ExitMethod();
    }

    protected async Task HandleInternalAsync(
        ISecondLevelAssignmentRepository repo,
        IDatabaseTransaction transaction,
        ObjectIdent relatedEntityId,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        Logger.LogInfoMessage(
            "Recalculating active memberships of user {userId} because of AssignmentConditionTriggered event",
            LogHelpers.Arguments(relatedEntityId.Id));

        cancellationToken.ThrowIfCancellationRequested();

        SecondLevelProjectionAssignmentsUser currentState =
            await repo.GetAssignmentUserAsync(relatedEntityId.Id, transaction, cancellationToken);

        ISet<ObjectIdent> newAssignments = currentState.CalculateActiveMemberships(Logger);

        Logger.LogDebugMessage(
            "Found {n} active memberships for user {userId}",
            LogHelpers.Arguments(newAssignments.Count, relatedEntityId.Id));

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTraceMessage(
                "User {userId} is member of the following containers {containers}",
                LogHelpers.Arguments(
                    relatedEntityId.Id,
                    string.Join(", ", newAssignments.Select(a => a.ToLogString()))));
        }

        currentState.ActiveMemberships = newAssignments.ToList();

        cancellationToken.ThrowIfCancellationRequested();
        Logger.LogDebugMessage("Storing the new assignment user.", LogHelpers.Arguments());
        await repo.SaveAssignmentUserAsync(currentState, transaction, cancellationToken);

        Logger.ExitMethod();
    }
}
