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

namespace UserProfileService.Projection.SecondLevel.Assignments.Handler;

/// <summary>
///     Handles <see cref="UserCreated" /> regarding assignments.
/// </summary>
internal class UserCreatedHandler : SecondLevelAssignmentEventHandlerBase<UserCreated>
{
    /// <inheritdoc />
    public UserCreatedHandler(
        ISecondLevelAssignmentRepository repository,
        IMapper mapper,
        IStreamNameResolver streamNameResolver,
        ILogger<UserCreatedHandler> logger) : base(repository, mapper, streamNameResolver, logger)
    {
    }

    /// <inheritdoc />
    protected override async Task HandleEventAsync(
        UserCreated domainEvent,
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
            "Creating assignment user {userId} with no assignments",
            LogHelpers.Arguments(relatedEntityId.Id));

        var currentState =
            new SecondLevelProjectionAssignmentsUser
            {
                ProfileId = relatedEntityId.Id
            };

        cancellationToken.ThrowIfCancellationRequested();
        Logger.LogDebugMessage("Storing the new assignment user.", LogHelpers.Arguments());
        await repo.SaveAssignmentUserAsync(currentState, transaction, cancellationToken);

        Logger.ExitMethod();
    }
}
