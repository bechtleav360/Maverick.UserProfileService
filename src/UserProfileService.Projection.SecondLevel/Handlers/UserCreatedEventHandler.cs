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
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.SecondLevel.Abstractions;

namespace UserProfileService.Projection.SecondLevel.Handlers;

/// <summary>
///     Processes an <see cref="UserCreated" /> event.
/// </summary>
internal class UserCreatedEventHandler : SecondLevelEventHandlerBase<UserCreated>
{
    /// <summary>
    ///     Initializes a new instance of an <see cref="UserCreatedEventHandler" />.
    /// </summary>
    /// <param name="repository">The repository to be used.</param>
    /// <param name="mapper"></param>
    /// <param name="streamNameResolver">The resolver that will convert from or to stream names.</param>
    /// <param name="messageInformer">
    ///     The message informer is used to notify consumer (if present) when
    ///     an event message type appears.
    /// </param>
    /// <param name="logger">the logger to be used.</param>
    public UserCreatedEventHandler(
        ISecondLevelProjectionRepository repository,
        IMapper mapper,
        IStreamNameResolver streamNameResolver,
        IMessageInformer messageInformer,
        // ReSharper disable once SuggestBaseTypeForParameterInConstructor
        ILogger<UserCreatedEventHandler> logger) : base(repository, mapper, streamNameResolver, messageInformer, logger)
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

        var user =
            Mapper.Map<SecondLevelProjectionUser>(domainEvent);

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogDebugMessage(
                "Storing user: {profile}",
                LogHelpers.Arguments(user.ToLogString()));
        }
        else
        {
            Logger.LogInfoMessage(
                "Storing user profile (Id = {profileId})",
                LogHelpers.Arguments(user.Id));
        }

        await ExecuteInsideTransactionAsync(
            (repo, t, ct)
                => repo.CreateProfileAsync(
                    user,
                    t,
                    ct),
            eventHeader,
            cancellationToken);

        Logger.ExitMethod();
    }
}
