using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.SecondLevel.Assignments.Abstractions;
using UserProfileService.Projection.SecondLevel.Assignments.Utilities;

namespace UserProfileService.Projection.SecondLevel.Assignments.Handler;

/// <summary>
///     Handles <see cref="WasAssignedToBase{TContainer}" /> regarding assignments.
/// </summary>
internal abstract class WasAssignedToHandlerBase<TContainer, TEvent> : SecondLevelAssignmentEventHandlerBase<TEvent>
    where TContainer : class, IContainer
    where TEvent : WasAssignedToBase<TContainer>
{
    /// <inheritdoc />
    protected WasAssignedToHandlerBase(
        ISecondLevelAssignmentRepository repository,
        IMapper mapper,
        IStreamNameResolver streamNameResolver,
        ILogger logger) : base(repository, mapper, streamNameResolver, logger)
    {
    }

    private async Task HandleInternalAsync(
        IDatabaseTransaction transaction,
        ObjectIdent relatedEntityId,
        WasAssignedToBase<TContainer> assignedEvent,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        Logger.LogDebugMessage(
            "Adding assignment for user {userId}",
            LogHelpers.Arguments(relatedEntityId.Id));

        cancellationToken.ThrowIfCancellationRequested();

        SecondLevelProjectionAssignmentsUser currentState =
            await Repository.GetAssignmentUserAsync(relatedEntityId.Id, transaction, cancellationToken);

        if (currentState == null)
        {
            throw new StatesMismatchException(
                $"Expecting to have an assignments entry for {relatedEntityId.ToLogString()}");
        }

        IContainer targetContainer = assignedEvent.Target;

        cancellationToken.ThrowIfCancellationRequested();

        if (currentState.Containers.All(c => c.Id != targetContainer.Id))
        {
            Logger.LogDebugMessage(
                "The container {containerId} is not known for assignment user {relatedId}",
                LogHelpers.Arguments(targetContainer, relatedEntityId.ToLogString()));

            AddContainer(currentState, targetContainer);
        }

        SecondLevelProjectionAssignment currentAssignment = currentState.Assignments.FirstOrDefault(
            a => a.Profile.Id == assignedEvent.ProfileId && a.Parent.Id == targetContainer.Id);

        cancellationToken.ThrowIfCancellationRequested();

        if (currentAssignment != null)
        {
            Logger.LogInfoMessage(
                "Adding more conditions to the assignment between {profileId} and {targetId} for user {relatedId}",
                LogHelpers.Arguments(assignedEvent.ProfileId, assignedEvent.Target.Id, relatedEntityId.Id));

            currentAssignment.Conditions = currentAssignment.Conditions
                .Union(Mapper.Map<IList<RangeCondition>>(assignedEvent.Conditions))
                .ToList();
        }
        else
        {
            Logger.LogInfoMessage(
                "Adding assignment between {profileId} and {targetId} for user {relatedId}",
                LogHelpers.Arguments(assignedEvent.ProfileId, assignedEvent.Target.Id, relatedEntityId.Id));

            ISecondLevelAssignmentContainer childContainer =
                currentState.Containers.FirstOrDefault(c => c.Id == assignedEvent.ProfileId);

            ObjectType objectType = childContainer != null
                ? Mapper.Map<ObjectType>(childContainer.ContainerType)
                : ObjectType.User;

            currentState.Assignments.Add(
                new SecondLevelProjectionAssignment
                {
                    Profile = new ObjectIdent(assignedEvent.ProfileId, objectType),
                    Conditions = Mapper.Map<IList<RangeCondition>>(assignedEvent.Conditions),
                    Parent = Mapper.Map<ObjectIdent>(assignedEvent.Target)
                });
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
        await Repository.SaveAssignmentUserAsync(currentState, transaction, cancellationToken);

        Logger.ExitMethod();
    }

    /// <inheritdoc />
    protected override async Task HandleEventAsync(
        TEvent domainEvent,
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
                => HandleInternalAsync(transaction, relatedEntityIdent, domainEvent, ct),
            eventHeader,
            cancellationToken);

        Logger.ExitMethod();
    }

    protected virtual void AddContainer(SecondLevelProjectionAssignmentsUser user, IContainer container)
    {
        Logger.EnterMethod();

        if (user.Containers.Any(c => c.Id == container.Id))
        {
            Logger.LogDebug(
                "The container to add to assignment user {userId} already exists",
                LogHelpers.Arguments(user.ProfileId));

            Logger.ExitMethod();

            return;
        }

        user.Containers.Add(Mapper.Map<ISecondLevelAssignmentContainer>(container));
        Logger.ExitMethod();
    }
}
