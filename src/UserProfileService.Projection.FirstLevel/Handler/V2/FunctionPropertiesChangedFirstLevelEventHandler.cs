using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models;
using Microsoft.Extensions.Logging;
using UserProfileService.Common;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.FirstLevel.Abstractions;
using PropertiesChangedResolvedEvent = Maverick.UserProfileService.AggregateEvents.Resolved.V1.PropertiesChanged;

namespace UserProfileService.Projection.FirstLevel.Handler.V2;

/// <summary>
///     This handler is used to process <see cref="FunctionPropertiesChangedEvent" />.
/// </summary>
internal class FunctionPropertiesChangedFirstLevelEventHandler : FirstLevelEventHandlerBase<FunctionPropertiesChangedEvent>
{
    private readonly IPropertiesChangedRelatedEventsResolver _PropertiesChangedRelatedEventsResolver;
    private readonly ISagaService _SagaService;

    /// <summary>
    ///     Creates an instance of the object <see cref="FunctionPropertiesChangedFirstLevelEventHandler" />.
    /// </summary>
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
    /// <param name="propertiesChangedRelatedEventsResolver"></param>
    public FunctionPropertiesChangedFirstLevelEventHandler(
        ILogger<FunctionPropertiesChangedFirstLevelEventHandler> logger,
        IFirstLevelProjectionRepository repository,
        ISagaService sagaService,
        IPropertiesChangedRelatedEventsResolver propertiesChangedRelatedEventsResolver) : base(logger, repository)
    {
        _SagaService = sagaService;
        _PropertiesChangedRelatedEventsResolver = propertiesChangedRelatedEventsResolver;
    }

    /// <summary>
    ///     Handles the eventObject with eventObject specific processes.
    /// </summary>
    /// <param name="eventObject">The eventObject object that occurred.</param>
    /// <param name="streamEvent"></param>
    /// <param name="transaction"> Defines an object as a result of a started transaction.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task HandleInternalAsync(
        FunctionPropertiesChangedEvent eventObject,
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
                "@event: {event}.",
                LogHelpers.Arguments(eventObject.ToLogString()));
        }

        List<EventTuple> eventTupleEvents =
            await _PropertiesChangedRelatedEventsResolver.CreateFunctionPropertiesChangedEventsAsync(
                eventObject.Payload.Id,
                eventObject,
                PropertiesChangedContext.Self,
                transaction,
                cancellationToken);

        Guid sagaId = await _SagaService.CreateBatchAsync(cancellationToken, eventTupleEvents.ToArray());

        await _SagaService.ExecuteBatchAsync(sagaId, cancellationToken);

        Logger.ExitMethod();
    }
}
