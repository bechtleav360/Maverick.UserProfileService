using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.Logging;
using UserProfileService.Common;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.FirstLevel.Abstractions;
using UserProfileService.Projection.FirstLevel.Extensions;
using OrganizationCreatedResolvedEvent = Maverick.UserProfileService.AggregateEvents.Resolved.V1.OrganizationCreated;
using ResolvedInitiator = Maverick.UserProfileService.AggregateEvents.Common.EventInitiator;
using RequestTagAssignment = Maverick.UserProfileService.Models.RequestModels.TagAssignment;
using System.Collections.Concurrent;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Common.V2.Extensions;

namespace UserProfileService.Projection.FirstLevel.Handler.V2;

/// <summary>
///     This handler is used to process <see cref="OrganizationCreatedEvent" />.
/// </summary>
internal class OrganizationCreatedFirstLevelEventHandler : FirstLevelEventHandlerTagsIncludedBase<OrganizationCreatedEvent>
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _propertyLocks = new();

    /// <summary>
    ///     Creates an instance of the object <see cref="OrganizationCreatedFirstLevelEventHandler" />.
    /// </summary>
    /// <param name="mapper">The Mapper is used to map several existing events in new event with the right order.</param>
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
    public OrganizationCreatedFirstLevelEventHandler(
        ILogger<OrganizationCreatedFirstLevelEventHandler> logger,
        IFirstLevelProjectionRepository repository,
        ISagaService sagaService,
        IFirstLevelEventTupleCreator creator,
        IMapper mapper) : base(
        logger,
        repository,
        sagaService,
        mapper,
        creator)
    {
    }

    protected override async Task HandleInternalAsync(
        OrganizationCreatedEvent eventObject,
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
                "eventObject: {eventObject}, streamEvent: {streamEvent}",
                LogHelpers.Arguments(eventObject.ToLogString(), streamEvent.ToLogString()));
        }

        var organization = Mapper.Map<FirstLevelProjectionOrganization>(eventObject);

        string externalId = organization.ExternalIds.FirstOrDefaultUnconverted()?.Id;
        string name = organization.Name;
        string displayName = organization.DisplayName;

        string semaphoreKey = string.IsNullOrWhiteSpace(externalId)
            ? (string.IsNullOrWhiteSpace(name) ? displayName : name)
            : externalId;

        SemaphoreSlim semaphore = _propertyLocks.GetOrAdd(
            semaphoreKey,
            _ => new SemaphoreSlim(1, 1));

        Logger.LogDebugMessage("Trying to enter semaphore for key: {property}", LogHelpers.Arguments(semaphoreKey));

        await semaphore.WaitAsync(cancellationToken);

        try
        {
            Logger.LogDebugMessage("Entered semaphore for key: {property}", LogHelpers.Arguments(semaphoreKey));

            bool organizationExist = await Repository.OrganizationExistAsync(
                externalId,
                name,
                displayName, cancellationToken: cancellationToken);

            if (organizationExist)
            {

                Logger.LogWarnMessage(
                    "The organization with the external Id: {extId} and displayName: {d} already exist and can not be created",
                    LogHelpers.Arguments(
                        organization.ExternalIds.FirstOrDefaultUnconverted()?.Id,
                        organization.DisplayName));

                throw new AlreadyExistsException(
                    $"The organization with the external Id: {externalId} and displayName: {displayName} already exist and can not be created");
            }

            await Repository.CreateProfileAsync(organization, transaction, cancellationToken);

            var organizationCreatedResolvedEvent = Mapper.Map<OrganizationCreatedResolvedEvent>(eventObject);

            Guid batchSagaId = await SagaService.CreateBatchAsync(
                cancellationToken,
                Creator.CreateEvents(
                        new ObjectIdent(eventObject.Payload.Id, ObjectType.Organization),
                        new List<IUserProfileServiceEvent>
                        {
                            organizationCreatedResolvedEvent
                        },
                        eventObject)
                    .ToArray());

            await CreateTagsAddedEvent(
                transaction,
                batchSagaId,
                eventObject.Payload.Tags.ToList(),
                async (repo, tag) =>
                    await repo.AddTagToProfileAsync(
                        tag,
                        organization.Id,
                        transaction,
                        cancellationToken),
                eventObject,
                organization.ToObjectIdent(),
                cancellationToken);

            await CreateProfileAssignmentsAsync(
                transaction,
                organization.ToObjectIdent(),
                eventObject.Payload.Members,
                batchSagaId,
                eventObject,
                eventObject.Payload.Id,
                eventObject.Payload.Tags.Where(tag => tag.IsInheritable).ToList(),
                cancellationToken);

            await SagaService.ExecuteBatchAsync(batchSagaId, cancellationToken);
        }
        finally
        {
            semaphore.Release();
            Logger.LogDebugMessage("Left semaphore for key: {property}", LogHelpers.Arguments(semaphoreKey));
            _propertyLocks.Remove(semaphoreKey, out semaphore);
        }

        Logger.ExitMethod();
    }
}
