using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Informer.Abstraction;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.SecondLevel.Abstractions;

namespace UserProfileService.Projection.SecondLevel.Handlers;

/// <summary>
///     Processes an <see cref="RoleChanged" /> event.
/// </summary>
public class RoleChangedEventHandler : SecondLevelEventHandlerBase<RoleChanged>
{
    /// <summary>
    ///     Initializes a new instance of an <see cref="RoleChangedEventHandler" />.
    /// </summary>
    /// <param name="repository">The repository to be used.</param>
    /// <param name="mapper">The mapper that is used to map object to other objects.</param>
    /// <param name="streamNameResolver">The resolver that will convert from or to stream names.</param>
    /// <param name="messageInformer">
    ///     ///     The message informer is used to notify consumer (if present) when
    ///     an event message type appears.
    /// </param>
    /// <param name="logger">The logger to be used.</param>
    public RoleChangedEventHandler(
        ISecondLevelProjectionRepository repository,
        IMapper mapper,
        IStreamNameResolver streamNameResolver,
        IMessageInformer messageInformer,
        ILogger<RoleChangedEventHandler> logger) : base(repository, mapper, streamNameResolver, messageInformer, logger)
    {
    }

    /// <inheritdoc />
    protected override async Task HandleEventAsync(
        RoleChanged domainEvent,
        StreamedEventHeader eventHeader,
        ObjectIdent relatedEntityIdent,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (domainEvent == null)
        {
            throw new ArgumentNullException(nameof(domainEvent));
        }

        if (eventHeader == null)
        {
            throw new ArgumentNullException(nameof(eventHeader));
        }

        if (relatedEntityIdent == null)
        {
            throw new ArgumentNullException(nameof(relatedEntityIdent));
        }

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTraceMessage(
                "Input parameter domainEvent: {{domainEvent}}, streamEvent: {eventHeader}, relatedEntityIdent: {relatedEntityIdent}",
                LogHelpers.Arguments(
                    domainEvent.ToLogString(),
                    eventHeader.ToLogString(),
                    relatedEntityIdent.ToLogString()));
        }

        if (domainEvent.Context == PropertiesChangedContext.SecurityAssignments)
        {
            Logger.LogInfoMessage(
                "The context is {context} so the security-assignment of the entity with the id = {entity} will be updated.",
                LogHelpers.Arguments(domainEvent.Context.ToLogString(), relatedEntityIdent.ToLogString()));

            // in this special case only the property "name"
            // can be changed. But that can be changed in the future.
            // So pay attention when changing the ILinkedObject-Interface 
            var propertiesToChange = new Dictionary<string, object>
            {
                { nameof(ILinkedObject.Name), domainEvent.Role.Name }
            };

            Logger.LogInfoMessage(
                "The updated property is name: {updatedValue}",
                domainEvent.Role.Name.ToLogString().AsArgumentList());

            await ExecuteInsideTransactionAsync(
                (repo, t, ct)
                    => repo.TryUpdateLinkedObjectAsync(
                        relatedEntityIdent.Id,
                        domainEvent.Role.Id,
                        propertiesToChange,
                        t,
                        ct),
                eventHeader,
                cancellationToken);
        }

        Logger.ExitMethod();
    }
}
