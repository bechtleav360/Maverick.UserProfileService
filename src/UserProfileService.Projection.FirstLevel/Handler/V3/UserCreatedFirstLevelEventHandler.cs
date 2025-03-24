using System;
using System.Collections.Generic;
using System.Linq;
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
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.FirstLevel.Abstractions;
using UserProfileService.Projection.FirstLevel.Extensions;
using UserCreatedEventResolved = Maverick.UserProfileService.AggregateEvents.Resolved.V1.UserCreated;
using UserCreatedEventV3 = UserProfileService.Events.Implementation.V3.UserCreatedEvent;
using ResolvedInitiator = Maverick.UserProfileService.AggregateEvents.Common.EventInitiator;
using TagResolved = Maverick.UserProfileService.AggregateEvents.Common.Models.Tag;
using TagAssignmentResolved = Maverick.UserProfileService.AggregateEvents.Common.Models.TagAssignment;
using TagAddedResolved = Maverick.UserProfileService.AggregateEvents.Resolved.V1.TagsAdded;
using System.Collections.Concurrent;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Common.V2.Extensions;

namespace UserProfileService.Projection.FirstLevel.Handler.V3;

/// <summary>
///     This handler is used to process <see cref="UserCreatedEvent" />.
/// </summary>
internal class UserCreatedFirstLevelEventHandler : FirstLevelEventHandlerTagsIncludedBase<UserCreatedEventV3>
{

    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _propertyLocks = new();

    /// <summary>
    ///     Creates an instance of the object <see cref="UserCreatedFirstLevelEventHandler" />.
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
    public UserCreatedFirstLevelEventHandler(
        ILogger<UserCreatedFirstLevelEventHandler> logger,
        IFirstLevelProjectionRepository repository,
        ISagaService sagaService,
        IFirstLevelEventTupleCreator creator,
        IMapper mapper) : base(logger, repository, sagaService, mapper, creator)
    {
    }

    protected override async Task HandleInternalAsync(
        UserCreatedEventV3 eventObject,
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

        // map new events
        var createdUserEvent = Mapper.Map<UserCreatedEventResolved>(eventObject);
        var firstLevelUser = Mapper.Map<FirstLevelProjectionUser>(eventObject);

        string externalId = firstLevelUser.ExternalIds.FirstOrDefaultUnconverted()?.Id;
        string displayName = firstLevelUser.DisplayName;
        string email = firstLevelUser.Email;
        string semaphoreKey = externalId ?? displayName ?? email;


        SemaphoreSlim semaphore = _propertyLocks.GetOrAdd(
            semaphoreKey,
            _ => new SemaphoreSlim(1, 1));

        Logger.LogDebugMessage("Trying to enter semaphore for key: {property}", LogHelpers.Arguments(semaphoreKey));

        await semaphore.WaitAsync(cancellationToken);

        try
        {

            Logger.LogDebugMessage("Entered semaphore for key: {property}", LogHelpers.Arguments(semaphoreKey));

            bool userExist = await Repository.UserExistAsync(
             externalId,
               displayName,
               email, cancellationToken);

            if (userExist)
            {
                
                Logger.LogWarnMessage(
                    "The profile with the external Id: {extId} and displayName: {d} already exist and can not be created",
                    LogHelpers.Arguments(
                        firstLevelUser.ExternalIds.FirstOrDefaultUnconverted()?.Id,
                        firstLevelUser.DisplayName));

                throw new AlreadyExistsException(
                    $"The profile with the external Id: {externalId} and displayName: {displayName} already exist and can not be created");
            }

            Logger.LogInfoMessage(
                "The User with the external Id: {extId} doesn't exist and will be created",
                LogHelpers.Arguments(
                    firstLevelUser.ExternalIds.FirstOrDefaultUnconverted()?.Id));

            await Repository.CreateProfileAsync(firstLevelUser, transaction, cancellationToken);

            Guid batchId = await SagaService.CreateBatchAsync(
                cancellationToken,
                Creator.CreateEvents(
                        firstLevelUser.ToObjectIdent(),
                        new List<IUserProfileServiceEvent>
                        {
                            createdUserEvent
                        },
                        eventObject)
                    .ToArray());

            await CreateTagsAddedEvent(
                transaction,
                batchId,
                eventObject.Payload.Tags.ToList(),
                async (repo, tag) =>
                    await repo.AddTagToProfileAsync(
                        tag,
                        firstLevelUser.Id,
                        transaction,
                        cancellationToken),
                eventObject,
                firstLevelUser.ToObjectIdent(),
                cancellationToken);

            await SagaService.ExecuteBatchAsync(batchId, cancellationToken);
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
