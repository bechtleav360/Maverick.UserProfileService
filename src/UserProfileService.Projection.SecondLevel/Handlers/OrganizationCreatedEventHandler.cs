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
using UserProfileService.Messaging.Abstractions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.SecondLevel.Abstractions;

namespace UserProfileService.Projection.SecondLevel.Handlers;

/// <summary>
///     Processes an <see cref="GroupCreated" /> event.
/// </summary>
internal class OrganizationCreatedEventHandler : SecondLevelEventHandlerBase<OrganizationCreated>
{
    /// <summary>
    ///     Initializes a new instance of an <see cref="OrganizationCreatedEventHandler" />.
    /// </summary>
    /// <param name="repository">The repository to be used.</param>
    /// <param name="mapper"></param>
    /// <param name="streamNameResolver">The resolver that will convert from or to stream names.</param>
    /// <param name="messageInformer">
    ///     The message informer is used to notify consumer (if present) when
    ///     an event message type appears.
    /// </param>
    /// <param name="logger">the logger to be used.</param>
    public OrganizationCreatedEventHandler(
        ISecondLevelProjectionRepository repository,
        IMapper mapper,
        IStreamNameResolver streamNameResolver,
        IMessageInformer messageInformer,
        // ReSharper disable once SuggestBaseTypeForParameterInConstructor
        ILogger<OrganizationCreatedEventHandler> logger) : base(repository, mapper, streamNameResolver, messageInformer, logger)
    {
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException">The <paramref name="domainEvent" /> is <c>null</c>.</exception>
    protected override async Task HandleEventAsync(
        OrganizationCreated domainEvent,
        StreamedEventHeader eventHeader,
        ObjectIdent relatedEntityIdent,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (domainEvent == null)
        {
            throw new ArgumentNullException(nameof(domainEvent));
        }

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTraceMessage(
                "Input parameter domainEvent: {domainEvent}",
                LogHelpers.Arguments(domainEvent.ToLogString()));
        }

        var organization =
            Mapper.Map<SecondLevelProjectionOrganization>(domainEvent);

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogDebugMessage(
                "Storing organization: {profile}",
                LogHelpers.Arguments(organization.ToLogString()));
        }
        else
        {
            Logger.LogInfoMessage(
                "Storing organization profile (Id = {profileId})",
                LogHelpers.Arguments(organization.Id));
        }

        await ExecuteInsideTransactionAsync(
            (repo, t, ct)
                => repo.CreateProfileAsync(
                    organization,
                    t,
                    ct),
            eventHeader,
            cancellationToken);

        Logger.ExitMethod();
    }
}
