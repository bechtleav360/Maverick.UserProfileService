using System;
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
using UserProfileService.Sync.Projection.Abstractions;

namespace UserProfileService.Sync.Projection.Handlers;

internal class MainSyncEventHandler : ISyncProjectionEventHandler
{
    private readonly ILogger<MainSyncEventHandler> _logger;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    ///     Initializes a new instance of <see cref="ISyncProjectionEventHandler{TEvent}" />
    /// </summary>
    /// <param name="serviceProvider">
    ///     The provider to get the specified
    ///     <see cref="MainSyncEventHandler" />s.
    /// </param>
    /// <param name="logger">The logger to be used.</param>
    public MainSyncEventHandler(
        IServiceProvider serviceProvider,
        ILogger<MainSyncEventHandler> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    private static Task HandleInternalAsync(
        IServiceProvider services,
        IUserProfileServiceEvent domainEvent,
        StreamedEventHeader eventHeader,
        CancellationToken cancellationToken)
    {
        return domainEvent switch
        {
            UserCreated userCreated => services.GetRequiredService<ISyncProjectionEventHandler<UserCreated>>()
                .HandleEventAsync(
                    userCreated,
                    eventHeader,
                    cancellationToken),
            EntityDeleted entityDeleted => services
                .GetRequiredService<
                    ISyncProjectionEventHandler<
                        EntityDeleted>>()
                .HandleEventAsync(
                    entityDeleted,
                    eventHeader,
                    cancellationToken),

            PropertiesChanged propertyChanged => services
                .GetRequiredService<
                    ISyncProjectionEventHandler<
                        PropertiesChanged>>()
                .HandleEventAsync(
                    propertyChanged,
                    eventHeader,
                    cancellationToken),

            GroupCreated groupCreated => services
                .GetRequiredService<
                    ISyncProjectionEventHandler<
                        GroupCreated>>()
                .HandleEventAsync(
                    groupCreated,
                    eventHeader,
                    cancellationToken),
            ContainerDeleted containerDeleted => services
                .GetRequiredService<
                    ISyncProjectionEventHandler<
                        ContainerDeleted>>()
                .HandleEventAsync(
                    containerDeleted,
                    eventHeader,
                    cancellationToken),

            FunctionCreated functionCreated => services
                .GetRequiredService<
                    ISyncProjectionEventHandler<
                        FunctionCreated>>()
                .HandleEventAsync(
                    functionCreated,
                    eventHeader,
                    cancellationToken),
            MemberAdded memberAdded => services
                .GetRequiredService<
                    ISyncProjectionEventHandler<
                        MemberAdded>>()
                .HandleEventAsync(
                    memberAdded,
                    eventHeader,
                    cancellationToken),

            MemberDeleted memberDeleted => services
                .GetRequiredService<
                    ISyncProjectionEventHandler<
                        MemberDeleted>>()
                .HandleEventAsync(
                    memberDeleted,
                    eventHeader,
                    cancellationToken),

            MemberRemoved memberRemoved => services
                .GetRequiredService<
                    ISyncProjectionEventHandler<
                        MemberRemoved>>()
                .HandleEventAsync(
                    memberRemoved,
                    eventHeader,
                    cancellationToken),

            OrganizationCreated organizationCreated => services
                .GetRequiredService<
                    ISyncProjectionEventHandler<
                        OrganizationCreated>>()
                .HandleEventAsync(
                    organizationCreated,
                    eventHeader,
                    cancellationToken),

            RoleCreated roleCreated => services.GetRequiredService<ISyncProjectionEventHandler<RoleCreated>>()
                .HandleEventAsync(
                    roleCreated,
                    eventHeader,
                    cancellationToken),

            WasAssignedToOrganization wasAssignedToOrganization => services
                .GetRequiredService<ISyncProjectionEventHandler<WasAssignedToOrganization>>()
                .HandleEventAsync(
                    wasAssignedToOrganization,
                    eventHeader,
                    cancellationToken),

            WasUnassignedFrom wasUnassignedFrom => services
                .GetRequiredService<ISyncProjectionEventHandler<WasUnassignedFrom>>()
                .HandleEventAsync(
                    wasUnassignedFrom,
                    eventHeader,
                    cancellationToken),

            _ => services
                .GetRequiredService<
                    ISyncProjectionEventHandler<
                        NoActionEvent>>()
                .HandleEventAsync(
                    new NoActionEvent(domainEvent),
                    eventHeader,
                    cancellationToken) // other event handler can be ignored.
        };
    }

    public async Task HandleEventAsync(
        IUserProfileServiceEvent domainEvent,
        StreamedEventHeader eventHeader,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

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

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage(
                "Incoming event: {domainEvent}\n, Incoming event as stream event: {eventHeader}",
                LogHelpers.Arguments(domainEvent.ToLogString(), eventHeader.ToLogString()));
        }

        using IServiceScope serviceScope = _serviceProvider.CreateScope();
        await HandleInternalAsync(serviceScope.ServiceProvider, domainEvent, eventHeader, cancellationToken);

        _logger.LogInfoMessage(
            "Event (id: {domainEventId}; type: {domainEventType}) processed.",
            LogHelpers.Arguments(domainEvent.EventId, domainEvent.Type));

        _logger.ExitMethod();
    }
}
