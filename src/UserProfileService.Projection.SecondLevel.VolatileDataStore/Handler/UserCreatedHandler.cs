using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
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
///     Handles <see cref="UserCreated" /> regarding assignments.
/// </summary>
internal class UserCreatedHandler : SecondLevelVolatileDataEventHandlerBase<UserCreated>
{
    /// <inheritdoc />
    public UserCreatedHandler(
        ISecondLevelVolatileDataRepository repository,
        IStreamNameResolver streamNameResolver,
        ILogger<UserCreatedHandler> logger) : base(repository, streamNameResolver, logger)
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

        await ExecuteInsideTransactionAsync(
            (repo, transaction, ct) => repo.SaveUserIdAsync(domainEvent.Id, transaction, ct),
            eventHeader,
            cancellationToken);

        Logger.ExitMethod();
    }
}
