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
using UserProfileService.Projection.SecondLevel.Assignments.Abstractions;

namespace UserProfileService.Projection.SecondLevel.Assignments.Handler;

/// <summary>
///     Handles <see cref="UserCreated" /> regarding assignments.
/// </summary>
internal class EntityDeletedHandler : SecondLevelAssignmentEventHandlerBase<EntityDeleted>
{
    /// <inheritdoc />
    public EntityDeletedHandler(
        ISecondLevelAssignmentRepository repository,
        IMapper mapper,
        IStreamNameResolver streamNameResolver,
        ILogger<EntityDeletedHandler> logger) : base(repository, mapper, streamNameResolver, logger)
    {
    }

    /// <inheritdoc />
    protected override async Task HandleEventAsync(
        EntityDeleted domainEvent,
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

        cancellationToken.ThrowIfCancellationRequested();

        Logger.LogInfoMessage(
            "Deleting assignment user {userId} as the entity was deletd",
            LogHelpers.Arguments(relatedEntityId.Id));

        await repo.RemoveAssignmentUserAsync(relatedEntityId.Id, transaction, cancellationToken);

        Logger.ExitMethod();
    }
}
