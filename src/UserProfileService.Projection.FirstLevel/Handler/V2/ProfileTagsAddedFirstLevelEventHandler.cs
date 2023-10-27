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

namespace UserProfileService.Projection.FirstLevel.Handler.V2;

/// <summary>
///     This handler is used to process <see cref="ProfileTagsAddedEvent" />.
/// </summary>
internal class ProfileTagsAddedFirstLevelEventHandler : FirstLevelEventHandlerTagsIncludedBase<ProfileTagsAddedEvent>
{
    /// <summary>
    ///     Creates an instance of the object <see cref="ProfileTagsAddedFirstLevelEventHandler" />.
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
    /// <param name="creator">The creator is used to create <inheritdoc cref="EventTuple" /> from the given parameter.</param>
    /// <param name="mapper">The Mapper is used to map several existing events in new event with the right order.</param>
    public ProfileTagsAddedFirstLevelEventHandler(
        ILogger<ProfileTagsAddedFirstLevelEventHandler> logger,
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
        ProfileTagsAddedEvent eventObject,
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

        // Includes null check for payload.
        if (string.IsNullOrWhiteSpace(eventObject.Payload?.Id))
        {
            throw new ArgumentException(
                "The profile id taken from the payload of the event can not be null or empty",
                nameof(eventObject));
        }

        if (eventObject.Payload.Tags == null || !eventObject.Payload.Tags.Any())
        {
            throw new ArgumentException(
                "The tag assignments taken from the payload of the event can not be null or empty",
                nameof(eventObject));
        }

        IFirstLevelProjectionProfile profileToAddTagsTo = await Repository.GetProfileAsync(
            eventObject.Payload.Id,
            transaction,
            cancellationToken);

        Logger.LogInfoMessage(
            "Start handling event to add tags to profile with profile {profile}",
            profileToAddTagsTo.ToLogString().AsArgumentList());

        Guid batchId = await SagaService.CreateBatchAsync(cancellationToken);

        Logger.LogDebugMessage(
            "Created batch with id {batchId} for event with id {eventId}",
            LogHelpers.Arguments(batchId, eventObject.EventId));

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
            profileToAddTagsTo.ToObjectIdent(),
            cancellationToken);

        if (eventObject.Payload.Tags.Any(t => t.IsInheritable))
        {
            Logger.LogDebugMessage(
                "Found inheritable tags to add to child elements. Try to get all child elements of profile with id {id}.",
                LogHelpers.Arguments(profileToAddTagsTo.Id));

            IList<FirstLevelRelationProfile> childElementsOfMember = await ExecuteSafelyAsync(
                () => Repository.GetAllChildrenAsync(
                    profileToAddTagsTo.ToObjectIdent(),
                    transaction,
                    cancellationToken),
                new List<FirstLevelRelationProfile>());

            if (childElementsOfMember.Any())
            {
                Logger.LogDebugMessage(
                    "Found {count} child elements for profile with id {id}",
                    LogHelpers.Arguments(childElementsOfMember.Count, profileToAddTagsTo.Id));

                if (Logger.IsEnabledForTrace())
                {
                    string childElementIdsAsStr = string.Join(
                        " , ",
                        childElementsOfMember.Select(t => t.Profile.Id));

                    Logger.LogTraceMessage(
                        "Found {count} child elements for profile with id {id}. Ids: {children}",
                        LogHelpers.Arguments(
                            childElementsOfMember.Count,
                            profileToAddTagsTo.Id,
                            childElementIdsAsStr));
                }

                await AddInheritableTagsToProfiles(
                    transaction,
                    profileToAddTagsTo.ToObjectIdent(),
                    batchId,
                    eventObject,
                    childElementsOfMember.Select(pr => pr.Profile).ToList(),
                    eventObject.Payload.Tags,
                    cancellationToken);
            }
            else
            {
                Logger.LogDebugMessage(
                    "No child elements found to add to add tags to for profile with id {id}.",
                    LogHelpers.Arguments(profileToAddTagsTo.Id));
            }
        }
        else
        {
            Logger.LogDebugMessage("No inheritable tags found to add to children.", LogHelpers.Arguments());
        }

        await SagaService.ExecuteBatchAsync(batchId, cancellationToken);

        Logger.LogInfoMessage(
            "Finished handling event to add tags to profile {profile}",
            profileToAddTagsTo.ToLogString().AsArgumentList());

        Logger.ExitMethod();
    }
}
