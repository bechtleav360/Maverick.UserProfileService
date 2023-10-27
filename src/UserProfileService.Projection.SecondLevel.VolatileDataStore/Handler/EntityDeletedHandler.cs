using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.SecondLevel.VolatileDataStore.Abstractions;

namespace UserProfileService.Projection.SecondLevel.VolatileDataStore.Handler;

/// <summary>
///     Handles <see cref="EntityDeleted" /> regarding assignments.
/// </summary>
internal class EntityDeletedHandler : SecondLevelVolatileDataEventHandlerBase<EntityDeleted>
{
    /// <inheritdoc />
    public EntityDeletedHandler(
        ISecondLevelVolatileDataRepository repository,
        IStreamNameResolver streamNameResolver,
        ILogger<EntityDeletedHandler> logger) : base(repository, streamNameResolver, logger)
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

        // Calling the repo can be skipped, if the entity to be deleted is definitively something else than a user.
        if (relatedEntityIdent.Type != ObjectType.User
            && relatedEntityIdent.Type != ObjectType.Profile
            && relatedEntityIdent.Type != ObjectType.Unknown)
        {
            Logger.LogDebugMessage(
                "Entity deleted: Entity type {entityType} is not supported by this projection.",
                LogHelpers.Arguments(relatedEntityIdent.Type.ToString("G")));

            Logger.ExitMethod();
        }

        // for all other types an attempt to delete the profile will be started
        // this means, that no exception will be thrown, if the user profile cannot be found
        await ExecuteInsideTransactionAsync(
            (repo, transaction, ct) => repo.TryDeleteUserAsync(domainEvent.Id, transaction, ct),
            eventHeader,
            cancellationToken);

        Logger.ExitMethod();
    }
}
