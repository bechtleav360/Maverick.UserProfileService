#nullable enable
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Projection.SecondLevel.Abstractions;

namespace UserProfileService.Projection.SecondLevel;

/// <summary>
///     Contains the default implementation of a second-level-projection event handler. It will use dependency injection to
///     get event handlers.
/// </summary>
internal class MainSecondLevelEventHandler : ISecondLevelEventHandler
{
    private readonly ILogger<MainSecondLevelEventHandler> _Logger;
    private readonly IServiceProvider _ServiceProvider;

    /// <summary>
    ///     Initializes a new instance of <see cref="MainSecondLevelEventHandler" />
    /// </summary>
    /// <param name="serviceProvider">The provider to get the specified <see cref="ISecondLevelEventHandler{TEvent}" />s.</param>
    /// <param name="logger">The logger to be used.</param>
    public MainSecondLevelEventHandler(
        IServiceProvider serviceProvider,
        ILogger<MainSecondLevelEventHandler> logger)
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
        Type handlerType = typeof(ISecondLevelEventHandler<>).MakeGenericType(domainEvent.GetType());
        ISecondLevelEventHandler? handlerToTrigger;

        try
        {
            handlerToTrigger = (ISecondLevelEventHandler)serviceProvider.GetRequiredService(handlerType);
        }
        catch (InvalidOperationException inEx)
        {
            throw new NotSupportedException(
                $"This domain event (full type: {domainEvent.GetType().FullName}) is not supported by this event handler.");
        }
        catch (Exception ex)
        {
            _Logger.LogErrorMessage(ex, "An unexpected error occurred.", LogHelpers.Arguments());

            throw;
        }

        handlerToTrigger.HandleEventAsync(domainEvent, eventHeader, cancellationToken);

        return Task.CompletedTask;
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

        if (_Logger.IsEnabledForTrace())
        {
            _Logger.LogTraceMessage(
                "Incoming event: {domainEvent}",
                domainEvent.ToLogString().AsArgumentList());
        }
        else
        {
            _Logger.LogDebugMessage(
                "Incoming event: id: {domainEventId}; type: {domainEventType}",
                LogHelpers.Arguments(domainEvent.EventId, domainEvent.Type));
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

        using IServiceScope serviceScope = _ServiceProvider.CreateScope();
        await HandleInternalAsync(serviceScope.ServiceProvider, domainEvent, eventHeader, cancellationToken);

        _Logger.LogInfoMessage(
            "Event (id: {domainEventId}; type: {domainEventType}) processed.",
            LogHelpers.Arguments(domainEvent.EventId, domainEvent.Type));

        _Logger.ExitMethod();
    }
}
