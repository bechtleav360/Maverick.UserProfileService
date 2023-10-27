using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Maverick.UserProfileService.AggregateEvents.Common;
using Microsoft.Extensions.Logging;
using UserProfileService.Common;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.FirstLevel.Abstractions;
using V3UserCreatedEvent = UserProfileService.Events.Implementation.V3.UserCreatedEvent;

namespace UserProfileService.Projection.FirstLevel.Handler.V2;

/// <summary>
///     This handler is used to process <see cref="UserCreatedEvent" />.
/// </summary>
internal class UserCreatedFirstLevelEventHandler : FirstLevelEventHandlerTagsIncludedBase<UserCreatedEvent>
{
    internal readonly V3.UserCreatedFirstLevelEventHandler _v3UserCreatedFirstLevelEventHandler;

    /// <summary>
    ///     Creates an instance of the object <see cref="UserCreatedFirstLevelEventHandler" />.
    /// </summary>
    /// <param name="mapper">The Mapper is used to map several existing events in new event with the right order.</param>
    /// <param name="loggerFactory">The logger factory creates a logger that is used to write logging messages.</param>
    /// <param name="logger">
    ///     The logger factory that is used to create a logger. The logger logs message for debugging
    ///     and control reasons.
    /// </param>
    /// <param name="repository">
    ///     The read service is used to read from the internal query storage to get all information to
    ///     generate all needed stream events.
    /// </param>
    /// <param name="sagaService">
    ///     The saga service is used to write all created <see cref="IUserProfileServiceEvent" /> to the
    ///     write stream.
    /// </param>
    /// <param name="creator">The creator is used to create <inheritdoc cref="EventTuple" /> from the given parameter.</param>
    public UserCreatedFirstLevelEventHandler(
        ILoggerFactory loggerFactory,
        ILogger<UserCreatedFirstLevelEventHandler> logger,
        IFirstLevelProjectionRepository repository,
        ISagaService sagaService,
        IFirstLevelEventTupleCreator creator,
        IMapper mapper) : base(logger, repository, sagaService, mapper, creator)
    {
        _v3UserCreatedFirstLevelEventHandler =
            new V3.UserCreatedFirstLevelEventHandler(
                loggerFactory.CreateLogger<V3.UserCreatedFirstLevelEventHandler>(),
                repository,
                sagaService,
                creator,
                mapper);
    }

    protected override async Task HandleInternalAsync(
        UserCreatedEvent eventObject,
        StreamedEventHeader streamEvent,
        IDatabaseTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (eventObject == null)
        {
            throw new ArgumentNullException(nameof(eventObject));
        }

        if (transaction == null)
        {
            throw new ArgumentNullException(nameof(transaction));
        }

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTraceMessage(
                "UserCreatedEvent: {userCreatedEvent}",
                eventObject.ToLogString().AsArgumentList());
        }

        Logger.LogInfoMessage(
            "Trying to convert V2 to V3 of event {eventName}.",
            LogHelpers.Arguments(eventObject.GetType().Name));

        var userCreatedEvent = Mapper.Map<V3UserCreatedEvent>(eventObject);

        await _v3UserCreatedFirstLevelEventHandler.HandleEventAsync(userCreatedEvent, streamEvent, cancellationToken);

        Logger.ExitMethod();
    }
}
