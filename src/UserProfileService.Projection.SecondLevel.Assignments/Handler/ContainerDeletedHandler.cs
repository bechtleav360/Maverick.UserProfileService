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
internal class ContainerDeletedHandler : SecondLevelAssignmentEventHandlerBase<ContainerDeleted>
{
    /// <inheritdoc />
    public ContainerDeletedHandler(
        ISecondLevelAssignmentRepository repository,
        IMapper mapper,
        IStreamNameResolver streamNameResolver,
        ILogger<ContainerDeletedHandler> logger) : base(repository, mapper, streamNameResolver, logger)
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
        ContainerDeleted domainEvent,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        Logger.LogInfoMessage(
            "Removing container {containerid} from assignment user {userId}",
            LogHelpers.Arguments(relatedEntityId.Id, domainEvent.ContainerId));

        cancellationToken.ThrowIfCancellationRequested();

        SecondLevelProjectionAssignmentsUser currentState =
            await repo.GetAssignmentUserAsync(relatedEntityId.Id, transaction, cancellationToken);

        currentState.Assignments = currentState.Assignments.Where(
                a => a.Parent.Id
                    != domainEvent.ContainerId)
            .ToList();

        cancellationToken.ThrowIfCancellationRequested();

        Logger.LogInfoMessage(
            "Removing unnecessary container and assignments for user {userId}",
            LogHelpers.Arguments(currentState.ProfileId));

        ISet<ObjectIdent> connectedContainers = currentState.GetConnectedContainers();

        List<string> unnecessaryContainers = currentState.Containers
            .Select(c => c.Id)
            .Except(connectedContainers.Select(c => c.Id))
            .ToList();

        currentState.Containers =
            currentState.Containers.Where(c => !unnecessaryContainers.Contains(c.Id)).ToList();

        currentState.Assignments = currentState.Assignments
            .Where(a => unnecessaryContainers.All(c => c != a.Parent.Id && c != a.Profile.Id))
            .ToList();

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
