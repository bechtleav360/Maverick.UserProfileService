using System;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.AggregateEvents.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Projection.FirstLevel.Abstractions;
using FunctionCreatedEventV3 = UserProfileService.Events.Implementation.V3.FunctionCreatedEvent;
using UserCreatedEventV3 = UserProfileService.Events.Implementation.V3.UserCreatedEvent;

namespace UserProfileService.Projection.FirstLevel;

/// <summary>
///     Contains the default implementation of a second-level-projection event handler. It will use dependency injection to
///     get event handlers.
/// </summary>
internal class MainFirstEventHandler : IFirstLevelProjectionEventHandler
{
    private readonly ILogger<MainFirstEventHandler> _Logger;
    private readonly IServiceProvider _ServiceProvider;

    /// <summary>
    ///     Initializes a new instance of <see cref="MainFirstEventHandler" />
    /// </summary>
    /// <param name="serviceProvider">
    ///     The provider to get the specified
    ///     <see cref="IFirstLevelProjectionEventHandler{TEvent}" />s.
    /// </param>
    /// <param name="logger">The logger to be used.</param>
    public MainFirstEventHandler(
        IServiceProvider serviceProvider,
        ILogger<MainFirstEventHandler> logger)
    {
        _ServiceProvider = serviceProvider;
        _Logger = logger;
    }

    private Task HandleInternalAsync(
        IServiceProvider serviceProvider,
        IUserProfileServiceEvent domainEvent,
        StreamedEventHeader eventHeader,
        CancellationToken cancellationToken)
    {
        Type handlerType = typeof(IFirstLevelProjectionEventHandler<>).MakeGenericType(domainEvent.GetType());
        IFirstLevelProjectionEventHandler handlerToTrigger;

        try
        {
            handlerToTrigger = (IFirstLevelProjectionEventHandler)serviceProvider.GetRequiredService(handlerType);
        }
        catch (InvalidOperationException inEx)
        {
            throw new NotSupportedException(
                $"This domain event (full type: {domainEvent.GetType().FullName}) is not supported by this event handler.",
                inEx);
        }
        catch (Exception ex)
        {
            _Logger.LogErrorMessage(ex, "An unexpected error occurred.", LogHelpers.Arguments());

            throw;
        }

        return handlerToTrigger.HandleEventAsync(domainEvent, eventHeader, cancellationToken);
    }

    /// <inheritdoc />
    public async Task HandleEventAsync(
        IUserProfileServiceEvent domainEvent,
        StreamedEventHeader eventHeader,
        CancellationToken cancellationToken = default)
    {
        _Logger.EnterMethod();

        if (domainEvent == null)
        {
            throw new ArgumentNullException(nameof(domainEvent));
        }

        if (eventHeader == null)
        {
            throw new ArgumentNullException(nameof(eventHeader));
        }

        if (string.IsNullOrEmpty(domainEvent.EventId))
        {
            throw new InvalidDomainEventException(
                "The domain event is not valid. Event id is missing.",
                domainEvent);
        }

        if (string.IsNullOrEmpty(domainEvent.Type))
        {
            throw new InvalidDomainEventException(
                "The domain event is not valid. Event type is missing.",
                domainEvent);
        }

        if (_Logger.IsEnabledForTrace())
        {
            _Logger.LogTraceMessage(
                "Incoming event: {domainEvent}\n, Incoming event as stream event: {eventHeader}",
                LogHelpers.Arguments(domainEvent.ToLogString(), eventHeader.ToLogString()));
        }

        using IServiceScope serviceScope = _ServiceProvider.CreateScope();
        await HandleInternalAsync(serviceScope.ServiceProvider, domainEvent, eventHeader, cancellationToken);

        _Logger.LogInfoMessage(
            "Event (id: {domainEventId}; type: {domainEventType}) processed.",
            LogHelpers.Arguments(domainEvent.EventId, domainEvent.Type));

        _Logger.ExitMethod();
    }
}
