using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.Models.Abstraction;
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

namespace UserProfileService.Projection.SecondLevel.Assignments.Handler;

/// <summary>
///     Handles <see cref="PropertiesChanged" /> regarding assignments.
/// </summary>
internal class PropertiesChangedHandler : SecondLevelAssignmentEventHandlerBase<PropertiesChanged>
{
    /// <inheritdoc />
    public PropertiesChangedHandler(
        ISecondLevelAssignmentRepository repository,
        IMapper mapper,
        IStreamNameResolver streamNameResolver,
        ILogger<PropertiesChangedHandler> logger) : base(repository, mapper, streamNameResolver, logger)
    {
    }

    /// <inheritdoc />
    protected override async Task HandleEventAsync(
        PropertiesChanged domainEvent,
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

        if (domainEvent.Id == relatedEntityIdent.Id)
        {
            Logger.LogDebugMessage(
                "Skipping update of {userId} as the name of the own profile is not relevant",
                LogHelpers.Arguments(domainEvent.Id));

            Logger.ExitMethod();

            return;
        }

        if (!domainEvent.Properties.ContainsKey(nameof(IContainerProfile.Name)))
        {
            Logger.LogInfoMessage(
                "Ignoring properties changed for object {objectType} as the name property wasn't changed",
                LogHelpers.Arguments(relatedEntityIdent.ToLogString()));

            Logger.ExitMethod();

            return;
        }

        await ExecuteInsideTransactionAsync(
            (repo, transaction, ct)
                => HandleInternalAsync(
                    repo,
                    transaction,
                    relatedEntityIdent,
                    domainEvent.Id,
                    domainEvent.Properties[nameof(IContainerProfile.Name)].ToString(),
                    ct),
            eventHeader,
            cancellationToken);

        Logger.ExitMethod();
    }

    protected async Task HandleInternalAsync(
        ISecondLevelAssignmentRepository repo,
        IDatabaseTransaction transaction,
        ObjectIdent relatedEntityId,
        string changedEntityId,
        string newName,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        Logger.LogInfoMessage(
            "Updating name of {changedEntityId} for assignment user {userId}",
            LogHelpers.Arguments(changedEntityId, relatedEntityId.Id));

        cancellationToken.ThrowIfCancellationRequested();

        SecondLevelProjectionAssignmentsUser currentState =
            await repo.GetAssignmentUserAsync(relatedEntityId.Id, transaction, cancellationToken);

        ISecondLevelAssignmentContainer container =
            currentState.Containers.FirstOrDefault(c => c.Id == changedEntityId);

        if (container == null)
        {
            Logger.LogWarnMessage(
                "Unable to update the name of the specified container {containerId} in assignments of {userId}",
                LogHelpers.Arguments(changedEntityId, relatedEntityId.Id));

            return;
        }

        container.Name = newName;
        cancellationToken.ThrowIfCancellationRequested();

        cancellationToken.ThrowIfCancellationRequested();
        Logger.LogDebugMessage("Storing the new assignment user.", LogHelpers.Arguments());
        await repo.SaveAssignmentUserAsync(currentState, transaction, cancellationToken);

        Logger.ExitMethod();
    }
}
