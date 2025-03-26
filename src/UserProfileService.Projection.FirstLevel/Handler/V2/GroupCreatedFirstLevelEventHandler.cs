using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserProfileService.Common;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Common.V2.Extensions;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.FirstLevel.Abstractions;
using UserProfileService.Projection.FirstLevel.Extensions;
using UserProfileService.Validation.Abstractions.Configuration;

namespace UserProfileService.Projection.FirstLevel.Handler.V2;

/// <summary>
///     This handler is used to process <see cref="OrganizationCreatedEvent" />.
/// </summary>
internal class GroupCreatedFirstLevelEventHandler : FirstLevelEventHandlerTagsIncludedBase<GroupCreatedEvent>
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _propertyLocks = new();
    private readonly ValidationConfiguration _validationConfiguration;

    /// <summary>
    ///     Creates an instance of the object <see cref="GroupCreatedFirstLevelEventHandler" />.
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
    /// <param name="validationConfiguration"> <see cref="ValidationConfiguration"/></param>
    /// <param name="sagaService">
    ///     The saga service is used to write all created <see cref="IUserProfileServiceEvent" /> to the
    ///     write stream.
    /// </param>
    /// <param name="creator">The creator is used to create <inheritdoc cref="EventTuple" /> from the given parameter.</param>
    public GroupCreatedFirstLevelEventHandler(
        ILogger<GroupCreatedFirstLevelEventHandler> logger,
        IFirstLevelProjectionRepository repository,
        ISagaService sagaService,
        IFirstLevelEventTupleCreator creator,
        IOptions<ValidationConfiguration> validationConfiguration,
        IMapper mapper) : base(
        logger,
        repository,
        sagaService,
        mapper,
        creator)
    {
        _validationConfiguration = validationConfiguration.Value;
    }

    protected override async Task HandleInternalAsync(
        GroupCreatedEvent eventObject,
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

        var firstLevelProjectionGroup = Mapper.Map<FirstLevelProjectionGroup>(eventObject);
        var createdGroup = Mapper.Map<GroupCreated>(eventObject);

        string externalId = firstLevelProjectionGroup.ExternalIds.FirstOrDefaultUnconverted()?.Id;
        string name = firstLevelProjectionGroup.Name;
        string displayName = firstLevelProjectionGroup.DisplayName;

        string semaphoreKey = string.IsNullOrWhiteSpace(externalId)
            ? (string.IsNullOrWhiteSpace(name) ? displayName : name)
            : externalId;

        bool ignoreCase = _validationConfiguration.Internal.Group.Name.IgnoreCase;
        bool duplicateGroupNameAllowed = _validationConfiguration.Internal.Group.Name.Duplicate;

        Logger.LogDebugMessage(
            "Trying to create group with parameter: {eId},{name},{displayName},ignoreCase: {iCase}",
            LogHelpers.Arguments(externalId, name, displayName, ignoreCase));

        SemaphoreSlim semaphore = null;

        if (!duplicateGroupNameAllowed)
        {
            semaphore = _propertyLocks.GetOrAdd(
                semaphoreKey,
                _ => new SemaphoreSlim(1, 1));

            await semaphore.WaitAsync(cancellationToken);

            Logger.LogDebugMessage("Entered semaphore for key: {property}", LogHelpers.Arguments(semaphoreKey));

        }
        
        try
        {
            bool groupExist = await Repository.GroupExistAsync(
                externalId,
                name,
                displayName,
                ignoreCase,
                cancellationToken);

            if (groupExist && !duplicateGroupNameAllowed)
            {
                Logger.LogWarnMessage(
                    "The group with the external Id: {extId} and displayName: {d}, name: {n} already exist and can not be created",
                    LogHelpers.Arguments(
                        firstLevelProjectionGroup.ExternalIds.FirstOrDefaultUnconverted()?.Id,
                        firstLevelProjectionGroup.DisplayName,
                        firstLevelProjectionGroup.Name));

                throw new AlreadyExistsException(
                    $"The profile with the external Id: {externalId} and displayName: {displayName} and name: {name} already exist and can not be created");
            }
            else if (groupExist)
            {
                Logger.LogWarnMessage(
                    "The group with the displayName: {d}, name: {n} and external Id: {extId} already exist but will created again, DUPLICATED GROUP NAME are allowed ",
                    LogHelpers.Arguments(
                        firstLevelProjectionGroup.DisplayName,
                        firstLevelProjectionGroup.Name,
                        firstLevelProjectionGroup.ExternalIds.FirstOrDefaultUnconverted()?.Id));
            }

            await Repository.CreateProfileAsync(firstLevelProjectionGroup, transaction, cancellationToken);

            Guid batchId = await SagaService.CreateBatchAsync(
                cancellationToken,
                Creator.CreateEvents(
                        new ObjectIdent(eventObject.Payload.Id, ObjectType.Group),
                        new[] { createdGroup },
                        eventObject)
                    .ToArray());

            await CreateTagsAddedEvent(
                transaction,
                batchId,
                eventObject.Payload.Tags.ToList(),
                async (repo, tag) =>
                    await repo.AddTagToProfileAsync(
                        tag,
                        eventObject.Payload.Id,
                        transaction,
                        cancellationToken),
                eventObject,
                firstLevelProjectionGroup.ToObjectIdent(),
                cancellationToken);

            await CreateProfileAssignmentsAsync(
                transaction,
                firstLevelProjectionGroup.ToObjectIdent(),
                eventObject.Payload.Members,
                batchId,
                eventObject,
                eventObject.Payload.Id,
                eventObject.Payload.Tags.Where(tag => tag.IsInheritable).ToList(),
                cancellationToken);

            await SagaService.ExecuteBatchAsync(batchId, cancellationToken);
        }
        finally
        {
            if (!duplicateGroupNameAllowed)
            {
                semaphore?.Release();
                Logger.LogDebugMessage("Left semaphore for key: {property}", LogHelpers.Arguments(semaphoreKey));
                _propertyLocks.Remove(semaphoreKey, out semaphore);
            }
        }

        
        Logger.ExitMethod();
    }
}
