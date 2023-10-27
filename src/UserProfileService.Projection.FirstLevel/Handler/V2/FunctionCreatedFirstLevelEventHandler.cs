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
using V3FunctionCreatedHandler = UserProfileService.Projection.FirstLevel.Handler.V3;
using V3FunctionCreatedEvent = UserProfileService.Events.Implementation.V3.FunctionCreatedEvent;

namespace UserProfileService.Projection.FirstLevel.Handler.V2;

/// <summary>
///     This handler is used to process <see cref="FunctionCreatedEvent" />.
/// </summary>
internal class FunctionCreatedFirstLevelEventHandler : FirstLevelEventHandlerBase<FunctionCreatedEvent>
{
    private readonly IMapper _Mapper;
    private readonly V3FunctionCreatedHandler.FunctionCreatedFirstLevelEventHandler _v3FunctionCreatedFirstLevelEventHandler;

    /// <summary>
    ///     Creates an instance of the object <see cref="FunctionCreatedFirstLevelEventHandler" />.
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
    public FunctionCreatedFirstLevelEventHandler(
        ILoggerFactory loggerFactory,
        ILogger<FunctionCreatedFirstLevelEventHandler> logger,
        IFirstLevelProjectionRepository repository,
        ISagaService sagaService,
        IFirstLevelEventTupleCreator creator,
        IMapper mapper) : base(logger, repository)
    {
        _Mapper = mapper;

        _v3FunctionCreatedFirstLevelEventHandler = new V3FunctionCreatedHandler.FunctionCreatedFirstLevelEventHandler(
            loggerFactory.CreateLogger<V3FunctionCreatedHandler.FunctionCreatedFirstLevelEventHandler>(),
            repository,
            sagaService,
            creator,
            mapper);
    }

    protected override async Task HandleInternalAsync(
        FunctionCreatedEvent eventObject,
        StreamedEventHeader streamEvent,
        IDatabaseTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (eventObject == null)
        {
            Logger.LogWarnMessage(
                "The variable with the name {variableName} is null.",
                LogHelpers.Arguments(nameof(eventObject)));

            throw new ArgumentNullException($"The variable with the name {nameof(eventObject)} is null.");
        }

        Logger.LogInfoMessage(
            "Trying to convert V2 to V3 of event {eventName}.",
            LogHelpers.Arguments(eventObject.GetType().Name));

        var functionCreatedEvent = _Mapper.Map<V3FunctionCreatedEvent>(eventObject);

        await _v3FunctionCreatedFirstLevelEventHandler.HandleEventAsync(functionCreatedEvent, streamEvent, cancellationToken);

        Logger.ExitMethod();
    }
}
