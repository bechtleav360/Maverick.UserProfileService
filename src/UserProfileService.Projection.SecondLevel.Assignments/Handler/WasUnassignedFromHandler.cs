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
///     Handles <see cref="WasUnassignedFrom" /> regarding assignments.
/// </summary>
internal class WasUnassignedFromHandler : SecondLevelAssignmentEventHandlerBase<WasUnassignedFrom>
{
    /// <inheritdoc />
    public WasUnassignedFromHandler(
        ISecondLevelAssignmentRepository repository,
        IMapper mapper,
        IStreamNameResolver streamNameResolver,
        ILogger<WasUnassignedFromHandler> logger) : base(repository, mapper, streamNameResolver, logger)
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
                => HandleInternalAsync(
                    repo,
                    transaction,
                    relatedEntityIdent,
                    domainEvent,
                    ct),
            eventHeader,
            cancellationToken);

        Logger.ExitMethod();
    }

    protected async Task HandleInternalAsync(
        ISecondLevelAssignmentRepository repo,
        IDatabaseTransaction transaction,
        ObjectIdent relatedEntityId,
        WasUnassignedFrom domainEvent,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        Logger.LogInfoMessage(
            "Creating assignment user {userId} with no assignments",
            LogHelpers.Arguments(relatedEntityId.Id));

        SecondLevelProjectionAssignmentsUser currentState =
            await repo.GetAssignmentUserAsync(relatedEntityId.Id, transaction, cancellationToken);

        SecondLevelProjectionAssignment assignment = currentState.Assignments.SingleOrDefault(
            a => a.Parent.Id == domainEvent.ParentId && a.Profile.Id == domainEvent.ChildId);

        if (assignment == null)
        {
            Logger.LogInfoMessage(
                "Unable to find the assignment between the two specified profiles",
                LogHelpers.Arguments());

            return;
        }

        assignment.Conditions = assignment.Conditions
            .Where(c => !domainEvent.Conditions.Any(dc => dc.Start == c.Start && dc.End == c.End))
            .ToList();

        cancellationToken.ThrowIfCancellationRequested();

        if (!assignment.Conditions.Any())
        {
            currentState.Assignments.Remove(assignment);

            Logger.LogInfoMessage(
                "Removing unnecessary container and assignments for user {userId}",
                LogHelpers.Arguments(currentState.ProfileId));

            ISet<ObjectIdent> connectedContainers = currentState.GetConnectedContainers();

            List<string> unnecessaryContainers = currentState.Containers.Select(c => c.Id)
                .Where(c => connectedContainers.All(con => con.Id != c))
                .ToList();

            currentState.Containers =
                currentState.Containers.Where(c => !unnecessaryContainers.Contains(c.Id)).ToList();

            currentState.Assignments = currentState.Assignments
                .Where(a => unnecessaryContainers.All(c => c != a.Parent.Id && c != a.Profile.Id))
                .ToList();
        }

        cancellationToken.ThrowIfCancellationRequested();

        Logger.LogDebugMessage(
            "Calculating active assignments for {profileId}",
            relatedEntityId.ToLogString().AsArgumentList());

        ISet<ObjectIdent> newAssignments = currentState.CalculateActiveMemberships(Logger);
        currentState.ActiveMemberships = newAssignments.ToList();

        if (Logger.IsEnabledFor(LogLevel.Debug))
        {
            Logger.LogDebugMessage(
                "Found {n} active assignments for {profileId}",
                LogHelpers.Arguments(newAssignments.Count, relatedEntityId.ToLogString()));
        }

        cancellationToken.ThrowIfCancellationRequested();
        Logger.LogDebugMessage("Storing the new assignment user.", LogHelpers.Arguments());
        await repo.SaveAssignmentUserAsync(currentState, transaction, cancellationToken);

        Logger.ExitMethod();
    }
}
